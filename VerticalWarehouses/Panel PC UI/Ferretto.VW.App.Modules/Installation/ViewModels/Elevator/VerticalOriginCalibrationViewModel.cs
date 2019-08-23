using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Commands;
using Prism.Events;
using Axis = Ferretto.VW.CommonUtils.Messages.Enumerations.Axis;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalOriginCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineVerticalOriginProcedureService verticalOriginProcedureService;

        private decimal? currentPosition;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private decimal lowerBound;

        private string noteString = VW.App.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;

        private decimal offset;

        private SubscriptionToken receivedCalibrateAxisUpdateToken;

        private SubscriptionToken receivedSwitchAxisUpdateToken;

        private SubscriptionToken receiveExceptionToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private decimal resolution;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private SubscriptionToken updateCurrentPositionToken;

        private decimal upperBound;

        #endregion

        #region Constructors

        public VerticalOriginCalibrationViewModel(
            IMachineVerticalOriginProcedureService verticalOriginProcedureService,
            IMachineElevatorService machineElevatorService)
            : base(Services.PresentationMode.Installer)
        {
            if (verticalOriginProcedureService is null)
            {
                throw new ArgumentNullException(nameof(verticalOriginProcedureService));
            }

            if (machineElevatorService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorService));
            }

            this.verticalOriginProcedureService = verticalOriginProcedureService;
            this.machineElevatorService = machineElevatorService;
        }

        #endregion

        #region Properties

        public decimal? CurrentPosition
        {
            get => this.currentPosition;
            set => this.SetProperty(ref this.currentPosition, value);
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            set
            {
                if (this.SetProperty(ref this.isExecutingProcedure, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public decimal LowerBound
        {
            get => this.lowerBound;
            set => this.SetProperty(ref this.lowerBound, value);
        }

        public string NoteString
        {
            get => this.noteString;
            set => this.SetProperty(ref this.noteString, value);
        }

        public decimal Offset
        {
            get => this.offset;
            set => this.SetProperty(ref this.offset, value);
        }

        public decimal Resolution
        {
            get => this.resolution;
            set => this.SetProperty(ref this.resolution, value);
        }

        public ICommand StartCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.ExecuteStartCommandAsync(),
                this.CanExecuteStartCommand));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.ExecuteStopCommandAsync(),
                this.CanExecuteStopCommand));

        public decimal UpperBound
        {
            get => this.upperBound;
            set => this.SetProperty(ref this.upperBound, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.EventAggregator
                .GetEvent<NotificationEventUI<SwitchAxisMessageData>>()
                .Unsubscribe(this.receivedSwitchAxisUpdateToken);

            this.EventAggregator
                .GetEvent<NotificationEventUI<CalibrateAxisMessageData>>()
                .Unsubscribe(this.receivedCalibrateAxisUpdateToken);

            this.EventAggregator
                .GetEvent<NotificationEventUI<HomingMessageData>>()
                .Unsubscribe(this.receiveHomingUpdateToken);
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;

            await this.RetrieveProcedureInformationAsync();

            this.SubscribeToEvents();
        }

        public async Task RetrieveProcedureInformationAsync()
        {
            try
            {
                var procedureParameters = await this.verticalOriginProcedureService.GetProcedureParametersAsync();

                this.UpperBound = procedureParameters.UpperBound;
                this.LowerBound = procedureParameters.LowerBound;
                this.Offset = procedureParameters.Offset;
                this.Resolution = procedureParameters.Resolution;

                this.CurrentPosition = await this.machineElevatorService.GetVerticalPositionAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private bool CanExecuteStartCommand()
        {
            return !this.isExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanExecuteStopCommand()
        {
            return this.isExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        private async Task ExecuteStartCommandAsync()
        {
            try
            {
                this.IsExecutingProcedure = true;
                this.IsWaitingForResponse = true;

                await this.verticalOriginProcedureService.StartAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;
            }
        }

        private async Task ExecuteStopCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.verticalOriginProcedureService.StopAsync();

                this.NoteString = VW.App.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.IsWaitingForResponse = false;
            }
            finally
            {
                this.IsWaitingForResponse = false; // TODO missing notification from service, to confirm abort of operation
                this.IsExecutingProcedure = false;
            }
        }

        private string GetStringByCalibrateAxisMessageData(Axis axisToCalibrate, MessageStatus status)
        {
            var res = string.Empty;
            switch (status)
            {
                case MessageStatus.OperationExecuting:
                    res = axisToCalibrate == Axis.Horizontal ?
                        VW.App.Resources.InstallationApp.HorizontalHomingExecuting :
                        VW.App.Resources.InstallationApp.VerticalHomingExecuting;
                    break;

                case MessageStatus.OperationError:
                    res = axisToCalibrate == Axis.Horizontal ?
                        VW.App.Resources.InstallationApp.HorizontalHomingError :
                        VW.App.Resources.InstallationApp.VerticalHomingError;
                    break;
            }
            return res;
        }

        private void OnAxisSwitched(NotificationMessageUI<SwitchAxisMessageData> message)
        {
            if (message == null || message.Data is null)
            {
                return;
            }

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.NoteString = VW.App.Resources.InstallationApp.SwitchEngineStarted;
                    break;

                case MessageStatus.OperationEnd:
                    this.NoteString = VW.App.Resources.InstallationApp.SwitchEngineCompleted;
                    break;

                case MessageStatus.OperationError:
                    this.NoteString = VW.App.Resources.InstallationApp.SwitchEngineError;
                    break;
            }
        }

        private void OnCalibrationStepCompleted(NotificationMessageUI<CalibrateAxisMessageData> message)
        {
            if (message.Status == MessageStatus.OperationExecuting
                ||
                message.Status == MessageStatus.OperationError)
            {
                this.NoteString = string.Format(
                    this.GetStringByCalibrateAxisMessageData(message.Data.AxisToCalibrate, message.Status),
                    message.Data.CurrentStepCalibrate,
                    message.Data.MaxStepCalibrate);

                if (message.Status == MessageStatus.OperationError)
                {
                    this.IsExecutingProcedure = false;
                }
            }
        }

        private void OnHomingProcedureStatusChanged(MessageNotifiedEventArgs message)
        {
            if (message.NotificationMessage is NotificationMessageUI<HomingMessageData> h)
            {
                switch (h.Status)
                {
                    case MessageStatus.OperationStart:
                        this.NoteString = VW.App.Resources.InstallationApp.HorizontalHomingStarted;
                        this.IsWaitingForResponse = false;
                        this.IsExecutingProcedure = true;
                        break;

                    case MessageStatus.OperationEnd:
                        this.NoteString = VW.App.Resources.InstallationApp.HorizontalHomingCompleted;
                        this.IsWaitingForResponse = false;
                        this.IsExecutingProcedure = false;
                        break;

                    case MessageStatus.OperationError:
                        this.NoteString = VW.App.Resources.InstallationApp.HorizontalHomingError;
                        this.IsWaitingForResponse = false;
                        this.IsExecutingProcedure = false;
                        break;
                }
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
        }

        private void SubscribeToEvents()
        {
            this.receivedSwitchAxisUpdateToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SwitchAxisMessageData>>()
                .Subscribe(
                    message => this.OnAxisSwitched(message),
                    ThreadOption.UIThread,
                    false);

            this.receivedCalibrateAxisUpdateToken = this.EventAggregator
                .GetEvent<NotificationEventUI<CalibrateAxisMessageData>>()
                .Subscribe(
                    message => this.OnCalibrationStepCompleted(message),
                    ThreadOption.UIThread,
                    false);

            this.receiveHomingUpdateToken = this.EventAggregator
                .GetEvent<NotificationEventUI<HomingMessageData>>()
                .Subscribe(
                message =>
                {
                    this.OnHomingProcedureStatusChanged(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.UIThread,
                false);

            this.receiveExceptionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<InverterExceptionMessageData>>()
                .Subscribe(
                message =>
                {
                    this.OnHomingProcedureStatusChanged(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.UIThread,
                false);

            this.updateCurrentPositionToken = this.EventAggregator // TODO copy this in manual movements
                .GetEvent<NotificationEventUI<CurrentPositionMessageData>>()
                .Subscribe(
                message => this.CurrentPosition = message?.Data?.CurrentPosition,
                ThreadOption.UIThread,
                false);
        }

        #endregion
    }
}
