using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;
using Prism.Commands;
using Prism.Events;
using Axis = Ferretto.VW.CommonUtils.Messages.Enumerations.Axis;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalAxisCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineHomingService homingService;

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

        public VerticalAxisCalibrationViewModel(
            IMachineHomingService homingService)
            : base(Services.PresentationMode.Installator)
        {
            if (homingService == null)
            {
                throw new ArgumentNullException(nameof(homingService));
            }

            this.homingService = homingService;
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

        public async Task GetParameterValuesAsync()
        {
            try
            {
                const string category = "VerticalAxis";
                this.UpperBound = await this.homingService.GetDecimalConfigurationParameterAsync(category, "UpperBound");
                this.LowerBound = await this.homingService.GetDecimalConfigurationParameterAsync(category, "LowerBound");
                this.Offset = await this.homingService.GetDecimalConfigurationParameterAsync(category, "Offset");
                this.Resolution = await this.homingService.GetDecimalConfigurationParameterAsync(category, "Resolution");

                await this.homingService.NotifyCurrentAxisAxisAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;

            await this.GetParameterValuesAsync();

            this.SubscribeToEvents();
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

                await this.homingService.ExecuteAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.IsExecutingProcedure = true;
            }
        }

        private async Task ExecuteStopCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.homingService.StopAsync();

                this.ShowNotification(VW.App.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.IsWaitingForResponse = false;
            }
            finally
            {
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
            if (message == null || message.Data == null)
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
                    ThreadOption.PublisherThread,
                    false);

            this.receivedCalibrateAxisUpdateToken = this.EventAggregator
                .GetEvent<NotificationEventUI<CalibrateAxisMessageData>>()
                .Subscribe(
                    message => this.OnCalibrationStepCompleted(message),
                    ThreadOption.PublisherThread,
                    false);

            this.receiveHomingUpdateToken = this.EventAggregator
                .GetEvent<NotificationEventUI<HomingMessageData>>()
                .Subscribe(
                message =>
                {
                    this.OnHomingProcedureStatusChanged(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.receiveExceptionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<InverterExceptionMessageData>>()
                .Subscribe(
                message =>
                {
                    this.OnHomingProcedureStatusChanged(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.updateCurrentPositionToken = this.EventAggregator // TODO copy this in manual movements
                .GetEvent<NotificationEventUI<CurrentPositionMessageData>>()
                .Subscribe(
                message => this.CurrentPosition = message?.Data?.CurrentPosition,
                ThreadOption.PublisherThread,
                false);
        }

        #endregion
    }
}
