using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class SemiAutoMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineMissionOperationsWebService machineMissionOperationsWebService;

        private readonly ISensorsService sensorsService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private bool bayIsShutterThreeSensors;

        private SubscriptionToken elevatorPositionChangedToken;

        private SubscriptionToken homingToken;

        private int? inputLoadingUnitCode;

        private bool isWaitingForResponse;

        private IEnumerable<LoadingUnit> loadingUnits;

        private SubscriptionToken positioningOperationChangedToken;

        private DelegateCommand resetCommand;

        private SubscriptionToken sensorsToken;

        private SubscriptionToken shutterPositionToken;

        private DelegateCommand stopMovingCommand;

        #endregion

        #region Constructors

        public SemiAutoMovementsViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineCellsWebService machineCellsWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineShuttersWebService shuttersWebService,
            IMachineCarouselWebService machineCarouselWebService,
            ISensorsService sensorsService,
            IMachineMissionOperationsWebService machineMissionOperationsWebService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));
            this.machineMissionOperationsWebService = machineMissionOperationsWebService ?? throw new ArgumentNullException(nameof(machineMissionOperationsWebService));
        }

        #endregion

        #region Properties

        public bool BayIsMultiPosition
        {
            get => this.bayIsMultiPosition;
            set => this.SetProperty(ref this.bayIsMultiPosition, value);
        }

        public bool BayIsShutterThreeSensors
        {
            get => this.bayIsShutterThreeSensors;
            set => this.SetProperty(ref this.bayIsShutterThreeSensors, value);
        }

        public int? InputLoadingUnitCode
        {
            get => this.inputLoadingUnitCode;
            set => this.SetProperty(ref this.inputLoadingUnitCode, value);
        }

        public bool IsMoving
        {
            get => this.IsElevatorMovingToCell
                || this.IsElevatorMovingToHeight
                || this.IsElevatorMovingToLoadingUnit
                || this.IsElevatorMovingToBay
                || this.IsBusyLoadingFromBay
                || this.IsBusyLoadingFromCell
                || this.IsBusyUnloadingToBay
                || this.IsBusyUnloadingToCell
                || this.IsTuningChain
                || this.IsTuningBay
                || this.IsCarouselMoving
                || this.IsShutterMoving;
            set
            {
                this.IsElevatorMovingToCell = false;
                this.IsElevatorMovingToHeight = false;
                this.IsElevatorMovingToLoadingUnit = false;
                this.IsElevatorMovingToBay = false;
                this.IsBusyLoadingFromBay = false;
                this.IsBusyLoadingFromCell = false;
                this.IsBusyUnloadingToBay = false;
                this.IsBusyUnloadingToCell = false;
                this.IsTuningChain = false;
                this.IsCarouselMoving = false;
                this.IsTuningBay = false;
                this.IsShutterMoving = false;
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            private set
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

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.homingToken?.Dispose();
            this.homingToken = null;

            this.shutterPositionToken?.Dispose();
            this.shutterPositionToken = null;

            this.subscriptionToken?.Dispose();
            this.subscriptionToken = null;

            this.sensorsToken?.Dispose();
            this.sensorsToken = null;
            */
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            try
            {
                this.IsWaitingForResponse = true;

                this.bay = await this.bayManagerService.GetBayAsync();
                this.cells = await this.machineCellsWebService.GetAllAsync();
                this.loadingUnits = await this.machineLoadingUnitsWebService.GetAllAsync();
                this.procedureParameters = await this.machineElevatorWebService.GetVerticalManualMovementsParametersAsync();

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

                this.SelectBayPositionUp();
                this.InputLoadingUnitIdPropertyChanged();
                this.InputCellIdPropertyChanged();

                this.BayIsMultiPosition = this.bay.IsDouble;

                this.BayIsShutterThreeSensors = this.bay.Shutter.Type is ShutterType.ThreeSensors;

                this.SubscribeToEvents();

                this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
                this.RaiseCanExecuteChanged();
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

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState != MachinePowerState.Powered)
            {
                this.IsMoving = false;
            }
        }

        private bool CanResetCommand()
        {
            return
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanStopMoving()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private void OnHomingChanged(NotificationMessageUI<HomingMessageData> message)
        {
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

        private void OnPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                    {
                        this.ShowNotification(string.Empty);

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.IsMoving = false;

                        /***
                         * WHY??
                        if (message.Data?.MovementMode == CommonUtils.Messages.Enumerations.MovementMode.BayChain)
                        {
                            this.IsCarouselMoving = false;
                        }
    */
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
            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
            this.RaiseCanExecuteChanged();
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
            this.CanInputCellId =
                this.cells != null
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;

            this.CanInputHeight =
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;

            this.CanInputLoadingUnitId =
                this.loadingUnits != null
                &&
                this.cells != null
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;

            this.RefreshActionPoliciesAsync();

            this.moveToHeightCommand?.RaiseCanExecuteChanged();
            this.moveToLoadingUnitHeightCommand?.RaiseCanExecuteChanged();
            this.tuningBayCommand?.RaiseCanExecuteChanged();
            this.tuningChainCommand?.RaiseCanExecuteChanged();

            this.openShutterCommand?.RaiseCanExecuteChanged();
            this.intermediateShutterCommand?.RaiseCanExecuteChanged();
            this.closedShutterCommand?.RaiseCanExecuteChanged();
            this.carouselDownCommand?.RaiseCanExecuteChanged();
            this.carouselUpCommand?.RaiseCanExecuteChanged();
            this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
            this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();
            this.stopMovingCommand?.RaiseCanExecuteChanged();
            this.resetCommand?.RaiseCanExecuteChanged();
            this.setWeightControlCommand?.RaiseCanExecuteChanged();
            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
        }

        private async Task ResetCommandAsync()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
            var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmationOperation, "Movimenti semi-automatici", DialogType.Question, DialogButtons.YesNo);
            if (messageBoxResult is DialogResult.Yes)
            {
                try
                {
                    this.IsWaitingForResponse = true;

                    await this.machineMissionOperationsWebService.ResetMachineAsync();

                    await this.sensorsService.RefreshAsync(false);

                    this.RaiseCanExecuteChanged();

                    this.ShowNotification("Reset macchina avvenuta con successo.", Services.Models.NotificationSeverity.Success);
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

        private async Task StopMovingAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                if (this.IsShutterMoving)
                {
                    await this.shuttersWebService.StopAsync();
                }
                else if (this.IsCarouselMoving)
                {
                    await this.machineCarouselWebService.StopAsync();
                }
                else if (this.IsTuningBay)
                {
                    await this.machineCarouselWebService.StopAsync();
                }
                else
                {
                    await this.machineElevatorWebService.StopAsync();
                }
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
            this.homingToken = this.homingToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<HomingMessageData>>()
                    .Subscribe(
                        this.OnHomingChanged,
                        ThreadOption.UIThread,
                        false);

            this.shutterPositionToken = this.shutterPositionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Subscribe(
                        this.OnShutterPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.positioningOperationChangedToken = this.positioningOperationChangedToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnPositioningOperationChanged,
                        ThreadOption.UIThread,
                        false);

            this.elevatorPositionChangedToken = this.elevatorPositionChangedToken
             ??
             this.EventAggregator
                 .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
                 .Subscribe(
                     this.OnElevatorPositionChanged,
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
