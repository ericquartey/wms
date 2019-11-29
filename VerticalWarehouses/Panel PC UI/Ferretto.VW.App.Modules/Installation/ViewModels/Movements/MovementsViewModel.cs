using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;
using Prism.Regions;

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

        private bool isKeyboardOpened;

        private bool isMovementsGuided = true;

        private bool isWaitingForResponse;

        private IEnumerable<LoadingUnit> loadingUnits;

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

                this.bays = await this.machineBaysWebService.GetAllAsync();

                this.HasShutter = this.bay.Shutter.Type != ShutterType.NotSpecified;
                this.BayIsShutterThreeSensors = this.bay.Shutter.Type == ShutterType.ThreeSensors;

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

        protected override async Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);
            this.RaiseCanExecuteChanged();
        }

        private bool CanGoToMovementsExecuteCommand()
        {
            return !this.IsWaitingForResponse;
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
            this.isMovementsGuided = isGuided;
            if (isGuided)
            {
                this.Title = InstallationApp.MovementsGuided;
            }
            else
            {
                this.Title = InstallationApp.MovementsManual;
            }
        }

        private void OnShutterPositionChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
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
            this.goToMovementsGuidedCommand?.RaiseCanExecuteChanged();
            this.goToMovementsManualCommand?.RaiseCanExecuteChanged();
            this.stopMovingCommand?.RaiseCanExecuteChanged();

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

        private async Task StopMovingAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineService.StopMovingByAllAsync();
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

            //this.sensorsToken = this.sensorsToken
            //    ??
            //    this.EventAggregator
            //        .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
            //        .Subscribe(
            //            this.OnSensorsChanged,
            //            ThreadOption.UIThread,
            //            false,
            //            m => m.Data != null);
        }

        #endregion
    }
}
