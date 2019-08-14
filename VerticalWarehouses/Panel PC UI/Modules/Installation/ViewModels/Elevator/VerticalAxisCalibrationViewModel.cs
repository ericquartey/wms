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
using Prism.Mvvm;
using Axis = Ferretto.VW.CommonUtils.Messages.Enumerations.Axis;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalAxisCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineHomingService homingService;

        private decimal? currentPosition;

        private decimal lowerBound;

        private string noteString = VW.App.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;

        private decimal offset;

        private SubscriptionToken receivedCalibrateAxisUpdateToken;

        private SubscriptionToken receivedSwitchAxisUpdateToken;

        private SubscriptionToken receiveExceptionToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private decimal resolution;

        private ICommand startCommand;

        private ICommand stopCommand;

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

        public decimal LowerBound
        {
            get => this.lowerBound;
            set => this.SetProperty(ref this.lowerBound, value);
        }

        public BindableBase NavigationViewModel { get; set; }

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
            (this.startCommand = new DelegateCommand(async () => await this.ExecuteStartCommandAsync()));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(async () => await this.ExecuteStopCommandAsync()));

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

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task GetParameterValuesAsync()
        {
            try
            {
                this.CurrentPosition = null;

                const string category = "VerticalAxis";
                this.UpperBound = await this.homingService.GetDecimalConfigurationParameterAsync(category, "UpperBound");
                this.LowerBound = await this.homingService.GetDecimalConfigurationParameterAsync(category, "LowerBound");
                this.Offset = await this.homingService.GetDecimalConfigurationParameterAsync(category, "Offset");
                this.Resolution = await this.homingService.GetDecimalConfigurationParameterAsync(category, "Resolution");

                await this.homingService.NotifyCurrentAxisAxisAsync();
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;

            await this.GetParameterValuesAsync();

            this.receivedSwitchAxisUpdateToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SwitchAxisMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentActionStatus(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.receivedCalibrateAxisUpdateToken = this.EventAggregator
                .GetEvent<NotificationEventUI<CalibrateAxisMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentActionStatus(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.receiveHomingUpdateToken = this.EventAggregator
                .GetEvent<NotificationEventUI<HomingMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentActionStatus(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.receiveExceptionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<InverterExceptionMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentActionStatus(new MessageNotifiedEventArgs(message));
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

        private void CheckInputsCorrectness()
        {
            var isStartButtonActive = // TODO
                    this.lowerBound > 0
                    &&
                    this.lowerBound < this.upperBound
                    &&
                    this.upperBound > 0
                    &&
                    this.resolution > 0
                    &&
                    this.offset > 0;
        }

        private async Task ExecuteStartCommandAsync()
        {
            try
            {
                await this.homingService.ExecuteAsync();
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private async Task ExecuteStopCommandAsync()
        {
            try
            {
                await this.homingService.StopAsync();

                this.NoteString = VW.App.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
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

        private void UpdateCurrentActionStatus(MessageNotifiedEventArgs message)
        {
            if (message.NotificationMessage is NotificationMessageUI<SwitchAxisMessageData> s)
            {
                switch (s.Status)
                {
                    case MessageStatus.OperationStart:
                        this.NoteString = VW.App.Resources.InstallationApp.SwitchEngineStarted;
                        break;

                    case MessageStatus.OperationEnd:
                        this.NoteString = VW.App.Resources.InstallationApp.SwitchEngineCompleted;
                        break;

                    case MessageStatus.OperationError:
                        this.NoteString = VW.App.Resources.InstallationApp.SwitchEngineError;
                        //this.IsStartButtonActive = true;
                        //this.IsStopButtonActive = false;
                        break;
                }
            }

            if (message.NotificationMessage is NotificationMessageUI<CalibrateAxisMessageData> c)
            {
                if (c.Status == MessageStatus.OperationExecuting ||
                    c.Status == MessageStatus.OperationError)
                {
                    this.NoteString = string.Format(
                        this.GetStringByCalibrateAxisMessageData(c.Data.AxisToCalibrate, c.Status),
                        c.Data.CurrentStepCalibrate,
                        c.Data.MaxStepCalibrate);

                    if (c.Status == MessageStatus.OperationError)
                    {
                        //this.IsStartButtonActive = true;
                        // this.IsStopButtonActive = false;
                    }
                }
            }

            if (message.NotificationMessage is NotificationMessageUI<HomingMessageData> h)
            {
                switch (h.Status)
                {
                    case MessageStatus.OperationStart:
                        this.NoteString = VW.App.Resources.InstallationApp.HorizontalHomingStarted;
                        break;

                    case MessageStatus.OperationEnd:
                        this.NoteString = VW.App.Resources.InstallationApp.HorizontalHomingCompleted;
                        //this.IsStartButtonActive = true;
                        //this.IsStopButtonActive = false;
                        break;

                    case MessageStatus.OperationError:
                        this.NoteString = VW.App.Resources.InstallationApp.HorizontalHomingError;
                        //this.IsStartButtonActive = true;
                        //this.IsStopButtonActive = false;
                        break;
                }
            }

            if (message.NotificationMessage is NotificationMessageUI<InverterExceptionMessageData> f)
            {
                switch (f.Status)
                {
                    case MessageStatus.OperationError:
                        this.NoteString = f.Description;
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
