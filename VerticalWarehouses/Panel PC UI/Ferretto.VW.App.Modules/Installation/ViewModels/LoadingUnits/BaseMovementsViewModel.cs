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
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public partial class BaseMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly Sensors sensors = new Sensors();

        private bool bayIsMultiPosition;

        private double? elevatorHorizontalPosition;

        private double? elevatorVerticalPosition;

        private bool isExecutingProcedure;

        private bool isPosition1Selected;

        private bool isPosition2Selected;

        private bool isShutterTwoSensors;

        private bool isStopping;

        private bool isWaitingForResponse;

        private bool isZeroChain;

        private int? loadingUnitId;

        private LoadingUnit loadingUnitInBay;

        private IEnumerable<LoadingUnit> loadingUnits;

        private SubscriptionToken moveLoadingUnitToken;

        private DelegateCommand selectBayPosition1Command;

        private DelegateCommand selectBayPosition2Command;

        private SubscriptionToken sensorsToken;

        private ShutterSensors shutterSensors;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public BaseMovementsViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
        }

        #endregion

        #region Properties

        public Bay Bay { get; private set; }

        public bool BayIsMultiPosition
        {
            get => this.bayIsMultiPosition;
            set => this.SetProperty(ref this.bayIsMultiPosition, value);
        }

        public IBayManager BayManagerService => this.bayManagerService;

        public Guid? CurrentMissionId { get; private set; }

        public double? ElevatorHorizontalPosition
        {
            get => this.elevatorHorizontalPosition;
            set => this.SetProperty(ref this.elevatorHorizontalPosition, value);
        }

        public double? ElevatorVerticalPosition
        {
            get => this.elevatorVerticalPosition;
            set => this.SetProperty(ref this.elevatorVerticalPosition, value);
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
                if (this.Bay is null)
                {
                    return false;
                }

                if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayOne)
                {
                    return this.Sensors.LUPresentInBay1;
                }
                else if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayTwo)
                {
                    return this.Sensors.LUPresentInBay2;
                }
                else if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayThree)
                {
                    return this.Sensors.LUPresentInBay3;
                }

                return false;
            }
        }

        public bool IsLoadingUnitOnElevator => this.Sensors.LuPresentInMachineSideBay1 && this.Sensors.LuPresentInOperatorSideBay1;

        public bool IsOneTonMachine => this.bayManagerService.Identity.IsOneTonMachine;

        public bool IsPosition1Selected
        {
            get => this.isPosition1Selected;
            set
            {
                if (this.SetProperty(ref this.isPosition1Selected, value))
                {
                    this.IsPosition2Selected = !this.isPosition1Selected;
                }
            }
        }

        public bool IsPosition2Selected
        {
            get => this.isPosition2Selected;
            set
            {
                if (this.SetProperty(ref this.isPosition2Selected, value) && value)
                {
                    this.IsPosition1Selected = !this.isPosition1Selected;
                }
            }
        }

        public bool IsShutterTwoSensors
        {
            get => this.isShutterTwoSensors;
            set => this.SetProperty(ref this.isShutterTwoSensors, value);
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

        public bool IsZeroChain
        {
            get => this.isZeroChain;
            set => this.SetProperty(ref this.isZeroChain, value);
        }

        public int? LoadingUnitCellId
        {
            get
            {
                if (!this.loadingUnitId.HasValue)
                {
                    return null;
                }

                return this.loadingUnits.FirstOrDefault(l => l.Id == this.loadingUnitId.Value)?.CellId;
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

        public LoadingUnit LoadingUnitInBay
        {
            get => this.loadingUnitInBay;
            set => this.SetProperty(ref this.loadingUnitInBay, value);
        }

        public IMachineLoadingUnitsWebService MachineLoadingUnitsWebService => this.machineLoadingUnitsWebService;

        public ICommand SelectBayPosition1Command =>
                        this.selectBayPosition1Command
                        ??
                        (this.selectBayPosition1Command = new DelegateCommand(this.SelectBayPosition1));

        public ICommand SelectBayPosition2Command =>
                        this.selectBayPosition2Command
                        ??
                        (this.selectBayPosition2Command = new DelegateCommand(this.SelectBayPosition2));

        public Sensors Sensors => this.sensors;

        public ShutterSensors ShutterSensors => this.shutterSensors;

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
                    async () => await this.StartStop(),
                    this.CanExecuteStopCommand));

        #endregion

        #region Methods

        public virtual bool CanExecuteStartCommand()
        {
            return
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        public override void Disappear()
        {
            base.Disappear();

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.subscriptionToken?.Dispose();
            this.subscriptionToken = null;

            this.sensorsToken?.Dispose();
            this.sensorsToken = null;
            */
        }

        public MAS.AutomationService.Contracts.LoadingUnitLocation GetLoadingUnitSource()
        {
            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                if (this.IsPosition1Selected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Up;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Down;
                }
            }

            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                if (this.IsPosition1Selected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Up;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Down;
                }
            }

            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                if (this.IsPosition1Selected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay3Up;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay3Down;
                }
            }

            return MAS.AutomationService.Contracts.LoadingUnitLocation.NoLocation;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsZeroChain = this.IsOneTonMachine
                ? this.sensors.ZeroPawlSensorOneK
                : this.sensors.ZeroPawlSensor;

            this.IsBackNavigationAllowed = true;

            this.SubscribeToEvents();

            await this.RetrieveElevatorPositionAsync();

            await this.RetrieveLoadingUnitsAsync();

            await this.InitializeSensors();

            this.RaisePropertyChanged(nameof(this.LoadingUnitInBay));
            this.RaisePropertyChanged(nameof(this.ShutterSensors));

            this.RaiseCanExecuteChanged();
        }

        public virtual void RaiseCanExecuteChanged()
        {
            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
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

        public virtual void SelectBayPosition1()
        {
            this.IsPosition1Selected = true;
        }

        public virtual void SelectBayPosition2()
        {
            this.IsPosition2Selected = true;
        }

        public virtual Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public virtual async Task StartStop()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.StopAsync(null, this.Bay.Number);

                this.IsStopping = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

            this.RestoreStates();
        }

        protected virtual void Ended()
        {
            this.RestoreStates();

            this.ShowNotification(
                VW.App.Resources.InstallationApp.ProcedureCompleted,
                Services.Models.NotificationSeverity.Success);
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState != MachinePowerState.Powered)
            {
                this.RestoreStates();
            }
        }

        protected virtual void OnWaitResume()
        {
        }

        private bool CanExecuteStopCommand()
        {
            return
                this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsStopping;
        }

        private async Task InitializeSensors()
        {
            try
            {
                this.Bay = await this.bayManagerService.GetBayAsync();

                this.BayIsMultiPosition = this.Bay.IsDouble;

                this.IsShutterTwoSensors = this.Bay.Shutter.Type == MAS.AutomationService.Contracts.ShutterType.TwoSensors;

                this.shutterSensors = new ShutterSensors((int)this.Bay.Number);

                var sensorsStates = await this.machineSensorsWebService.GetAsync();

                this.sensors.Update(sensorsStates.ToArray());
                this.ShutterSensors.Update(sensorsStates.ToArray());
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void OnElevatorPositionChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
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
            }
        }

        private void OnMoveLoadingUnitChanged(NotificationMessageUI<MoveLoadingUnitMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.IsExecutingProcedure = true;
                    this.RaiseCanExecuteChanged();

                    this.CurrentMissionId = (message.Data as MoveLoadingUnitMessageData).MissionId;

                    break;

                case MessageStatus.OperationWaitResume:
                    this.OnWaitResume();
                    break;

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

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStates);
            this.ShutterSensors.Update(message.Data.SensorsStates);

            this.IsZeroChain = this.IsOneTonMachine
                ? this.sensors.ZeroPawlSensorOneK
                : this.sensors.ZeroPawlSensor;

            this.RaisePropertyChanged(nameof(this.LoadingUnitInBay));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitOnElevator));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitInBay));
            this.RaiseCanExecuteChanged();
        }

        private void RestoreStates()
        {
            this.IsExecutingProcedure = false;

            this.RaiseCanExecuteChanged();
        }

        private async Task RetrieveElevatorPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.ElevatorVerticalPosition = await this.machineElevatorWebService.GetVerticalPositionAsync();
                this.ElevatorHorizontalPosition = await this.machineElevatorWebService.GetHorizontalPositionAsync();
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

        private void Stopped()
        {
            this.IsStopping = false;
            this.RestoreStates();

            this.ShowNotification(
                Resources.InstallationApp.ProcedureWasStopped,
                Services.Models.NotificationSeverity.Warning);
        }

        private void SubscribeToEvents()
        {
            this.subscriptionToken = this.subscriptionToken
               ??
               this.EventAggregator
                   .GetEvent<NotificationEventUI<PositioningMessageData>>()
                   .Subscribe(
                       this.OnElevatorPositionChanged,
                       ThreadOption.UIThread,
                       false);

            this.moveLoadingUnitToken = this.moveLoadingUnitToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                    .Subscribe(
                        this.OnMoveLoadingUnitChanged,
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

        #endregion
    }
}
