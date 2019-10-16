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
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public partial class LoadingUnitFromBayToCellViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly Sensors sensors = new Sensors();

        private int? cellId;

        private IEnumerable<Cell> cells;

        private bool isExecutingProcedure;

        private bool isStopping;

        private bool isWaitingForResponse;

        private bool isZeroChain;

        private int? loadingUnitId;

        private IEnumerable<LoadingUnit> loadingUnits;

        private SubscriptionToken sensorsToken;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public LoadingUnitFromBayToCellViewModel(
                IMachineDepositAndPickupProcedureWebService machineDepositPickupProcedure,
                IMachineElevatorWebService machineElevatorWebService,
                IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
                IMachineSensorsWebService machineSensorsWebService,
                IBayManager bayManagerService)
                : base(PresentationMode.Installer)
        {
            if (machineDepositPickupProcedure == null)
            {
                throw new ArgumentNullException(nameof(machineDepositPickupProcedure));
            }

            if (machineElevatorWebService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorWebService));
            }

            if (machineLoadingUnitsWebService is null)
            {
                throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            }

            if (bayManagerService is null)
            {
                throw new ArgumentNullException(nameof(bayManagerService));
            }

            if (machineSensorsWebService is null)
            {
                throw new ArgumentNullException(nameof(machineSensorsWebService));
            }

            this.machineSensorsWebService = machineSensorsWebService;
            this.machineElevatorWebService = machineElevatorWebService;
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService;
            this.bayManagerService = bayManagerService;
            this.cellId = 0;
            this.SelectBayPosition1();
        }

        #endregion

        #region Properties

        public int? CellId
        {
            get => this.cellId;
            set
            {
                if (this.SetProperty(ref this.cellId, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCellIdValid
        {
            get
            {
                if (!this.cellId.HasValue)
                {
                    return false;
                }

                return this.cells.Any(l => l.Id == this.cellId.Value);
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

        public bool IsLoadingUnitIdValid
        {
            get
            {
                if (!this.loadingUnitId.HasValue)
                {
                    return false;
                }

                return this.loadingUnits.Any(l => l.Id == this.loadingUnitId.Value);
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

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set
            {
                if (this.SetProperty(ref this.loadingUnitId, value))
                {
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

        protected IEnumerable<Cell> Cells
        {
            get => this.cells;
            private set
            {
                if (this.SetProperty(ref this.cells, value))
                {
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

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

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

            await this.RetrieveElevatorPositionAsync();

            await this.RetrieveLoadingUnitsAsync();

            await this.RetrieveCellsAsync();

            this.RaisePropertyChanged(nameof(this.LoadingUnitInBay));

            this.RaiseCanExecuteChanged();
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

        protected override void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            base.OnMachineModeChanged(e);

            // reset all status if stop machine
            if (e.MachinePower == Services.Models.MachinePowerState.Unpowered)
            {
                this.RestoreStates();
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

        private void Ended()
        {
            this.ShowNotification(
                VW.App.Resources.InstallationApp.ProcedureCompleted,
                Services.Models.NotificationSeverity.Success);
        }

        private MAS.AutomationService.Contracts.LoadingUnitDestination GetLoadingUnitSource()
        {
            if (this.bayManagerService.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                if (this.IsPosition1Selected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay1Up;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay1Down;
                }
            }

            if (this.bayManagerService.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                if (this.IsPosition1Selected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay2Up;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay2Down;
                }
            }

            if (this.bayManagerService.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                if (this.IsPosition1Selected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay3Up;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay3Down;
                }
            }

            return MAS.AutomationService.Contracts.LoadingUnitDestination.NoDestination;
        }

        private async Task InitializeSensors()
        {
            try
            {
                var sensorsStates = await this.machineSensorsWebService.GetAsync();

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

                        this.Ended();

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

        private void RestoreStates()
        {
            this.IsElevatorDisembarking = false;
            this.IsElevatorEmbarking = false;
            this.IsElevatorMovingToBay = false;
            this.IsExecutingProcedure = false;

            this.RaiseCanExecuteChanged();
        }

        private async Task RetrieveCellsAsync()
        {
            try
            {
                this.Cells = await this.machineCellsWebService.GetAllAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task StartAsync()
        {
            try
            {
                if (!this.IsLoadingUnitIdValid)
                {
                    this.ShowNotification("Id cassetto inserito non valido", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (this.IsCellIdValid)
                {
                    this.ShowNotification("Id cella inserita non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var source = this.GetLoadingUnitSource();

                if (source == MAS.AutomationService.Contracts.LoadingUnitDestination.NoDestination)
                {
                    this.ShowNotification("Tipo scelta sorgente non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                //await this.machineElevatorWebService.StartMovingSourceDestinationAsync(source, MAS.AutomationService.Contracts.LoadingUnitDestination.Cell, null, this.CellId.Value);

                this.IsExecutingProcedure = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void StartStop()
        {
            this.RestoreStates();
            this.IsStopping = true;
        }

        private void Stopped()
        {
            this.RestoreStates();
            this.ShowNotification(
                VW.App.Resources.InstallationApp.ProcedureWasStopped,
                Services.Models.NotificationSeverity.Warning);
        }

        #endregion
    }
}
