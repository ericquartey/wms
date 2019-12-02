using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class DepositAndPickUpTestViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineDepositAndPickupProcedureWebService machineDepositAndPickupProcedureWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly ISensorsService sensorsService;

        private readonly IBayManager bayManagerService;

        private int? completedCycles;

        private double? grossWeight;

        private int inputDelay;

        private int? inputRequiredCycles;

        private bool bayIsMultiPosition;

        private bool isExecutingProcedure;

        private bool isPositionDownSelected;

        private bool isPositionUpSelected;

        private LoadingUnit loadingUnitInBay;

        private DelegateCommand selectBayPositionDownCommand;

        private DelegateCommand selectBayPositionUpCommand;

        private BayPosition selectedBayPosition;

        private bool isStopping;

        private bool isWaitingForResponse;

        private IEnumerable<LoadingUnit> loadingUnits;

        private double? netWeight;

        private RepeatedTestProcedure procedureParameters;

        private SubscriptionToken sensorsToken;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private SubscriptionToken subscriptionToken;

        private double? tare;

        private int? totalCompletedCycles;

        private Bay bay;

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

        public bool BayIsMultiPosition
        {
            get => this.bayIsMultiPosition;
            set => this.SetProperty(ref this.bayIsMultiPosition, value);
        }

        public bool IsPositionDownSelected
        {
            get => this.isPositionDownSelected;
            set
            {
                if (this.SetProperty(ref this.isPositionDownSelected, value))
                {
                    this.IsPositionUpSelected = !this.IsPositionDownSelected;
                }
            }
        }

        public bool IsPositionUpSelected
        {
            get => this.isPositionUpSelected;
            set
            {
                if (this.SetProperty(ref this.isPositionUpSelected, value))
                {
                    this.IsPositionDownSelected = !this.IsPositionUpSelected;
                }
            }
        }

        public LoadingUnit LoadingUnitInBay
        {
            get => this.loadingUnitInBay;
            set => this.SetProperty(ref this.loadingUnitInBay, value);
        }

        public ICommand SelectBayPosition1Command =>
            this.selectBayPositionDownCommand
            ??
            (this.selectBayPositionDownCommand = new DelegateCommand(this.SelectBayPositionDown));

        public ICommand SelectBayPosition2Command =>
            this.selectBayPositionUpCommand
            ??
            (this.selectBayPositionUpCommand = new DelegateCommand(this.SelectBayPositionUp));

        public BayPosition SelectedBayPosition
        {
            get => this.selectedBayPosition;
            private set
            {
                if (this.SetProperty(ref this.selectedBayPosition, value))
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

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set
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

        private void SelectBayPositionDown()
        {
            this.IsPositionDownSelected = true;
            this.SelectedBayPosition = this.bay.Positions.Single(p => p.Height == this.bay.Positions.Min(pos => pos.Height));
        }

        private void SelectBayPositionUp()
        {
            this.IsPositionUpSelected = true;
            this.SelectedBayPosition = this.bay.Positions.Single(p => p.Height == this.bay.Positions.Max(pos => pos.Height));
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
                        this.Stopped();

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
                    this.Stopped();
                    break;
            }
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.RaiseCanExecuteChanged();
        }

        private void RaiseCanExecuteChanged()
        {
            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
        }

        private async Task StartAsync()
        {
            try
            {
                if (!this.Tare.HasValue)
                {
                    this.ShowNotification("Tara non inserita", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (this.Tare.Value <= 0)
                {
                    this.ShowNotification("Tara deve essere maggiore di 0", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (!this.NetWeight.HasValue)
                {
                    this.ShowNotification("Peso non inserito", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (this.NetWeight.Value <= 0)
                {
                    this.ShowNotification("Peso deve essere maggiore di 0", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if ((this.InputRequiredCycles.Value - this.CumulativePerformedCycles.Value) <= 0)
                {
                    this.ShowNotification("Total completed cycles are greater than required cycles.", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.isExecutingProcedure = true;
            }
            catch (Exception ex)
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
