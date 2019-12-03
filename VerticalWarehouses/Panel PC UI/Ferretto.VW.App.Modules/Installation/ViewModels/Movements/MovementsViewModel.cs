using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;
using Prism.Regions;
using IDialogService = Ferretto.VW.App.Controls.Interfaces.IDialogService;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class MovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

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

        private IEnumerable<Bay> bays;

        private IEnumerable<Cell> cells;

        private DelegateCommand goToMovementsGuidedCommand;

        private DelegateCommand goToMovementsManualCommand;

        private DelegateCommand goToStatusSensorsCommand;

        private bool isKeyboardOpened;

        private bool isMovementsGuided = true;

        private bool isWaitingForResponse;

        private DelegateCommand keyboardCloseCommand;

        private DelegateCommand keyboardOpenCommand;

        private IEnumerable<LoadingUnit> loadingUnits;

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

        public override EnableMask EnableMask => EnableMask.MachinePoweredOn;

        public ICommand GoToMovementsGuidedCommand =>
            this.goToMovementsGuidedCommand
            ??
            (this.goToMovementsGuidedCommand = new DelegateCommand(
                () => this.GoToMovementsExecuteCommand(true),
                this.CanGoToMovementsExecuteCommand));

        public ICommand GoToMovementsManualCommand =>
            this.goToMovementsManualCommand
            ??
            (this.goToMovementsManualCommand = new DelegateCommand(
                () => this.GoToMovementsExecuteCommand(false),
                this.CanGoToMovementsExecuteCommand));

        public ICommand GoToStatusSensorsCommand =>
            this.goToStatusSensorsCommand
            ??
            (this.goToStatusSensorsCommand = new DelegateCommand(
                () => this.StatusSensorsCommand()));

        public bool IsKeyboardOpened
        {
            get => this.isKeyboardOpened;
            set => this.SetProperty(ref this.isKeyboardOpened, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMovementsGuided => this.isMovementsGuided;

        public bool IsMovementsManual => !this.isMovementsGuided;

        public bool IsMoving
        {
            get =>
                //this.IsElevatorMovingToCell
                //|| this.IsElevatorMovingToHeight
                //|| this.IsElevatorMovingToLoadingUnit
                //|| this.IsElevatorMovingToBay
                //|| this.IsBusyLoadingFromBay
                //|| this.IsBusyLoadingFromCell
                //|| this.IsBusyUnloadingToBay
                //|| this.IsBusyUnloadingToCell
                //|| this.IsTuningChain
                //|| this.IsTuningBay
                //|| this.IsCarouselMoving
                //||
                this.IsShutterMoving;
            set
            {
                //this.IsElevatorMovingToCell = value;
                //this.IsElevatorMovingToHeight = value;
                //this.IsElevatorMovingToLoadingUnit = value;
                //this.IsElevatorMovingToBay = value;
                //this.IsBusyLoadingFromBay = value;
                //this.IsBusyLoadingFromCell = value;
                //this.IsBusyUnloadingToBay = value;
                //this.IsBusyUnloadingToCell = value;
                //this.IsTuningChain = value;
                //this.IsCarouselMoving = value;
                //this.IsTuningBay = value;
                this.IsShutterMoving = value;
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public ICommand KeyboardCloseCommand =>
            this.keyboardCloseCommand
            ??
            (this.keyboardCloseCommand = new DelegateCommand(() => this.KeyboardClose()));

        public ICommand KeyboardOpenCommand =>
           this.keyboardOpenCommand
           ??
           (this.keyboardOpenCommand = new DelegateCommand(() => this.KeyboardOpen()));

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
            base.Disappear();

            this.sensorsToken?.Dispose();
            this.sensorsToken = null;

            this.shutterPositionToken?.Dispose();
            this.shutterPositionToken = null;
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            try
            {
                this.GoToMovementsExecuteCommand(this.isMovementsGuided);

                await this.RefreshMachineInfoAsync();

                await this.sensorsService.RefreshAsync(true);

                this.bays = await this.machineBaysWebService.GetAllAsync();

                this.HasShutter = this.bay.Shutter.Type != ShutterType.NotSpecified;
                this.BayIsShutterThreeSensors = this.bay.Shutter.Type == ShutterType.ThreeSensors;

                await this.OnManualAppearedAsync();

                this.SubscribeToEvents();
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

            this.OnManualErrorStatusChanged();
        }

        protected override async Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);
            this.OnManualMachinePowerChanged();
            this.RaiseCanExecuteChanged();
        }

        private bool CanGoToMovementsExecuteCommand()
        {
            return !this.IsWaitingForResponse &&
                !this.IsMoving;
        }

        private bool CanResetCommand()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;
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

        private void GoToMovementsExecuteCommand(bool isGuided)
        {
            if (!isGuided)
            {
                var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmationOperation, InstallationApp.MovementsManual, DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult is DialogResult.No)
                {
                    return;
                }
            }

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

        private void KeyboardClose()
        {
            this.IsKeyboardOpened = false;
        }

        private void KeyboardOpen()
        {
            this.IsKeyboardOpened = true;
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            //this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
            this.RaiseCanExecuteChanged();
        }

        private void OnShutterPositionChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            this.OnManualShutterPositionChanged(message);
            this.OnGuidedShutterPositionChanged(message);
        }

        private void OperationWarningOrError(CommonUtils.Messages.Enumerations.MessageStatus status, string errorDescription)
        {
            this.IsMoving = false;

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

        private void RaiseCanExecuteChanged()
        {
            this.OnManualRaiseCanExecuteChanged();
            this.OnGuidedRaiseCanExecuteChanged();

            this.goToMovementsGuidedCommand?.RaiseCanExecuteChanged();
            this.goToMovementsManualCommand?.RaiseCanExecuteChanged();
            this.stopMovingCommand?.RaiseCanExecuteChanged();
            this.resetCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.SensorsService));
            this.RaisePropertyChanged(nameof(this.IsMovementsGuided));
            this.RaisePropertyChanged(nameof(this.IsMovementsManual));
        }

        private async Task RefreshMachineInfoAsync()
        {
            try
            {
                this.bay = await this.bayManagerService.GetBayAsync();
                this.cells = await this.machineCellsWebService.GetAllAsync();
                this.loadingUnits = await this.machineLoadingUnitsWebService.GetAllAsync();

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

                    await this.sensorsService.RefreshAsync(false);

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
                    Utils.Modules.Installation.Sensors.VERTICALAXIS,
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
            //this.homingToken = this.homingToken
            //    ??
            //    this.EventAggregator
            //        .GetEvent<NotificationEventUI<HomingMessageData>>()
            //        .Subscribe(
            //            async m => await this.OnHomingChangedAsync(m),
            //            ThreadOption.UIThread,
            //            false);

            this.shutterPositionToken = this.shutterPositionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Subscribe(
                        this.OnShutterPositionChanged,
                        ThreadOption.UIThread,
                        false);

            //this.positioningOperationChangedToken = this.positioningOperationChangedToken
            //    ??
            //    this.EventAggregator
            //        .GetEvent<NotificationEventUI<PositioningMessageData>>()
            //        .Subscribe(
            //            async m => await this.OnPositioningOperationChangedAsync(m),
            //            ThreadOption.UIThread,
            //            false);

            //this.elevatorPositionChangedToken = this.elevatorPositionChangedToken
            // ??
            // this.EventAggregator
            //     .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
            //     .Subscribe(
            //         this.OnElevatorPositionChanged,
            //         ThreadOption.UIThread,
            //         false);

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
