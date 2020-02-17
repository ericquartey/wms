using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed partial class DepositAndPickUpTestViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineDepositAndPickupProcedureWebService machineDepositAndPickupProcedureWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly ISensorsService sensorsService;

        private Bay bay;

        private int? completedCycles;

        private double? grossWeight;

        private int inputDelay;

        private int? inputRequiredCycles;

        private bool isExecutingProcedure;

        private bool isStopping;

        private IEnumerable<LoadingUnit> loadingUnits;

        private double? netWeight;

        private RepeatedTestProcedure procedureParameters;

        private SubscriptionToken sensorsToken;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private SubscriptionToken subscriptionToken;

        private double? tare;

        private int? totalCompletedCycles;

        #endregion

        #region Constructors

        public DepositAndPickUpTestViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineDepositAndPickupProcedureWebService machineDepositPickupProcedure,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            ISensorsService sensorsService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            if (bayManagerService is null)
            {
                throw new
                    ArgumentNullException(nameof(bayManagerService));
            }

            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineDepositAndPickupProcedureWebService = machineDepositPickupProcedure ?? throw new ArgumentNullException(nameof(machineDepositPickupProcedure));
            this.inputDelay = 0;
        }

        #endregion

        #region Enums

        public enum DepositAndPickUpState
        {
            None,

            GotoBay,

            PickUp,

            GotoBayAdjusted,

            Deposit,

            EndLoaded,
        }

        #endregion

        #region Properties

        public int? CompletedCycles
        {
            get => this.completedCycles;
            set
            {
                if (this.SetProperty(ref this.completedCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? CumulativePerformedCycles
        {
            get => this.totalCompletedCycles;
            private set => this.SetProperty(ref this.totalCompletedCycles, value);
        }

        public double? GrossWeight
        {
            get => this.grossWeight;
            set => this.SetProperty(ref this.grossWeight, value);
        }

        public int InputDelay
        {
            get => this.inputDelay;
            set
            {
                if (this.SetProperty(ref this.inputDelay, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? InputRequiredCycles
        {
            get => this.inputRequiredCycles;
            set
            {
                if (this.SetProperty(ref this.inputRequiredCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMoving =>
            this.IsElevatorMovingToBay
            ||
            this.IsElevatorDisembarking
            ||
            this.IsElevatorEmbarking;

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            private set
            {
                if (this.SetProperty(ref this.isExecutingProcedure, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsStopping
        {
            get => this.isStopping;
            set => this.SetProperty(ref this.isStopping, value);
        }

        public override bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double? NetWeight
        {
            get => this.netWeight;
            set
            {
                if (this.SetProperty(ref this.netWeight, value))
                {
                    this.UpdateLoadingUnit();
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand StartCommand =>
                   this.startCommand
           ??
           (this.startCommand = new DelegateCommand(
               async () => await this.StartAsync(),
               this.CanExecuteStartCommand));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                this.StartStop,
                this.CanExecuteStopCommand));

        public double? Tare
        {
            get => this.tare;
            set
            {
                if (this.SetProperty(ref this.tare, value))
                {
                    this.UpdateLoadingUnit();
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.subscriptionToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }

            if (this.sensorsToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Unsubscribe(this.sensorsToken);

                this.sensorsToken = null;
            }
        }

        public async Task GetCycleQtuantityAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.procedureParameters = await this.machineDepositAndPickupProcedureWebService.GetParametersAsync();
                this.InputRequiredCycles = this.procedureParameters.RequiredCycles;
                this.CumulativePerformedCycles = this.procedureParameters.PerformedCycles;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.CompletedCycles = 0;

            this.bay = await this.bayManagerService.GetBayAsync();

            this.BayIsMultiPosition = this.bay.IsDouble;

            this.IsBackNavigationAllowed = true;

            this.SubscribeToEvents();

            await this.GetCycleQtuantityAsync();

            await this.RetrieveLoadingUnitsAsync();

            this.RaisePropertyChanged(nameof(this.LoadingUnitInBay));

            this.RaiseCanExecuteChanged();

            this.SelectBayPositionDown();
        }

        public async Task RetrieveLoadingUnitsAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.loadingUnits = await this.machineLoadingUnitsWebService.GetAllAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState != MachinePowerState.Powered)
            {
                this.IsElevatorMovingToBay = false;
                this.IsElevatorDisembarking = false;
                this.IsElevatorEmbarking = false;
                this.IsExecutingProcedure = false;
            }
        }

        private bool CanExecuteStartCommand()
        {
            return
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
#if CHECK_BAY_SENSOR
                &&
                this.sensorsService.IsLoadingUnitInBay
#endif
                &&
                !this.sensorsService.IsLoadingUnitOnElevator;
        }

        private bool CanExecuteStopCommand()
        {
            return
                this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        private async Task ExecuteNextStateAsync()
        {
            if (this.IsStopping)
            {
                this.IsStopping = false;
                this.IsExecutingProcedure = false;
                this.Stopped();
            }

            if (!this.IsExecutingProcedure)
            {
                return;
            }

            switch (this.currentState)
            {
                case DepositAndPickUpState.None:
                    await this.MoveToBayHeightAsync();
                    break;

                case DepositAndPickUpState.GotoBay:
                    await this.StartMovementAsync();
                    break;

                case DepositAndPickUpState.PickUp:
                    await this.Restart();
                    break;

                case DepositAndPickUpState.GotoBayAdjusted:
                    await this.StartMovementAsync();
                    break;

                case DepositAndPickUpState.Deposit:
                    await this.MoveToBayHeightAsync();
                    break;
            }
        }

        private async Task OnElevatorPositionChangedAsync(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.IsExecutingProcedure = true;
                    this.RaiseCanExecuteChanged();

                    break;

                case MessageStatus.OperationEnd:
                    {
                        if (!this.IsExecutingProcedure)
                        {
                            break;
                        }

                        this.IsElevatorDisembarking = false;
                        this.IsElevatorEmbarking = false;
                        this.IsElevatorMovingToBay = false;

                        if (this.currentState == DepositAndPickUpState.PickUp)
                        {
                            this.CumulativePerformedCycles = await this.machineDepositAndPickupProcedureWebService.IncreasePerformedCyclesAsync();
                            this.CompletedCycles++;
                        }

                        await this.ExecuteNextStateAsync();

                        break;
                    }

                case MessageStatus.OperationStop:
                case MessageStatus.OperationFaultStop:
                case MessageStatus.OperationRunningStop:
                    {
                        this.Stopped();

                        break;
                    }

                case MessageStatus.OperationError:
                    this.IsExecutingProcedure = false;

                    break;
            }
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
        }

        private async Task StartAsync()
        {
            try
            {
                if (!this.Tare.HasValue)
                {
                    this.ShowNotification(InstallationApp.TrayTareValueNotInserted, Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (this.Tare.Value <= 0)
                {
                    this.ShowNotification(InstallationApp.TrayTareValueMustBePositive, Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (!this.NetWeight.HasValue)
                {
                    this.ShowNotification(InstallationApp.WeightValueNotInserted, Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (this.NetWeight.Value <= 0)
                {
                    this.ShowNotification(InstallationApp.WeightValueMustBePositive, Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if ((this.InputRequiredCycles.Value - this.CumulativePerformedCycles.Value) <= 0)
                {
                    this.ShowNotification(InstallationApp.TotalCycleMoreThanRequired, Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.currentState = DepositAndPickUpState.None;

                this.isExecutingProcedure = true;

                await this.ExecuteNextStateAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        private void StartStop()
        {
            this.IsStopping = true;
        }

        private void Stopped()
        {
            this.IsElevatorDisembarking = false;
            this.IsElevatorEmbarking = false;
            this.IsElevatorMovingToBay = false;
            this.IsExecutingProcedure = false;

            this.ShowNotification(
                VW.App.Resources.InstallationApp.ProcedureWasStopped,
                Services.Models.NotificationSeverity.Warning);
        }

        private void SubscribeToEvents()
        {
            this.subscriptionToken = this.subscriptionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        async message => await this.OnElevatorPositionChangedAsync(message),
                        ThreadOption.UIThread,
                        false);

            this.sensorsToken = this.sensorsToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null);
        }

        private void UpdateLoadingUnit()
        {
            if (this.tare.HasValue
                &&
                this.tare > 0)
            {
                this.LoadingUnitInBay = new LoadingUnit();
                this.LoadingUnitInBay.Id = 1;
                this.LoadingUnitInBay.Tare = this.Tare.Value;
                this.GrossWeight = this.Tare + this.NetWeight;
            }
            else
            {
                this.LoadingUnitInBay = null;
                this.GrossWeight = 0;
            }

            this.RaisePropertyChanged(nameof(this.LoadingUnitInBay));
        }

        #endregion
    }
}
