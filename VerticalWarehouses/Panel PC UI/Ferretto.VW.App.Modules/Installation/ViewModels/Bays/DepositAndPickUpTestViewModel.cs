using System;
using System.Collections.Generic;
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
    public partial class DepositAndPickUpTestViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly int? inputLoadingUnitCode;

        private readonly IMachineDepositAndPickupProcedureService machineDepositAndPickupProcedureService;

        private readonly IMachineLoadingUnitsService machineLoadingUnitsService;

        private readonly IMachineSensorsService machineSensorsService;

        private readonly IMachineSetupStatusService machineSetupStatusService;

        private readonly Sensors sensors = new Sensors();

        private int? completedCycles;

        private double? grossWeight;

        private int initialCycles;

        private int inputDelay;

        private int? inputRequiredCycles;

        private bool isExecutingProcedure;

        private bool isStopping;

        private bool isWaitingForResponse;

        private bool isZeroChain;

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
            IMachineSetupStatusService machineSetupStatusService,
            IMachineDepositAndPickupProcedureService machineDepositPickupProcedure,
            IMachineElevatorService machineElevatorService,
            IMachineLoadingUnitsService machineLoadingUnitsService,
            IMachineSensorsService machineSensorsService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
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
                throw new ArgumentNullException(nameof(machineSensorsService));
            }

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
                || this.IsElevatorDisembarking
                || this.IsElevatorEmbarking;

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

        public bool IsLoadingUnitInBay
        {
            get
            {
                if (this.bayManagerService.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayOne)
                {
                    return this.Sensors.LUPresentInBay1;
                }
                else if (this.bayManagerService.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayTwo)
                {
                    return this.Sensors.LUPresentInBay2;
                }
                else if (this.bayManagerService.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayThree)
                {
                    return this.Sensors.LUPresentInBay3;
                }

                return false;
            }
        }

        public bool IsLoadingUnitOnElevator => this.Sensors.LuPresentInMachineSideBay1 && this.Sensors.LuPresentInOperatorSideBay1;

        public bool IsOneTonMachine => this.bayManagerService.Identity.IsOneTonMachine;

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

        public Sensors Sensors => this.sensors;

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

        public int? TotalCompletedCycles
        {
            get => this.totalCompletedCycles;
            private set => this.SetProperty(ref this.totalCompletedCycles, value);
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
                this.procedureParameters = await this.machineDepositAndPickupProcedureService.GetParametersAsync();

                this.InputRequiredCycles = this.procedureParameters.RequiredCycles;
                this.TotalCompletedCycles = this.procedureParameters.PerformedCycles;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.CompletedCycles = 0;

            this.IsZeroChain = this.IsOneTonMachine ? this.sensors.ZeroPawlSensorOneK : this.sensors.ZeroPawlSensor;

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
                            this.RaisePropertyChanged(nameof(this.LoadingUnitInBay));
                            this.RaisePropertyChanged(nameof(this.IsLoadingUnitOnElevator));
                            this.RaisePropertyChanged(nameof(this.IsLoadingUnitInBay));
                            this.RaiseCanExecuteChanged();
                        },
                    ThreadOption.UIThread,
                    false);

            await this.InitializeSensors();

            await this.GetCycleQtuantityAsync();

            await this.RetrieveElevatorPositionAsync();

            await this.RetrieveLoadingUnitsAsync();

            this.RaisePropertyChanged(nameof(this.LoadingUnitInBay));

            this.RaiseCanExecuteChanged();
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
            }
        }

        private bool CanExecuteStartCommand()
        {
            return
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                this.IsLoadingUnitInBay
                &&
                !this.IsLoadingUnitOnElevator;
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

        private async Task InitializeSensors()
        {
            try
            {
                var sensorsStates = await this.machineSensorsService.GetAsync();

                this.sensors.Update(sensorsStates.ToArray());
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
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
                    this.RaiseCanExecuteChanged();

                    break;

                case MessageStatus.OperationExecuting:
                    {
                        if (message.Data.AxisMovement == Axis.Vertical)
                        {
                            this.ElevatorVerticalPosition = message?.Data?.CurrentPosition ?? this.ElevatorVerticalPosition;
                        }
                        else if (message.Data.AxisMovement == Axis.Horizontal)
                        {
                            this.ElevatorHorizontalPosition = message?.Data?.CurrentPosition ?? this.ElevatorHorizontalPosition;
                        }

                        break;
                    }

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
                            this.TotalCompletedCycles = await this.machineDepositAndPickupProcedureService.IncreasePerformedCyclesAsync();
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

                if ((this.InputRequiredCycles.Value - this.TotalCompletedCycles.Value) <= 0)
                {
                    this.ShowNotification("Total completed cycles are greater than required cycles.", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.currentState = DepositAndPickUpState.None;

                this.isExecutingProcedure = true;

                await this.ExecuteNextStateAsync();
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
            this.IsElevatorDisembarking = false;
            this.IsElevatorEmbarking = false;
            this.IsElevatorMovingToBay = false;
            this.IsExecutingProcedure = false;

            this.ShowNotification(
                VW.App.Resources.InstallationApp.ProcedureWasStopped,
                Services.Models.NotificationSeverity.Warning);
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
