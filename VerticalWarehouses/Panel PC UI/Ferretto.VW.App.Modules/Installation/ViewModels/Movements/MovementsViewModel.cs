using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Events;
using Prism.Regions;
using IDialogService = Ferretto.VW.App.Controls.Interfaces.IDialogService;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed partial class MovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineMissionOperationsWebService machineMissionOperationsWebService;

        private readonly IMachineService machineService;

        private readonly ISensorsService sensorsService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private Bay bay;

        private bool bayIsMultiPosition;

        private IEnumerable<Bay> bays;

        private IEnumerable<Cell> cells;

        private SubscriptionToken cellsToken;

        private SubscriptionToken elevatorPositionChangedToken;

        private DelegateCommand goToMovementsGuidedCommand;

        private DelegateCommand goToMovementsManualCommand;

        private DelegateCommand goToStatusSensorsCommand;

        private SubscriptionToken homingToken;

        private bool isCarouselMoving;

        private bool isElevatorInBay;

        private bool isElevatorInCell;

        private bool isExecutingProcedure;

        private bool isMovementsGuided = true;

        private IEnumerable<LoadingUnit> loadingUnits;

        private SubscriptionToken loadunitsToken;

        private SubscriptionToken positioningOperationChangedToken;

        private DelegateCommand resetCommand;

        private SubscriptionToken sensorsToken;

        private SubscriptionToken shutterPositionToken;

        private DelegateCommand stopMovingCommand;

        private string title;

        #endregion

        #region Constructors

        public MovementsViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineCellsWebService machineCellsWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineShuttersWebService shuttersWebService,
            IMachineCarouselWebService machineCarouselWebService,
            ISensorsService sensorsService,
            IMachineBaysWebService machineBaysWebService,
            IMachineMissionOperationsWebService machineMissionOperationsWebService,
            IBayManager bayManagerService,
            IMachineService machineService)
            : base(PresentationMode.Installer)
        {
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineMissionOperationsWebService = machineMissionOperationsWebService ?? throw new ArgumentNullException(nameof(machineMissionOperationsWebService));
        }

        #endregion

        #region Properties

        public bool BayIsMultiPosition
        {
            get => this.bayIsMultiPosition;
            set => this.SetProperty(ref this.bayIsMultiPosition, value);
        }

        public override EnableMask EnableMask => EnableMask.MachinePoweredOn;

        public ICommand GoToMovementsGuidedCommand =>
            this.goToMovementsGuidedCommand
            ??
            (this.goToMovementsGuidedCommand = new DelegateCommand(
                () => this.GoToMovementsExecuteCommand(true),
                this.CanGoToMovementsGuidedExecuteCommand));

        public ICommand GoToMovementsManualCommand =>
            this.goToMovementsManualCommand
            ??
            (this.goToMovementsManualCommand = new DelegateCommand(
                () => this.GoToMovementsExecuteCommand(false),
                this.CanGoToMovementsManualExecuteCommand));

        public ICommand GoToStatusSensorsCommand =>
            this.goToStatusSensorsCommand
            ??
            (this.goToStatusSensorsCommand = new DelegateCommand(
                () => this.StatusSensorsCommand()));

        public bool HasCarousel
        {
            get => this.MachineService.HasCarousel;
        }

        public bool HasShutter
        {
            get => this.MachineService.HasShutter;
        }

        public bool IsCarouselMoving
        {
            get => this.isCarouselMoving;
            private set => this.SetProperty(ref this.isCarouselMoving, value, this.RaiseCanExecuteChanged);
        }

        public bool IsElevatorInBay
        {
            get => this.isElevatorInBay;
            private set => this.SetProperty(ref this.isElevatorInBay, value, this.ElevatorChanged);
        }

        public bool IsElevatorInCell
        {
            get => this.isElevatorInCell;
            private set => this.SetProperty(ref this.isElevatorInCell, value, this.ElevatorChanged);
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            set => this.SetProperty(ref this.isExecutingProcedure, value);
        }

        public bool IsMovementsGuided => this.isMovementsGuided;

        public bool IsMovementsManual => !this.isMovementsGuided;

        public bool IsMoving
        {
            get => (this.machineService?.MachineStatus?.IsMoving ?? true) || (this.machineService?.MachineStatus?.IsMovingLoadingUnit ?? true);
        }

        public IMachineService MachineService => this.machineService;

        public MachineStatus MachineStatus => this.machineService.MachineStatus;

        public ICommand ResetCommand =>
            this.resetCommand
            ??
            (this.resetCommand = new DelegateCommand(
               async () => await this.ResetCommandAsync(),
               this.CanResetCommand));

        public ISensorsService SensorsService => this.sensorsService;

        public ICommand StopMovingCommand =>
            this.stopMovingCommand
            ??
            (this.stopMovingCommand = new DelegateCommand(
                async () => await this.StopMovingAsync(),
                this.CanStopMoving));

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.IsWaitingForResponse = false;

            base.Disappear();

            this.sensorsToken?.Dispose();
            this.sensorsToken = null;

            this.shutterPositionToken?.Dispose();
            this.shutterPositionToken = null;

            this.homingToken?.Dispose();
            this.homingToken = null;

            this.positioningOperationChangedToken?.Dispose();
            this.positioningOperationChangedToken = null;

            this.elevatorPositionChangedToken?.Dispose();
            this.elevatorPositionChangedToken = null;
        }

        public override async Task OnAppearedAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await base.OnAppearedAsync();

                this.SubscribeToEvents();

                this.GoToMovementsExecuteCommand(this.isMovementsGuided);

                await this.RefreshMachineInfoAsync();

                await this.sensorsService.RefreshAsync(true);

                this.bays = await this.machineBaysWebService.GetAllAsync();

                this.SelectBayPositionUp();
                this.InputLoadingUnitIdPropertyChanged();
                this.InputCellIdPropertyChanged();

                var elevatorPosition = await this.machineElevatorWebService.GetPositionAsync();

                this.IsElevatorInCell = elevatorPosition.CellId != null;
                this.IsElevatorInBay = elevatorPosition.BayPositionId != null;

                if (this.IsElevatorInCell)
                {
                    this.InputCellId = elevatorPosition.CellId;
                }
                else if (this.IsElevatorInBay)
                {
                    this.SelectedBayPosition = this.bay.Positions.SingleOrDefault(p => p.Id == elevatorPosition.BayPositionId);
                }

                this.BayIsMultiPosition = this.bay.IsDouble;

                this.OnManualAppearedAsync();

                this.RaiseCanExecuteChanged();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
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

            if (e.MachineMode == MachineMode.SwitchingToAutomatic)
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
            await base.OnMachineStatusChangedAsync(e);

            if ((e.MachineStatus?.IsMoving ?? false) && this.IsExecutingProcedure)
            {
                this.IsExecutingProcedure = false;
            }

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            if (!this.IsVisible)
            {
                return;
            }

            base.RaiseCanExecuteChanged();

            this.OnManualRaiseCanExecuteChanged();
            this.OnGuidedRaiseCanExecuteChanged();

            this.goToMovementsGuidedCommand?.RaiseCanExecuteChanged();
            this.goToMovementsManualCommand?.RaiseCanExecuteChanged();
            this.stopMovingCommand?.RaiseCanExecuteChanged();
            this.resetCommand?.RaiseCanExecuteChanged();

            if (this.MachineStatus.EmbarkedLoadingUnit != null)
            {
                if (!string.IsNullOrEmpty(this.MachineStatus.LogicalPositionId))
                {
                    this.LabelMoveToLoadunit = "Vai a cella";
                }
                else
                {
                    this.LabelMoveToLoadunit = "Vai a cella libera";
                }
            }
            else
            {
                this.LabelMoveToLoadunit = InstallationApp.GoToDrawer;
            }

            this.RaisePropertyChanged(nameof(this.IsMoving));
            this.RaisePropertyChanged(nameof(this.SensorsService));
            this.RaisePropertyChanged(nameof(this.MachineService));
            this.RaisePropertyChanged(nameof(this.MachineStatus));
            this.RaisePropertyChanged(nameof(this.IsMovementsGuided));
            this.RaisePropertyChanged(nameof(this.IsMovementsManual));
            this.RaisePropertyChanged(nameof(this.BayIsShutterThreeSensors));
        }

        private bool CanBaseExecute()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving;
        }

        private bool CanEmbark()
        {
            return
                this.CanBaseExecute()
                &&
                !this.sensorsService.Sensors.LuPresentInMachineSide
                &&
                !this.sensorsService.Sensors.LuPresentInOperatorSide
                &&
                this.sensorsService.IsZeroChain;
        }

        private bool CanGoToMovementsGuidedExecuteCommand()
        {
            return true;
        }

        private bool CanGoToMovementsManualExecuteCommand()
        {
            return this.MachineModeService?.MachineMode == MachineMode.Manual;
        }

        private bool CanResetCommand()
        {
            return this.CanBaseExecute();
        }

        private bool CanStopMoving()
        {
            return
                !this.IsKeyboardOpened
                &&
                this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private void ElevatorChanged()
        {
            this.RefreshActionPoliciesAsync().ConfigureAwait(false);
        }

        private void GoToMovementsExecuteCommand(bool isGuided)
        {
            // if (!isGuided && this.isMovementsGuided)
            // {
            //    var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
            //    var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmationOperation, InstallationApp.MovementsManual, DialogType.Question, DialogButtons.YesNo);
            //    if (messageBoxResult is DialogResult.No)
            //    {
            //        return;
            //    }
            // }
            if (isGuided)
            {
                this.Title = InstallationApp.MovementsGuided;
            }
            else
            {
                this.Title = InstallationApp.MovementsManual;
            }

            this.isMovementsGuided = isGuided;

            this.RaisePropertyChanged(nameof(this.IsMovementsGuided));
            this.RaisePropertyChanged(nameof(this.IsMovementsManual));
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.IsElevatorInBay = e.BayPositionId != null;
            this.IsElevatorInCell = e.CellId != null;
        }

        private async Task OnHomingChangedAsync(NotificationMessageUI<HomingMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.StopMoving();

                        await this.RefreshMachineInfoAsync();
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.StopMoving();

                        await this.RefreshMachineInfoAsync();
                        this.OperationWarningOrError(message.Status, message.Description);
                        break;
                    }
            }
        }

        private async Task OnPositioningOperationChangedAsync(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.StopMoving();

                        await this.RefreshMachineInfoAsync();
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.StopMoving();

                        await this.RefreshMachineInfoAsync();
                        break;
                    }
            }

            this.OnManualPositioningOperationChanged(message);
            await this.OnGuidedPositioningOperationChangedAsync(message);
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.RaiseCanExecuteChanged();
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
                    InstallationApp.ProcedureWasStopped,
                    Services.Models.NotificationSeverity.Warning);
            }
        }

        private async Task RefreshActionPoliciesAsync()
        {
            try
            {
                // System.Diagnostics.Debug.WriteLine("*************");
                // this.Log = $"*************{Environment.NewLine}{this.Log}";
                var selectedBayPosition = this.SelectedBayPosition;
                if (selectedBayPosition != null)
                {
                    this.loadFromBayPolicy = await this.machineElevatorWebService.CanLoadFromBayAsync(selectedBayPosition.Id, this.SelectedBayPosition.LoadingUnit != null);
                    this.loadFromBayCommand?.RaiseCanExecuteChanged();

                    // if (!string.IsNullOrEmpty(this.loadFromBayPolicy.Reason))
                    // {
                    //    this.Log = $"{DateTime.Now.ToLocalTime()} - LoadFromBay - {this.loadFromBayPolicy.Reason}{Environment.NewLine}{this.Log}";
                    // }

                    // System.Diagnostics.Debug.WriteLine($"ELEV <- BAY: {this.loadFromBayPolicy.IsAllowed} {this.loadFromBayPolicy.Reason}");
                    this.unloadToBayPolicy = await this.machineElevatorWebService.CanUnloadToBayAsync(selectedBayPosition.Id, this.MachineStatus.EmbarkedLoadingUnit != null);
                    this.unloadToBayCommand?.RaiseCanExecuteChanged();

                    // if (!string.IsNullOrEmpty(this.unloadToBayPolicy.Reason))
                    // {
                    //    this.Log = $"{DateTime.Now.ToLocalTime()} - UnloadToBay - {this.unloadToBayPolicy.Reason}{Environment.NewLine}{this.Log}";
                    // }

                    // System.Diagnostics.Debug.WriteLine($"ELEV -> BAY: {this.unloadToBayPolicy.IsAllowed} {this.unloadToBayPolicy.Reason}");
                    this.moveToBayPositionPolicy = await this.machineElevatorWebService.CanMoveToBayPositionAsync(selectedBayPosition.Id);
                    this.moveToBayPositionCommand?.RaiseCanExecuteChanged();

                    // if (!string.IsNullOrEmpty(this.moveToBayPositionPolicy.Reason))
                    // {
                    //    this.Log = $"{DateTime.Now.ToLocalTime()} - MoveToBayPosition - {this.moveToBayPositionPolicy.Reason}{Environment.NewLine}{this.Log}";
                    // }

                    // System.Diagnostics.Debug.WriteLine($"ELEV ^ BAY: {this.moveToBayPositionPolicy.IsAllowed} {this.moveToBayPositionPolicy.Reason}");
                }

                var selectedCell = this.SelectedCell;
                if (selectedCell != null)
                {
                    this.loadFromCellPolicy = await this.machineElevatorWebService.CanLoadFromCellAsync(selectedCell.Id);
                    this.loadFromCellCommand?.RaiseCanExecuteChanged();

                    // if (!string.IsNullOrEmpty(this.loadFromCellPolicy.Reason))
                    // {
                    //    this.Log = $"{DateTime.Now.ToLocalTime()} - LoadFromCell - {this.loadFromCellPolicy.Reason}{Environment.NewLine}{this.Log}";
                    // }

                    // System.Diagnostics.Debug.WriteLine($"ELEV <- CELL: {this.loadFromCellPolicy.IsAllowed} {this.loadFromCellPolicy.Reason}");
                    this.unloadToCellPolicy = await this.machineElevatorWebService.CanUnloadToCellAsync(selectedCell.Id);
                    this.unloadToCellCommand?.RaiseCanExecuteChanged();

                    // if (!string.IsNullOrEmpty(this.unloadToCellPolicy.Reason))
                    // {
                    //    this.Log = $"{DateTime.Now.ToLocalTime()} - UnloadToCell - {this.unloadToCellPolicy.Reason}{Environment.NewLine}{this.Log}";
                    // }

                    // System.Diagnostics.Debug.WriteLine($"ELEV -> CELL: {this.unloadToCellPolicy.IsAllowed} {this.unloadToCellPolicy.Reason}");
                    this.moveToCellPolicy = await this.machineElevatorWebService.CanMoveToCellAsync(selectedCell.Id);
                    this.moveToCellHeightCommand?.RaiseCanExecuteChanged();

                    // if (!string.IsNullOrEmpty(this.moveToCellPolicy.Reason))
                    // {
                    //    this.Log = $"{DateTime.Now.ToLocalTime()} - MoveToCellHeight - {this.moveToCellPolicy.Reason}{Environment.NewLine}{this.Log}";
                    // }

                    // System.Diagnostics.Debug.WriteLine($"ELEV ^ CELL: {this.moveToCellPolicy.IsAllowed} {this.moveToCellPolicy.Reason}");
                }

                if (this.HasCarousel)
                {
                    this.moveCarouselUpPolicy = await this.machineCarouselWebService.CanMoveAsync(VerticalMovementDirection.Up, this.IsMovementsManual ? MovementCategory.Manual : MovementCategory.Assisted);
                    this.moveCarouselUpCommand?.RaiseCanExecuteChanged();

                    // if (!string.IsNullOrEmpty(this.moveCarouselUpPolicy.Reason))
                    // {
                    //    this.Log = $"{DateTime.Now.ToLocalTime()} - MoveCarouselUp - {this.moveCarouselUpPolicy.Reason}{Environment.NewLine}{this.Log}";
                    // }
                    this.moveCarouselDownPolicy = await this.machineCarouselWebService.CanMoveAsync(VerticalMovementDirection.Down, this.IsMovementsManual ? MovementCategory.Manual : MovementCategory.Assisted);
                    this.moveCarouselDownCommand?.RaiseCanExecuteChanged();

                    // if (!string.IsNullOrEmpty(this.moveCarouselDownPolicy.Reason))
                    // {
                    //    this.Log = $"{DateTime.Now.ToLocalTime()} - MoveCarouselDown - {this.moveCarouselDownPolicy.Reason}{Environment.NewLine}{this.Log}";
                    // }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task RefreshMachineInfoAsync()
        {
            try
            {
                this.bay = this.machineService.Bay;

                this.InputCellIdPropertyChanged();
                this.InputLoadingUnitIdPropertyChanged();

                this.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task ResetCommandAsync()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
            var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmationOperation, this.Title, DialogType.Question, DialogButtons.YesNo);
            if (messageBoxResult is DialogResult.Yes)
            {
                try
                {
                    this.IsWaitingForResponse = true;

                    await this.machineMissionOperationsWebService.ResetMachineAsync();

                    this.RaiseCanExecuteChanged();

                    this.ShowNotification(InstallationApp.ResetMachineSuccessfull, Services.Models.NotificationSeverity.Success);
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
            // In caso di fine operazione
            if (this.IsMovementsManual && this.isCompleted)
            {
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;
                await this.machineService.StopMovingByAllAsync();
            }
            catch (System.Exception ex)
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
            this.loadingUnits = this.MachineService.Loadunits;
            this.loadunitsToken = this.loadunitsToken
                 ??
                 this.EventAggregator
                     .GetEvent<LoadUnitsChangedPubSubEvent>()
                     .Subscribe(
                         m =>
                         {
                             this.loadingUnits = m.Loadunits;
                             this.InputLoadingUnitIdPropertyChanged();
                             this.RaiseCanExecuteChanged();
                         },
                         ThreadOption.UIThread,
                         false,
                         m => this.IsVisible);

            this.cells = this.MachineService.Cells;
            this.cellsToken = this.cellsToken
                 ??
                 this.EventAggregator
                     .GetEvent<CellsChangedPubSubEvent>()
                     .Subscribe(
                         m =>
                         {
                             this.cells = m.Cells;
                             this.InputCellIdPropertyChanged();
                             this.RaiseCanExecuteChanged();
                         },
                         ThreadOption.UIThread,
                         false,
                         m => this.IsVisible);

            this.homingToken = this.homingToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<HomingMessageData>>()
                    .Subscribe(
                        async m => await this.OnHomingChangedAsync(m),
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
                        async m => await this.OnPositioningOperationChangedAsync(m),
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

            this.sensorsToken = this.sensorsToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null &&
                             this.IsVisible);
        }

        #endregion
    }
}
