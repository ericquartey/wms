using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public partial class SemiAutoMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly Sensors sensors = new Sensors();

        private readonly IMachineShuttersWebService shuttersWebService;

        private Bay bay;

        private SubscriptionToken homingToken;

        private int? inputLoadingUnitCode;

        private bool isShutterTwoSensors;

        private bool isWaitingForResponse;

        private bool isZeroChain;

        private IEnumerable<LoadingUnit> loadingUnits;

        private VerticalManualMovementsProcedure procedureParameters;

        private SubscriptionToken sensorsToken;

        private SubscriptionToken shutterPositionToken;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public SemiAutoMovementsViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineCellsWebService machineCellsWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IMachineShuttersWebService shuttersWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            if (machineElevatorWebService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorWebService));
            }

            if (machineCellsWebService is null)
            {
                throw new ArgumentNullException(nameof(machineCellsWebService));
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
                throw new System.ArgumentNullException(nameof(machineSensorsWebService));
            }

            if (shuttersWebService is null)
            {
                throw new System.ArgumentNullException(nameof(shuttersWebService));
            }

            if (machineCarouselWebService is null)
            {
                throw new System.ArgumentNullException(nameof(machineCarouselWebService));
            }

            this.machineSensorsWebService = machineSensorsWebService;
            this.machineElevatorWebService = machineElevatorWebService;
            this.machineCellsWebService = machineCellsWebService;
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService;
            this.bayManagerService = bayManagerService;
            this.shuttersWebService = shuttersWebService;
            this.machineCarouselWebService = machineCarouselWebService;
            this.shutterSensors = new ShutterSensors(this.BayNumber);
        }

        #endregion

        #region Properties

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

        public bool IsMoving =>
                   this.IsElevatorMovingToCell
                || this.IsElevatorMovingToHeight
                || this.IsElevatorMovingToLoadingUnit
                || this.IsElevatorMovingToBay
                || this.IsElevatorDisembarking
                || this.IsElevatorEmbarking
                || this.IsTuningChain
                || this.IsTuningBay
                || this.IsCarouselMoving
                || this.IsShutterMoving;

        public bool IsOneTonMachine => this.bayManagerService.Identity.IsOneTonMachine;

        public bool IsShutterTwoSensors
        {
            get => this.isShutterTwoSensors;
            set => this.SetProperty(ref this.isShutterTwoSensors, value);
        }

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

        public override void Disappear()
        {
            base.Disappear();

            if (this.homingToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<HomingMessageData>>()
                    .Unsubscribe(this.homingToken);

                this.homingToken = null;
            }

            if (this.shutterPositionToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Unsubscribe(this.shutterPositionToken);

                this.shutterPositionToken = null;
            }

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
            this.IsBackNavigationAllowed = true;

            this.SubscribeToEvents();

            try
            {
                this.bay = await this.bayManagerService.GetBay();
                this.BayNumber = (int)this.bay.Number;
                this.HasCarousel = this.bay.Carousel != null;
                this.IsShutterTwoSensors = this.bay.Shutter.Type == ShutterType.TwoSensors;
                this.BayIsMultiPosition = this.bay.Positions.Count() > 1;

                var sensorsStates = await this.machineSensorsWebService.GetAsync();

                this.sensors.Update(sensorsStates.ToArray());
                this.IsZeroChain = this.IsOneTonMachine ? this.sensors.ZeroPawlSensorOneK : this.sensors.ZeroPawlSensor;
                this.shutterSensors.Update(sensorsStates.ToArray());

                this.SelectBayPosition1();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }

            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
            this.RaiseCanExecuteChanged();

            try
            {
                this.IsWaitingForResponse = true;

                this.ElevatorVerticalPosition = await this.machineElevatorWebService.GetVerticalPositionAsync();
                this.ElevatorHorizontalPosition = await this.machineElevatorWebService.GetHorizontalPositionAsync();

                this.BayChainHorizontalPosition = await this.machineCarouselWebService.GetPositionAsync();

                this.procedureParameters = await this.machineElevatorWebService.GetVerticalManualMovementsParametersAsync();

                this.Cells = await this.machineCellsWebService.GetAllAsync();

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

            await base.OnAppearedAsync();
        }

        protected override void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            base.OnMachineModeChanged(e);

            // reset all status if stop machine
            if (e.MachinePower == Services.Models.MachinePowerState.Unpowered)
            {
                this.IsElevatorMovingToCell = false;
                this.IsElevatorMovingToHeight = false;
                this.IsElevatorMovingToLoadingUnit = false;
                this.IsElevatorMovingToBay = false;
                this.IsElevatorDisembarking = false;
                this.IsElevatorEmbarking = false;
                this.IsTuningChain = false;
                this.IsCarouselMoving = false;
                this.IsTuningBay = false;
                this.IsShutterMoving = false;
            }
        }

        private void OnElevatorPositionChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Data is null)
            {
                throw new ArgumentException();
            }

            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                    {
                        this.ShowNotification(string.Empty);

                        if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Vertical)
                        {
                            this.ElevatorVerticalPosition = message?.Data?.CurrentPosition ?? this.ElevatorVerticalPosition;
                        }
                        else if (message.Data.MovementMode >= CommonUtils.Messages.Enumerations.MovementMode.BayChain)
                        {
                            this.BayChainHorizontalPosition = message?.Data?.CurrentPosition ?? this.BayChainHorizontalPosition;
                        }
                        else if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Horizontal)
                        {
                            this.ElevatorHorizontalPosition = message?.Data?.CurrentPosition ?? this.ElevatorHorizontalPosition;
                        }

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationExecuting:
                    {
                        if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Vertical)
                        {
                            this.ElevatorVerticalPosition = message?.Data?.CurrentPosition ?? this.ElevatorVerticalPosition;
                        }
                        else if (message.Data.MovementMode >= CommonUtils.Messages.Enumerations.MovementMode.BayChain)
                        {
                            this.BayChainHorizontalPosition = message?.Data?.CurrentPosition ?? this.BayChainHorizontalPosition;
                        }
                        else if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Horizontal)
                        {
                            this.ElevatorHorizontalPosition = message?.Data?.CurrentPosition ?? this.ElevatorHorizontalPosition;
                        }

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.IsElevatorDisembarking = false;
                        this.IsElevatorEmbarking = false;
                        this.IsElevatorMovingToCell = false;
                        this.IsElevatorMovingToHeight = false;
                        this.IsElevatorMovingToLoadingUnit = false;
                        this.IsElevatorMovingToBay = false;
                        this.IsTuningChain = false;
                        this.IsTuningBay = false;
                        if (message.Data.MovementMode == CommonUtils.Messages.Enumerations.MovementMode.BayChain)
                        {
                            this.IsCarouselMoving = false;
                        }

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.OperationWarningOrError(message.Status, message.Description);
                        break;
                    }
            }
        }

        private void OnHomingChanged(NotificationMessageUI<HomingMessageData> message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Data is null)
            {
                throw new ArgumentException();
            }

            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                    {
                        this.IsTuningChain = true;
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.IsTuningChain = false;
                        this.IsTuningBay = false;
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.OperationWarningOrError(message.Status, message.Description);
                        break;
                    }
            }
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            this.sensors.Update(message.Data?.SensorsStates);
            this.IsZeroChain = this.IsOneTonMachine ? this.sensors.ZeroPawlSensorOneK : this.sensors.ZeroPawlSensor;
            this.shutterSensors.Update(message.Data?.SensorsStates);
            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
            this.RaiseCanExecuteChanged();
        }

        private void OnShutterPositionChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Data is null)
            {
                throw new ArgumentException();
            }

            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                    {
                        this.IsShutterMoving = true;
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.IsShutterMoving = false;
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.OperationWarningOrError(message.Status, message.Description);
                        break;
                    }
            }
        }

        private void OperationWarningOrError(CommonUtils.Messages.Enumerations.MessageStatus status, string errorDescription)
        {
            this.IsElevatorDisembarking = false;
            this.IsElevatorEmbarking = false;
            this.IsElevatorMovingToCell = false;
            this.IsElevatorMovingToHeight = false;
            this.IsElevatorMovingToLoadingUnit = false;
            this.IsElevatorMovingToBay = false;
            this.IsTuningChain = false;
            this.IsCarouselMoving = false;
            this.IsShutterMoving = false;
            this.IsTuningBay = false;

            if (status == CommonUtils.Messages.Enumerations.MessageStatus.OperationError)
            {
                this.ShowNotification(
                    errorDescription,
                    Services.Models.NotificationSeverity.Error);
            }
            else
            {
                this.ShowNotification(
                    VW.App.Resources.InstallationApp.ProcedureWasStopped,
                    Services.Models.NotificationSeverity.Warning);
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.CanInputCellId = this.Cells != null
               &&
               !this.IsMoving
               &&
               !this.IsWaitingForResponse;

            this.CanInputHeight = !this.IsMoving
               &&
               !this.IsWaitingForResponse;

            this.CanInputLoadingUnitId = this.LoadingUnits != null
               &&
               this.Cells != null
               &&
               !this.IsMoving
               &&
               !this.IsWaitingForResponse;

            this.moveToCellHeightCommand?.RaiseCanExecuteChanged();
            this.moveToHeightCommand?.RaiseCanExecuteChanged();
            this.moveToLoadingUnitHeightCommand?.RaiseCanExecuteChanged();
            this.tuningBayCommand?.RaiseCanExecuteChanged();
            this.tuningChainCommand?.RaiseCanExecuteChanged();
            this.embarkForwardsCommand?.RaiseCanExecuteChanged();
            this.embarkBackwardsCommand?.RaiseCanExecuteChanged();
            this.disembarkForwardsCommand?.RaiseCanExecuteChanged();
            this.disembarkBackwardsCommand?.RaiseCanExecuteChanged();
            this.moveToBayHeightCommand?.RaiseCanExecuteChanged();
            this.openShutterCommand?.RaiseCanExecuteChanged();
            this.intermediateShutterCommand?.RaiseCanExecuteChanged();
            this.closedShutterCommand?.RaiseCanExecuteChanged();
            this.carouselDownCommand?.RaiseCanExecuteChanged();
            this.carouselUpCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
        }

        private void SubscribeToEvents()
        {
            this.homingToken = this.EventAggregator
                .GetEvent<NotificationEventUI<HomingMessageData>>()
                .Subscribe(
                    message => this.OnHomingChanged(message),
                    ThreadOption.UIThread,
                    false);

            this.shutterPositionToken = this.EventAggregator
              .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
              .Subscribe(
                    message => this.OnShutterPositionChanged(message),
                    ThreadOption.UIThread,
                    false);

            this.subscriptionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                    message => this.OnElevatorPositionChanged(message),
                    ThreadOption.UIThread,
                    false);

            this.sensorsToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    this.OnSensorsChanged,
                    ThreadOption.UIThread,
                    false);
        }

        #endregion
    }
}
