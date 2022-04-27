using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.MovementsView)]
    internal sealed partial class MovementsViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly Services.IDialogService dialogService;

        private readonly IInstallationHubClient installationHubClient;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineExternalBayWebService machineExternalBayWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly IMachineService machineService;

        private readonly ISessionService sessionService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private readonly IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService;

        private SubscriptionToken cellsToken;

        private SubscriptionToken elevatorPositionChangedToken;

        private DelegateCommand goToMovementsGuidedCommand;

        private DelegateCommand goToMovementsManualCommand;

        private DelegateCommand goToOperatorCommand;

        private DelegateCommand goToStatusSensorsCommand;

        private SubscriptionToken homingToken;

        private bool isCarouselMoving;

        private bool isElevatorInBay;

        private bool isElevatorInCell;

        private bool isExecutingProcedure;

        private bool isExternalBayMoving;

        private bool isMovementsGuided = true;

        private SubscriptionToken loadunitsToken;

        private SubscriptionToken positioningOperationChangedToken;

        private DelegateCommand resetCommand;

        private SubscriptionToken shutterPositionToken;

        private DelegateCommand stopMovingCommand;

        private DelegateCommand stopMovingReleaseCommand;

        private string title;

        #endregion

        #region Constructors

        public MovementsViewModel(
            IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService,
            ISessionService sessionService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineCellsWebService machineCellsWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineShuttersWebService shuttersWebService,
            IMachineCarouselWebService machineCarouselWebService,
            Services.IDialogService dialogService,
            IMachineBaysWebService machineBaysWebService,
            IMachineMissionsWebService machineMissionsWebService,
            IBayManager bayManagerService,
            IInstallationHubClient installationHubClient,
            IMachineService machineService,
            IMachineExternalBayWebService machineExternalBayWebService)
            : base(PresentationMode.Installer)
        {
            this.verticalOriginProcedureWebService = verticalOriginProcedureWebService ?? throw new ArgumentNullException(nameof(verticalOriginProcedureWebService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
            this.installationHubClient = installationHubClient ?? throw new ArgumentNullException(nameof(installationHubClient));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.machineExternalBayWebService = machineExternalBayWebService ?? throw new ArgumentNullException(nameof(machineExternalBayWebService));
        }

        #endregion

        #region Properties

        public bool BayIsMultiPosition => this.MachineService.Bay.IsDouble;

        public override EnableMask EnableMask => EnableMask.Any;

        public string Error => string.Join(
            Environment.NewLine,
            this[nameof(this.InputLoadingUnitId)]);

        public ICommand GoToMovementsGuidedCommand =>
            this.goToMovementsGuidedCommand
            ??
            (this.goToMovementsGuidedCommand = new DelegateCommand(
                () => this.GoToMovementsExecuteCommand(true),
                this.CanGoToMovementsGuidedExecuteCommand
                ));

        public ICommand GoToMovementsManualCommand =>
            this.goToMovementsManualCommand
            ??
            (this.goToMovementsManualCommand = new DelegateCommand(
                () => this.GoToMovementsExecuteCommand(false),
                this.CanGoToMovementsManualExecuteCommand));

        public ICommand GoToOperatorCommand =>
            this.goToOperatorCommand
            ??
            (this.goToOperatorCommand = new DelegateCommand(
                () => this.GoToOperatorExecuteCommand(this.IsOperator),
                this.CanGoToMovementsManualExecuteCommand));

        public ICommand GoToStatusSensorsCommand =>
            this.goToStatusSensorsCommand
            ??
            (this.goToStatusSensorsCommand = new DelegateCommand(
                () => this.StatusSensorsCommand(),
                () => (this.HealthProbeService.HealthMasStatus == Services.HealthStatus.Healthy || this.HealthProbeService.HealthMasStatus == Services.HealthStatus.Degraded)));

        public bool HasBayExternal => this.MachineService.HasBayExternal;

        public bool HasBayExternalDouble => this.MachineService.HasBayExternal && this.MachineService.Bay.IsDouble;

        public bool HasCarousel => this.MachineService.HasCarousel;

        public bool HasShutter => this.MachineService.HasShutter;

        public bool IsCarouselMoving
        {
            get => this.isCarouselMoving;
            private set => this.SetProperty(ref this.isCarouselMoving, value, this.RaiseCanExecuteChanged);
        }

        public bool IsElevatorInBay
        {
            get => this.isElevatorInBay;
            private set => this.SetProperty(ref this.isElevatorInBay, value);
        }

        public bool IsElevatorInCell
        {
            get => this.isElevatorInCell;
            private set => this.SetProperty(ref this.isElevatorInCell, value);
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            set => this.SetProperty(ref this.isExecutingProcedure, value);
        }

        public bool IsExternalBayMoving
        {
            get => this.isExternalBayMoving;
            private set => this.SetProperty(ref this.isExternalBayMoving, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMovementsGuided => this.isMovementsGuided && !this.IsOperator;

        public bool IsMovementsManual => !this.isMovementsGuided && !this.IsOperator;

        public bool IsMovementsManualLimited => this.IsMovementsManual && this.sessionService.UserAccessLevel != UserAccessLevel.Movement;

        public bool IsOperator => this.sessionService.UserAccessLevel == UserAccessLevel.Operator;

        public override bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set => this.SetProperty(ref this.isWaitingForResponse, value);
        }

        public ICommand ResetCommand =>
            this.resetCommand
            ??
            (this.resetCommand = new DelegateCommand(
               async () => await this.ResetCommandAsync(),
               this.CanResetCommand));

        public ICommand StopMovingCommand =>
            this.stopMovingCommand
            ??
            (this.stopMovingCommand = new DelegateCommand(
                async () => await this.StopMovingAsync(),
                this.CanStopMoving));

        public ICommand StopMovingReleaseCommand =>
            this.stopMovingReleaseCommand
            ??
            (this.stopMovingReleaseCommand = new DelegateCommand(
                async () => await this.StopMovingReleaseAsync()));

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        private Bay Bay => this.MachineService.Bay;

        private IEnumerable<Cell> Cells => this.MachineService.Cells;

        private IEnumerable<LoadingUnit> LoadingUnits => this.MachineService.Loadunits;

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputLoadingUnitId):
                        if (this.InputLoadingUnitId.HasValue &&
                            (!this.MachineService.Loadunits.DrawerInLocationById(this.InputLoadingUnitId.Value) &&
                             !this.MachineService.Loadunits.DrawerInElevatorById(this.InputLoadingUnitId.Value) &&
                             !this.MachineService.Loadunits.DrawerInBayById(this.InputLoadingUnitId.Value))
                             )
                        {
                            return Localized.Get("InstallationApp.InvalidDrawerSelected");
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.sensorsTokenManual?.Dispose();
            this.sensorsTokenManual = null;

            this.loadunitsToken?.Dispose();
            this.loadunitsToken = null;

            this.cellsToken?.Dispose();
            this.cellsToken = null;

            this.shutterPositionToken?.Dispose();
            this.shutterPositionToken = null;

            this.homingToken?.Dispose();
            this.homingToken = null;

            this.positioningOperationChangedToken?.Dispose();
            this.positioningOperationChangedToken = null;

            this.elevatorPositionChangedToken?.Dispose();
            this.elevatorPositionChangedToken = null;

            this.lastActiveCommand = "";

            this.StopMoving();
        }

        public override async Task OnAppearedAsync()
        {
            try
            {
                this.SubscribeToEvents();

                this.LightIcon = !this.IsLightActive ? "Brightness5" : "Brightness2";

                this.isManualMovementCompleted = false;

                this.IsElevatorInBay = this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Bay;
                this.IsElevatorInCell = this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Cell;

                if (!this.CanGoToMovementsGuidedExecuteCommand())
                {
                    this.isMovementsGuided = false;
                }

                this.GoToMovementsExecuteCommand(this.isMovementsGuided);

                this.InputLoadingUnitIdPropertyChanged();

                if (this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Cell)
                {
                    this.inputCellId = this.MachineStatus.LogicalPositionId;
                    this.RaisePropertyChanged(nameof(this.InputCellId));
                }
                else if (this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Bay)
                {
                    this.IsPositionUpSelected = this.MachineStatus.BayPositionUpper ?? true;
                    this.IsPositionDownSelected = !(this.MachineStatus.BayPositionUpper ?? true);
                }

                if (!this.IsPositionUpSelected && !this.IsPositionDownSelected)
                {
                    this.IsPositionUpSelected = true;
                }

                if (this.MachineStatus.BayPositionUpper is null)
                {
                    this.IsPositionDownEnabled = this.IsPositionUpEnabled = true;
                }
                else
                {
                    this.IsPositionDownEnabled = this.MachineStatus.BayPositionUpper.Value ||
                            this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Cell;

                    this.IsPositionUpEnabled = !this.MachineStatus.BayPositionUpper.Value;
                }

                this.InputCellIdPropertyChanged();

                this.lastActiveCommand = "";

                this.RaisePropertyChanged(nameof(this.SelectedBayPosition));
                this.RaisePropertyChanged(nameof(this.HasShutter));
                this.RaisePropertyChanged(nameof(this.BayIsShutterThreeSensors));

                await base.OnAppearedAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override async Task OnDataRefreshAsync()
        {
            this.IsLightActive = await this.machineBaysWebService.GetLightAsync();

            await this.SensorsService.RefreshAsync(true);
        }

        protected override async Task OnErrorStatusChangedAsync(MachineErrorEventArgs e)
        {
            await base.OnErrorStatusChangedAsync(e);

            if (!(this.MachineError is null))
            {
                this.StopMoving();
            }
        }

        protected override async Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);

            if (e.MachineMode == MachineMode.SwitchingToAutomatic
                || e.MachineMode == MachineMode.SwitchingToLoadUnitOperations
                || e.MachineMode == MachineMode.SwitchingToLoadUnitOperations2
                || e.MachineMode == MachineMode.SwitchingToLoadUnitOperations3)
            {
                this.GoToMovementsExecuteCommand(true);
                this.goToMovementsManualCommand?.RaiseCanExecuteChanged();
            }
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            this.OnManualMachinePowerChanged();
            this.RaiseCanExecuteChanged();
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            if ((e.MachineStatus?.IsMoving ?? false) && this.IsExecutingProcedure)
            {
                this.IsExecutingProcedure = false;
            }

            // Se ho un cassetto a bordo devo movumentare quello con un vai a baia, scarica, o vai a cella. Quindi posso indicare nelle spinedit il suo valore
            if (!(e.MachineStatus?.IsMoving ?? false) &&
                this.MachineStatus.EmbarkedLoadingUnit != null &&
                (!this.InputLoadingUnitId.HasValue || this.InputLoadingUnitId != this.MachineStatus.EmbarkedLoadingUnit.Id))
            {
                this.InputLoadingUnitId = this.MachineStatus.EmbarkedLoadingUnit.Id;
            }

            await base.OnMachineStatusChangedAsync(e);
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.OnGuidedRaiseCanExecuteChanged();
            this.OnManualRaiseCanExecuteChanged();

            this.goToMovementsGuidedCommand?.RaiseCanExecuteChanged();
            this.goToMovementsManualCommand?.RaiseCanExecuteChanged();
            this.goToOperatorCommand?.RaiseCanExecuteChanged();
            this.stopMovingCommand?.RaiseCanExecuteChanged();
            this.resetCommand?.RaiseCanExecuteChanged();
            this.lightCommand?.RaiseCanExecuteChanged();
            this.isPolicyBypassedCommand?.RaiseCanExecuteChanged();

            if (this.MachineStatus.EmbarkedLoadingUnit != null)
            {
                this.LabelMoveToLoadunit = Localized.Get("InstallationApp.GoToFreeCell");
            }
            else
            {
                this.LabelMoveToLoadunit = Localized.Get("InstallationApp.GoToDrawer");
            }

            this.RaisePropertyChanged(nameof(this.SensorsService));
            this.RaisePropertyChanged(nameof(this.MachineService));
            this.RaisePropertyChanged(nameof(this.MachineStatus));
            this.RaisePropertyChanged(nameof(this.IsMovementsGuided));
            this.RaisePropertyChanged(nameof(this.IsMovementsManual));
            this.RaisePropertyChanged(nameof(this.IsMovementsManualLimited));
            this.RaisePropertyChanged(nameof(this.IsOperator));
            this.RaisePropertyChanged(nameof(this.BayIsShutterThreeSensors));
        }

        private bool CanBaseExecute()
        {
            switch (this.machineService.BayNumber)
            {
                case BayNumber.BayOne:
                default:
                    return this.MachineModeService.MachineMode == MachineMode.Manual &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                   !this.IsKeyboardOpened &&
                   !this.IsExecutingProcedure &&
                   !this.IsMoving;

                case BayNumber.BayTwo:
                    return this.MachineModeService.MachineMode == MachineMode.Manual2 &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                   !this.IsKeyboardOpened &&
                   !this.IsExecutingProcedure &&
                   !this.IsMoving;

                case BayNumber.BayThree:
                    return this.MachineModeService.MachineMode == MachineMode.Manual3 &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                   !this.IsKeyboardOpened &&
                   !this.IsExecutingProcedure &&
                   !this.IsMoving;
            }
        }

        private bool CanEmbark()
        {
            return
                this.CanBaseExecute()
                &&
                !this.SensorsService.Sensors.LuPresentInMachineSide
                &&
                !this.SensorsService.Sensors.LuPresentInOperatorSide
                &&
                this.SensorsService.IsZeroChain;
        }

        private bool CanGoToMovementsGuidedExecuteCommand()
        {
            switch (this.machineService.BayNumber)
            {
                case BayNumber.BayOne:
                default:
                    return (this.MachineModeService.MachineMode == MachineMode.Manual &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                   (this.machineService.IsAxisTuningCompleted || ConfigurationManager.AppSettings.GetOverrideSetupStatus()));

                case BayNumber.BayTwo:
                    return (this.MachineModeService.MachineMode == MachineMode.Manual2 &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                   (this.machineService.IsAxisTuningCompleted || ConfigurationManager.AppSettings.GetOverrideSetupStatus()));

                case BayNumber.BayThree:
                    return (this.MachineModeService.MachineMode == MachineMode.Manual3 &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                   (this.machineService.IsAxisTuningCompleted || ConfigurationManager.AppSettings.GetOverrideSetupStatus()));
            }
        }

        private bool CanGoToMovementsManualExecuteCommand()
        {
            switch (this.machineService.BayNumber)
            {
                case BayNumber.BayOne:
                default:
                    return this.MachineModeService.MachineMode == MachineMode.Manual &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered;

                case BayNumber.BayTwo:
                    return this.MachineModeService.MachineMode == MachineMode.Manual2 &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered;

                case BayNumber.BayThree:
                    return this.MachineModeService.MachineMode == MachineMode.Manual3 &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered;
            }
        }

        private bool CanResetCommand()
        {
            return this.CanBaseExecute() &&
                this.sessionService.UserAccessLevel > UserAccessLevel.Operator;
        }

        private bool CanStopMoving()
        {
            return
                !this.IsKeyboardOpened &&
                this.MachineModeService.MachinePower == MachinePowerState.Powered
                //&&
                //(this.IsMoving ||
                // this.IsMovingElevatorBackwards ||
                // this.IsMovingElevatorForwards ||
                // this.IsMovingElevatorUp ||
                // this.IsMovingElevatorDown ||
                // this.IsCarouselOpening ||
                // this.IsShutterMovingDown ||
                // this.IsShutterMovingUp ||
                // this.IsElevatorMovingToCell ||
                // this.IsElevatorMovingToHeight)
                ;
        }

        private void GoToMovementsExecuteCommand(bool isGuided)
        {
            this.lastActiveCommand = "";
            if (isGuided)
            {
                this.Title = Localized.Get("InstallationApp.MovementsGuided");
            }
            else
            {
                this.Title = Localized.Get("InstallationApp.MovementsManual");
            }

            this.isMovementsGuided = isGuided;

            this.RaisePropertyChanged(nameof(this.IsMovementsGuided));
            this.RaisePropertyChanged(nameof(this.IsMovementsManual));
            this.RaisePropertyChanged(nameof(this.IsMovementsManualLimited));
            this.RaisePropertyChanged(nameof(this.IsOperator));
        }

        private void GoToOperatorExecuteCommand(bool isOperator)
        {
            if (isOperator)
            {
                this.Title = Localized.Get("InstallationApp.Operator");
            }

            this.RaisePropertyChanged(nameof(this.IsMovementsGuided));
            this.RaisePropertyChanged(nameof(this.IsMovementsManual));
            this.RaisePropertyChanged(nameof(this.IsMovementsManualLimited));
            this.RaisePropertyChanged(nameof(this.IsOperator));
        }

        private void OnBayLightChanged(object sender, BayLightChangedEventArgs e)
        {
            if (this.Bay?.Number == e.BayNumber)
            {
                this.IsLightActive = e.IsLightOn;
                this.LightIcon = !this.IsLightActive ? "Brightness5" : "Brightness2";
            }
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.IsElevatorInBay = e.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Bay;
            this.IsElevatorInCell = e.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Cell;

            // se sto muovendo in modalità guidata, è possibile che la cella sia scelta dal mas, quindi la riassegno così le policy indicheranno la cella corrente
            if (e.CellId.HasValue)
            {
                this.selectedCell = this.Cells.SingleOrDefault(c => c.Id == e.CellId);
            }
        }

        private void OnHomingChanged(NotificationMessageUI<HomingMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.StopMoving();
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.StopMoving();
                        this.OperationWarningOrError(message.Status, message.Description);
                        break;
                    }
            }
        }

        private void OnPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        if (this.MachineStatus.BayPositionUpper is null)
                        {
                            this.IsPositionDownEnabled = this.IsPositionUpEnabled = true;
                        }
                        else
                        {
                            this.IsPositionDownEnabled = this.MachineStatus.BayPositionUpper.Value ||
                                    this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Cell;

                            this.IsPositionUpEnabled = !this.MachineStatus.BayPositionUpper.Value;
                        }

                        this.StopMoving();
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.StopMoving();
                        break;
                    }
            }

            this.OnManualPositioningOperationChanged(message);
            this.OnGuidedPositioningOperationChanged(message);
        }

        private async void OnSensorsChangedManual(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            if (this.IsMovementsManual && this.IsMoving)
            {
                switch (this.lastActiveCommand)
                {
                    case "MoveElevatorBackOrForwards":
                        if (this.SensorsService.IsZeroChain || (this.SensorsService.Sensors.LuPresentInMachineSide && this.SensorsService.Sensors.LuPresentInOperatorSide))
                        {
                            await this.StopMovingReleaseAsync();
                        }
                        break;

                    case "MoveOpenCarousel":
                        if (this.SensorsService.BayZeroChain)
                        {
                            await this.StopMovingReleaseAsync();
                        }
                        break;

                    case "MoveElevatorUp":
                        if (this.SensorsService.Sensors.ElevatorOverrun && !this.SensorsService.Sensors.ZeroVerticalSensor)
                        {
                            await this.StopMovingReleaseAsync();
                        }
                        break;

                    case "MoveElevatorDown":
                        if (this.SensorsService.Sensors.ElevatorOverrun && this.SensorsService.Sensors.ZeroVerticalSensor)
                        {
                            await this.StopMovingReleaseAsync();
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private void OnShutterPositionChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            this.OnManualShutterPositionChanged(message);
            this.OnGuidedShutterPositionChanged(message);
        }

        private void OperationWarningOrError(CommonUtils.Messages.Enumerations.MessageStatus status, string errorDescription)
        {
            if (status == CommonUtils.Messages.Enumerations.MessageStatus.OperationError)
            {
                this.ShowNotification(
                    errorDescription,
                    Services.Models.NotificationSeverity.Error);
            }
            else
            {
                this.ShowNotification(
                    Localized.Get("InstallationApp.ProcedureWasStopped"),
                    Services.Models.NotificationSeverity.Warning);
            }
        }

        private async Task RefreshActionPoliciesAsync()
        {
            try
            {
                var selectedBayPosition = this.SelectedBayPosition();
                if (selectedBayPosition != null)
                {
                    Debug.WriteLine("-->:RefreshActionPoliciesAsync:selectedBayPosition");

                    this.moveToBayPositionPolicy = await this.machineElevatorWebService.CanMoveToBayPositionAsync(selectedBayPosition.Id).ConfigureAwait(false);
                    this.moveToBayPositionCommand?.RaiseCanExecuteChanged();
                }

                var selectedCell = this.SelectedCell;
                var selectedLoadunitCell = this.SelectedLoadingUnit?.Cell;
                if (selectedCell != null || selectedLoadunitCell != null)
                {
                    Debug.WriteLine("-->:RefreshActionPoliciesAsync:selectedCell + selectedLoadunitCell");

                    if (selectedCell != null)
                    {
                        this.moveToCellPolicy = await this.machineElevatorWebService.CanMoveToCellAsync(selectedCell.Id).ConfigureAwait(false);
                        this.moveToCellHeightCommand?.RaiseCanExecuteChanged();
                        this.moveToHeightCommand?.RaiseCanExecuteChanged();
                    }
                    else
                    {
                        this.moveToCellPolicy = await this.machineElevatorWebService.CanMoveToCellAsync(selectedLoadunitCell.Id).ConfigureAwait(false);
                        this.moveToLoadingUnitHeightCommand?.RaiseCanExecuteChanged();
                    }
                }

                if (this.InputHeight.HasValue)
                {
                    Debug.WriteLine("-->:RefreshActionPoliciesAsync:inputHeight");

                    this.moveToHeightPolicy = await this.machineElevatorWebService.CanMoveToHeightAsync(this.InputHeight.Value).ConfigureAwait(false);
                    this.moveToHeightCommand?.RaiseCanExecuteChanged();
                }

                if (this.HasCarousel)
                {
                    Debug.WriteLine("-->:RefreshActionPoliciesAsync:carousel");

                    this.moveCarouselUpPolicy = await this.machineCarouselWebService.CanMoveAsync(VerticalMovementDirection.Up, this.IsMovementsManual ? MovementCategory.Manual : MovementCategory.Assisted).ConfigureAwait(false);
                    this.moveCarouselUpCommand?.RaiseCanExecuteChanged();

                    if (this.IsMovementsManual)
                    {
                        this.moveCarouselDownPolicy = await this.machineCarouselWebService.CanMoveAsync(VerticalMovementDirection.Down, this.IsMovementsManual ? MovementCategory.Manual : MovementCategory.Assisted).ConfigureAwait(false);
                        this.moveCarouselDownCommand?.RaiseCanExecuteChanged();
                    }
                }

                if (this.HasBayExternal)
                {
                    Debug.WriteLine("-->:RefreshActionPoliciesAsync:external bay");

                    if (this.MachineService.Bay.IsDouble)
                    {
                        this.moveExtBayTowardOperatorPolicy = await this.machineExternalBayWebService.CanMoveExternalDoubleAsync(ExternalBayMovementDirection.TowardOperator, this.IsMovementsManual ? MovementCategory.Manual : MovementCategory.Assisted, selectedBayPosition.IsUpper).ConfigureAwait(false);
                        this.moveExtBayTowardMachinePolicy = await this.machineExternalBayWebService.CanMoveExternalDoubleAsync(ExternalBayMovementDirection.TowardMachine, this.IsMovementsManual ? MovementCategory.Manual : MovementCategory.Assisted, selectedBayPosition.IsUpper).ConfigureAwait(false);
                    }
                    else
                    {
                        this.moveExtBayTowardOperatorPolicy = await this.machineExternalBayWebService.CanMoveAsync(ExternalBayMovementDirection.TowardOperator, this.IsMovementsManual ? MovementCategory.Manual : MovementCategory.Assisted).ConfigureAwait(false);
                        this.moveExtBayTowardMachinePolicy = await this.machineExternalBayWebService.CanMoveAsync(ExternalBayMovementDirection.TowardMachine, this.IsMovementsManual ? MovementCategory.Manual : MovementCategory.Assisted).ConfigureAwait(false);
                    }
                    this.moveExtBayTowardOperatorCommand?.RaiseCanExecuteChanged();

                    this.moveExtBayTowardMachineCommand?.RaiseCanExecuteChanged();

                    this.moveExtBayMovementForInsertionCommand?.RaiseCanExecuteChanged();

                    this.moveExtBayMovementForExtractionCommand?.RaiseCanExecuteChanged();
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task ResetCommandAsync()
        {
            this.IsKeyboardOpened = true;

            var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), Localized.Get("InstallationApp.ResetMachine"), DialogType.Question, DialogButtons.YesNo);
            if (messageBoxResult is DialogResult.Yes)
            {
                try
                {
                    this.IsWaitingForResponse = true;

                    await this.machineMissionsWebService.ResetMachineAsync();

                    this.ShowNotification(Localized.Get("InstallationApp.ResetMachineSuccessfull"), Services.Models.NotificationSeverity.Success);

                    this.MachineService.OnUpdateServiceAsync();
                }
                catch (Exception ex)
                {
                    this.ShowNotification(ex);
                }
                finally
                {
                    this.IsKeyboardOpened = false;

                    this.IsWaitingForResponse = false;
                }
            }
            else
            {
                this.IsKeyboardOpened = false;
            }
        }

        private BayPosition SelectedBayPosition()
        {
            if (this.IsPositionDownSelected)
            {
                return this.Bay.Positions.Single(p => p.Height == this.Bay.Positions.Min(pos => pos.Height));
            }
            else
            {
                return this.Bay.Positions.Single(p => p.Height == this.Bay.Positions.Max(pos => pos.Height));
            }
        }

        private void StatusSensorsCommand()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.Sensors.SECURITY,
                    data: null,
                    trackCurrentView: true);
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

        private async Task StopMovingAsync()
        {
            Debug.WriteLine($"Command fired");

            // In caso di fine operazione
            if (this.IsMovementsManual && this.isManualMovementCompleted)
            {
                Debug.WriteLine($"-- Command rejected");
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;
                await this.MachineService.StopMovingByAllAsync();

                Debug.WriteLine($"-- Command executed");
            }
            catch (Exception ex)
            {
                this.CloseOperation();
                this.ShowNotification(ex);
            }
            finally
            {
                this.StopMoving();
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopMovingReleaseAsync()
        {
            Debug.WriteLine($"Command Release fired");

            try
            {
                this.IsWaitingForResponse = true;
                await this.MachineService.StopMovingByAllAsync();

                Debug.WriteLine($"-- Command Release executed");
            }
            catch (Exception ex)
            {
                this.CloseOperation();
                this.ShowNotification(ex);
            }
            finally
            {
                this.StopMoving();
                this.IsWaitingForResponse = false;
            }
        }

        private void SubscribeToEvents()
        {
            this.homingToken = this.homingToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<HomingMessageData>>()
                    .Subscribe(
                        this.OnHomingChanged,
                        ThreadOption.UIThread,
                        false,
                        m => this.IsVisible);

            this.shutterPositionToken = this.shutterPositionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Subscribe(
                        this.OnShutterPositionChanged,
                        ThreadOption.UIThread,
                        false,
                        m => this.IsVisible);

            this.positioningOperationChangedToken = this.positioningOperationChangedToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnPositioningOperationChanged,
                        ThreadOption.UIThread,
                        false,
                        m => this.IsVisible);

            this.elevatorPositionChangedToken = this.elevatorPositionChangedToken
                ??
                this.EventAggregator
                    .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnElevatorPositionChanged,
                        ThreadOption.UIThread,
                        false,
                        m => this.IsVisible);

            this.sensorsTokenManual = this.sensorsTokenManual
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChangedManual,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null &&
                             this.IsVisible);

            this.installationHubClient.BayLightChanged += this.OnBayLightChanged;
        }

        #endregion
    }
}
