using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public partial class SemiAutoMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineLoadingUnitsService machineLoadingUnitsService;

        private readonly IMachineSensorsService machineSensorsService;

        private readonly Sensors sensors = new Sensors();

        private readonly IMachineShuttersService shuttersService;

        private SubscriptionToken homingToken;

        private int? inputLoadingUnitCode;

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
            IMachineElevatorService machineElevatorService,
            IMachineCellsService machineCellsService,
            IMachineLoadingUnitsService machineLoadingUnitsService,
            IMachineSensorsService machineSensorsService,
            IMachineShuttersService shuttersService,
            IMachineCarouselService machineCarouselService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            if (machineElevatorService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorService));
            }

            if (machineCellsService is null)
            {
                throw new ArgumentNullException(nameof(machineCellsService));
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

            if (shuttersService is null)
            {
                throw new System.ArgumentNullException(nameof(shuttersService));
            }

            if (machineCarouselService is null)
            {
                throw new System.ArgumentNullException(nameof(machineCarouselService));
            }

            this.machineSensorsService = machineSensorsService;
            this.machineElevatorService = machineElevatorService;
            this.machineCellsService = machineCellsService;
            this.machineLoadingUnitsService = machineLoadingUnitsService;
            this.bayManagerService = bayManagerService;
            this.shuttersService = shuttersService;
            this.machineCarouselService = machineCarouselService;
            this.shutterSensors = new ShutterSensors(this.BayNumber);

            this.SelectBayPosition1();
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

        public bool IsShutterTwoSensors => this.bayManagerService.Bay.Shutter.Type == ShutterType.TwoSensors;

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

        public override async Task OnNavigatedAsync()
        {
            this.IsBackNavigationAllowed = true;

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
                    message =>
                        {
                            this.sensors.Update(message?.Data?.SensorsStates);
                            this.IsZeroChain = this.IsOneTonMachine ? this.sensors.ZeroPawlSensorOneK : this.sensors.ZeroPawlSensor;
                            this.shutterSensors.Update(message?.Data?.SensorsStates);
                            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
                            this.RaiseCanExecuteChanged();
                        },
                    ThreadOption.UIThread,
                    false);

            try
            {
                var sensorsStates = await this.machineSensorsService.GetAsync();

                this.sensors.Update(sensorsStates.ToArray());
                this.IsZeroChain = this.IsOneTonMachine ? this.sensors.ZeroPawlSensorOneK : this.sensors.ZeroPawlSensor;
                this.shutterSensors.Update(sensorsStates.ToArray());
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }

            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
            this.RaiseCanExecuteChanged();

            await this.RetrieveElevatorPositionAsync();

            await this.RetrieveProcedureParametersAsync();

            await this.RetrieveCellsAsync();

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
            if (message is null || message.Data is null)
            {
                return;
            }

            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                    {
                        this.ShowNotification(string.Empty);

                        if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Horizontal)
                        {
                            this.ElevatorHorizontalPosition = message.Data.CurrentPosition;
                        }

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationExecuting:
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
                        if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Horizontal)
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
            if (message is null || message.Data is null)
            {
                return;
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

        private void OnShutterPositionChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            if (message is null || message.Data is null)
            {
                return;
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

        private async Task RetrieveProcedureParametersAsync()
        {
            try
            {
                this.procedureParameters = await this.machineElevatorService.GetVerticalManualMovementsParametersAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
