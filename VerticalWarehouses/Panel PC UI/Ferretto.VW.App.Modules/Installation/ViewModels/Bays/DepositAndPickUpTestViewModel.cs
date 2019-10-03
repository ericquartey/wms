using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public partial class DepositAndPickUpTestViewModel : BaseMainViewModel, IDataErrorInfo
    {
        public enum DepositAndPickUpState
        {
            None,
            GotoBay,
            Deposit,
            GotoBayAdjusted,
            EndLoaded,
        }

        #region Fields

        private readonly IMachineLoadingUnitsService machineLoadingUnitsService;

        private readonly IMachineSensorsService machineSensorsService;

        private readonly Sensors sensors = new Sensors();

        private int? inputLoadingUnitCode;

        private bool isWaitingForResponse;

        private bool isZeroChain;

        private IEnumerable<LoadingUnit> loadingUnits;

        private SubscriptionToken sensorsToken;

        private SubscriptionToken subscriptionToken;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineSetupStatusService machineSetupStatusService;

        private readonly IMachineDepositAndPickupProcedureService machineDepositAndPickupProcedureService;

        private int? completedCycles;

        private int initialCycles;

        private int inputDelay;

        private int? inputRequiredCycles;

        private bool isExecutingProcedure;

        private SubscriptionToken receivedActionUpdateToken;

        private DelegateCommand resetCommand;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private int? totalCompletedCycles;

        #endregion

        #region Constructors

        public DepositAndPickUpTestViewModel(
            IEventAggregator eventAggregator,
            IMachineSetupStatusService machineSetupStatusService,
            IMachineDepositAndPickupProcedureService machineDepositPickupProcedure,
            IMachineElevatorService machineElevatorService,
            IMachineLoadingUnitsService machineLoadingUnitsService,
            IMachineSensorsService machineSensorsService,
            IMachineServiceService machineServiceService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (machineDepositPickupProcedure == null)
            {
                throw new ArgumentNullException(nameof(machineDepositPickupProcedure));
            }

            if (machineElevatorService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorService));
            }

            if (machineLoadingUnitsService is null)
            {
                throw new ArgumentNullException(nameof(machineLoadingUnitsService));
            }

            if (bayManagerService is null)
            {
                throw new ArgumentNullException(nameof(bayManagerService));
            }

            if (machineSensorsService is null)
            {
                throw new System.ArgumentNullException(nameof(machineSensorsService));
            }

            if (machineServiceService is null)
            {
                throw new System.ArgumentNullException(nameof(machineServiceService));
            }

            this.eventAggregator = eventAggregator;
            this.machineSensorsService = machineSensorsService;
            this.machineElevatorService = machineElevatorService;
            this.machineLoadingUnitsService = machineLoadingUnitsService;
            this.bayManagerService = bayManagerService;
            this.machineSetupStatusService = machineSetupStatusService;
            this.machineDepositAndPickupProcedureService = machineDepositPickupProcedure;
            this.inputDelay = 0;
            this.SelectBayPosition1();
        }

        #endregion

        #region Properties
        public int? CompletedCycles
        {
            get => this.completedCycles;
            private set => this.SetProperty(ref this.completedCycles, value);
        }

        public string Error => string.Join(
                Environment.NewLine,
                this[nameof(this.InputRequiredCycles)],
                this[nameof(this.InputDelay)]);

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

        public ICommand ResetCommand =>
                        this.resetCommand
                        ??
                        (this.resetCommand = new DelegateCommand(
                            async () => await this.ResetAsync(),
                            this.CanExecuteResetCommand));

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
                async () => await this.StopAsync(),
                this.CanExecuteStopCommand));

        public int? TotalCompletedCycles
        {
            get => this.totalCompletedCycles;
            private set => this.SetProperty(ref this.totalCompletedCycles, value);
        }


        public int? InputLoadingUnitCode
        {
            get => this.inputLoadingUnitCode;
            set
            {
                if (this.SetProperty(ref this.inputLoadingUnitCode, value)
                    &&
                    this.LoadingUnits != null)
                {
                    this.LoadingUnitInBay = value == null
                        ? null
                        : this.LoadingUnits.SingleOrDefault(l => l.Id == value);
                }
            }
        }

        public bool IsElevatorMoving =>
                this.IsElevatorMovingToBay
                || this.IsElevatorDisembarking
                || this.IsElevatorEmbarking
                || this.IsTuningChain;

        public bool IsOneTonMachine => this.bayManagerService.Identity.IsOneTonMachine;

        public bool IsWaitingForResponse
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

        public IEnumerable<LoadingUnit> LoadingUnits { get => this.loadingUnits; set => this.loadingUnits = value; }

        public Sensors Sensors => this.sensors;

        #endregion

        #region Methods


        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputDelay):
                        if (this.InputDelay < 0)
                        {
                            return "InputDelay must be strictly positive.";
                        }

                        break;

                    case nameof(this.InputRequiredCycles):
                        if (!this.InputRequiredCycles.HasValue)
                        {
                            return $"InputRequiredCycles is required.";
                        }

                        if (this.InputRequiredCycles.Value <= 0)
                        {
                            return "InputRequiredCycles must be strictly positive.";
                        }

                        break;
                }

                return null;
            }
        }

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

            if (this.receivedActionUpdateToken != null)
            {
                this.eventAggregator
                  .GetEvent<NotificationEventUI<PositioningMessageData>>()
                  .Unsubscribe(this.receivedActionUpdateToken);

                this.receivedActionUpdateToken = null;
            }
        }

        public override async Task OnNavigatedAsync()
        {
            this.CompletedCycles = 0;

            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;

            this.subscriptionToken = this.EventAggregator
              .GetEvent<NotificationEventUI<PositioningMessageData>>()
              .Subscribe(
                  async message => await this.OnElevatorPositionChanged(message),
                  ThreadOption.UIThread,
                  false);

            this.sensorsToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    message =>
                        {
                            this.sensors.Update(message?.Data?.SensorsStates);
                            this.IsZeroChain = this.IsOneTonMachine ? this.sensors.ZeroPawlSensorOneK : this.sensors.ZeroPawlSensor;
                            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
                            this.RaiseCanExecuteChanged();
                        },
                    ThreadOption.UIThread,
                    false);

            try
            {
                var sensorsStates = await this.machineSensorsService.GetAsync();

                this.sensors.Update(sensorsStates.ToArray());
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }

            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
            this.RaiseCanExecuteChanged();

            await this.GetCycleQtuantityAsync();

            await this.RetrieveElevatorPositionAsync();

            await this.RetrieveLoadingUnitsAsync();

            await base.OnNavigatedAsync();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            this.RetrieveElevatorPositionAsync();
        }

        public async Task RetrieveLoadingUnitsAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.loadingUnits = await this.machineLoadingUnitsService.GetAllAsync();
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

        protected override void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            base.OnMachineModeChanged(e);

            // reset all status if stop machine
            if (e.MachinePower == Services.Models.MachinePowerState.Unpowered)
            {
                this.IsElevatorMovingToBay = false;
                this.IsElevatorDisembarking = false;
                this.IsElevatorEmbarking = false;
                this.IsExecutingProcedure = false;
                this.IsTuningChain = false;
            }
        }

        private async Task OnElevatorPositionChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message is null || message.Data is null)
            {
                return;
            }

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.IsExecutingProcedure = true;
                    break;

                case MessageStatus.OperationExecuting:
                    {
                        if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Vertical)
                        {
                            this.ElevatorVerticalPosition = message.Data.CurrentPosition;
                        }
                        else if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Horizontal)
                        {
                            this.ElevatorHorizontalPosition = message.Data.CurrentPosition;
                        }

                        break;
                    }

                case MessageStatus.OperationEnd:
                    {
                        this.IsElevatorDisembarking = false;
                        this.IsElevatorEmbarking = false;
                        this.IsElevatorMovingToBay = false;
                        this.IsTuningChain = false;

                        await this.ExecuteNextStateAsync();

                        if (this.currentState == DepositAndPickUpState.EndLoaded)
                        {
                            await this.machineDepositAndPickupProcedureService.IncreaseCycleQuantityAsync();
                            this.CompletedCycles++;
                            this.TotalCompletedCycles = this.initialCycles + this.completedCycles;
                        }

                        break;
                    }

                case MessageStatus.OperationStop:
                case MessageStatus.OperationFaultStop:
                case MessageStatus.OperationRunningStop:
                    {
                        this.IsElevatorDisembarking = false;
                        this.IsElevatorEmbarking = false;
                        this.IsElevatorMovingToBay = false;
                        this.IsTuningChain = false;
                        this.IsExecutingProcedure = false;

                        this.ShowNotification(
                            VW.App.Resources.InstallationApp.ProcedureWasStopped,
                            Services.Models.NotificationSeverity.Warning);

                        break;
                    }

                case MessageStatus.OperationError:
                    this.IsExecutingProcedure = false;
                    break;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.tuningBayCommand?.RaiseCanExecuteChanged();
            this.tuningChainCommand?.RaiseCanExecuteChanged();
            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
            this.resetCommand.RaiseCanExecuteChanged();
        }

        public async Task GetCycleQtuantityAsync()
        {
            try
            {
                //this.InputRequiredCycles = await this.machineDepositAndPickupProcedureService.GetRequiredCycleQuantityAsync();

                await this.InitializeTotalCycles();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private bool CanExecuteResetCommand()
        {
            return !this.IsExecutingProcedure
                   &&
                   !this.IsWaitingForResponse;
        }

        private bool CanExecuteStartCommand()
        {
            return !this.IsExecutingProcedure
                && !this.IsWaitingForResponse
                && string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanExecuteStopCommand()
        {
            return this.IsExecutingProcedure
                && !this.IsWaitingForResponse;
        }

        private async Task InitializeTotalCycles()
        {
            this.TotalCompletedCycles = await this.machineDepositAndPickupProcedureService.GetCycleQuantityAsync();
        }

        private async Task ResetAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineDepositAndPickupProcedureService.ResetAsync();

                await this.GetCycleQtuantityAsync();

                this.CompletedCycles = 0;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.IsExecutingProcedure = false;
            }
        }

        private async Task StartAsync()
        {
            try
            {

                if ((this.InputRequiredCycles.Value - this.TotalCompletedCycles.Value) <= 0)
                {
                    this.ShowNotification("Total completed cycles are greater than required cycles.", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                await this.InitializeTotalCycles();

                this.isExecutingProcedure = true;

                await this.ExecuteNextStateAsync();
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

        private async Task ExecuteNextStateAsync()
        {
            switch (this.currentState)
            {
                case DepositAndPickUpState.None:
                    await this.MoveToBayHeightAsync();
                    break;
                case DepositAndPickUpState.GotoBay:
                    await this.StartMovementAsync();
                    break;
                case DepositAndPickUpState.Deposit:
                    await this.MoveToBayHeightAsync();
                    break;
                case DepositAndPickUpState.GotoBayAdjusted:
                    await this.StartMovementAsync();
                    break;
                case DepositAndPickUpState.EndLoaded:
                    await this.CheckStart();
                    break;
            }
        }

        private async Task StopAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                // TODO stop moving
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.IsExecutingProcedure = false;
            }
        }

        private async Task UpdateCompletion(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message is null)
            {
                return;
            }
        }

        #endregion
    }
}
