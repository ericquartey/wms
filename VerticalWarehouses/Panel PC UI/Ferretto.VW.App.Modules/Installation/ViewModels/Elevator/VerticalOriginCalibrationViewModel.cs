using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;
using Axis = Ferretto.VW.CommonUtils.Messages.Enumerations.Axis;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class VerticalOriginCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService;

        private double? currentHorizontalPosition;

        private double? currentVerticalPosition;

        private SubscriptionToken elevatorPositionChangedToken;

        private double lowerBound;

        private string noteString;

        private double offset;

        private SubscriptionToken receivedCalibrateAxisUpdateToken;

        private SubscriptionToken receivedSwitchAxisUpdateToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private double resolution;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private double upperBound;

        #endregion

        #region Constructors

        public VerticalOriginCalibrationViewModel(
            IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService,
            IMachineElevatorService machineElevatorService)
            : base(PresentationMode.Installer)
        {
            this.verticalOriginProcedureWebService = verticalOriginProcedureWebService ?? throw new ArgumentNullException(nameof(verticalOriginProcedureWebService));
            this.machineElevatorService = machineElevatorService ?? throw new ArgumentNullException(nameof(machineElevatorService));
        }

        #endregion

        #region Properties

        public double? CurrentHorizontalPosition
        {
            get => this.currentHorizontalPosition;
            private set => this.SetProperty(ref this.currentHorizontalPosition, value);
        }

        public double? CurrentVerticalPosition
        {
            get => this.currentVerticalPosition;
            private set => this.SetProperty(ref this.currentVerticalPosition, value);
        }

        public override EnableMask EnableMask => EnableMask.MachineManualMode | EnableMask.MachinePoweredOn;

        public bool IsExecutingProcedure => this.MachineService.MachineStatus.IsMoving;

        public double LowerBound
        {
            get => this.lowerBound;
            set => this.SetProperty(ref this.lowerBound, value);
        }

        public string NoteString
        {
            get => this.noteString;
            set => this.SetProperty(ref this.noteString, value);
        }

        public double Offset
        {
            get => this.offset;
            set => this.SetProperty(ref this.offset, value);
        }

        public double Resolution
        {
            get => this.resolution;
            set => this.SetProperty(ref this.resolution, value);
        }

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
                async () => await this.StopAsync(),
                this.CanExecuteStopCommand));

        public double UpperBound
        {
            get => this.upperBound;
            set => this.SetProperty(ref this.upperBound, value);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            await base.OnAppearedAsync();
        }

        public async Task RetrieveProcedureInformationAsync()
        {
            try
            {
                var procedureParameters = await this.verticalOriginProcedureWebService.GetParametersAsync();

                this.UpperBound = procedureParameters.UpperBound;
                this.LowerBound = procedureParameters.LowerBound;
                this.Offset = procedureParameters.Offset;
                this.Resolution = procedureParameters.Resolution;

                this.CurrentVerticalPosition = this.machineElevatorService.Position.Vertical;
                this.CurrentHorizontalPosition = this.machineElevatorService.Position.Horizontal;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.SensorsService.RefreshAsync(true);

            await this.RetrieveProcedureInformationAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteStartCommand()
        {
            return !this.MachineService.MachineStatus.IsMoving &&
                   !this.MachineService.MachineStatus.IsMovingLoadingUnit &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh;
        }

        private bool CanExecuteStopCommand()
        {
            return
                this.MachineService.MachineStatus.IsMoving;
        }

        private string GetStringByCalibrateAxisMessageData(Axis axisToCalibrate, MessageStatus status)
        {
            var res = string.Empty;
            switch (status)
            {
                case MessageStatus.OperationExecuting:
                    res = axisToCalibrate == Axis.Horizontal ?
                        VW.App.Resources.Localized.Get("InstallationApp.HorizontalHomingExecuting") :
                        VW.App.Resources.Localized.Get("InstallationApp.VerticalHomingExecuting");
                    break;

                case MessageStatus.OperationError:
                    res = axisToCalibrate == Axis.Horizontal ?
                        VW.App.Resources.Localized.Get("InstallationApp.HorizontalHomingError") :
                        VW.App.Resources.Localized.Get("InstallationApp.VerticalHomingError");
                    break;
            }

            return res;
        }

        private void OnAxisSwitched(NotificationMessageUI<SwitchAxisMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.SwitchEngineStarted"));
                    break;

                case MessageStatus.OperationEnd:
                    this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.SwitchEngineCompleted"));

                    break;

                case MessageStatus.OperationError:
                    this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.SwitchEngineError"));

                    break;
            }
        }

        private void OnCalibrationStepCompleted(NotificationMessageUI<CalibrateAxisMessageData> message)
        {
            if (message.Status == MessageStatus.OperationExecuting)
            {
                this.ShowNotification(
                    string.Format(
                        this.GetStringByCalibrateAxisMessageData(message.Data.AxisToCalibrate, message.Status),
                        message.Data.CurrentStepCalibrate,
                        message.Data.MaxStepCalibrate));
            }
            else if (message.Status == MessageStatus.OperationError)
            {
                this.ShowNotification(message.Description);
            }
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.CurrentHorizontalPosition = e.HorizontalPosition;
            this.CurrentVerticalPosition = e.VerticalPosition;
        }

        private void OnHomingProcedureStatusChanged(NotificationMessageUI<HomingMessageData> message)
        {
            if (message.Data.AxisToCalibrate == Axis.HorizontalAndVertical)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationStart:
                        this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.HorizontalHomingStarted"));

                        break;

                    case MessageStatus.OperationEnd:
                        this.ShowNotification(
                            VW.App.Resources.Localized.Get("InstallationApp.HorizontalHomingCompleted"),
                            Services.Models.NotificationSeverity.Success);

                        break;

                    case MessageStatus.OperationError:
                        this.ShowNotification(
                            VW.App.Resources.Localized.Get("InstallationApp.HorizontalHomingError"),
                            Services.Models.NotificationSeverity.Error);

                        break;
                }
            }
        }

        private async Task StartAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.verticalOriginProcedureWebService.StartAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.MachineService.StopMovingByAllAsync();

                this.ShowNotification(
                    VW.App.Resources.Localized.Get("InstallationApp.SetOriginVerticalAxisNotCompleted"),
                    Services.Models.NotificationSeverity.Warning);
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
            this.receivedSwitchAxisUpdateToken = this.receivedSwitchAxisUpdateToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SwitchAxisMessageData>>()
                    .Subscribe(
                        this.OnAxisSwitched,
                        ThreadOption.UIThread,
                        false);

            this.receivedCalibrateAxisUpdateToken = this.receivedCalibrateAxisUpdateToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<CalibrateAxisMessageData>>()
                    .Subscribe(
                        this.OnCalibrationStepCompleted,
                        ThreadOption.UIThread,
                        false);

            this.receiveHomingUpdateToken = this.receiveHomingUpdateToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<HomingMessageData>>()
                    .Subscribe(
                        this.OnHomingProcedureStatusChanged,
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
        }

        #endregion
    }
}
