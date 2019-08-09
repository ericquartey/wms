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

        private readonly IHomingMachineService homingService;

        private string currentPosition;

        private string lowerBound;

        private string noteString = VW.App.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;

        private string offset;

        private SubscriptionToken receivedCalibrateAxisUpdateToken;

        private SubscriptionToken receivedSwitchAxisUpdateToken;

        private SubscriptionToken receiveExceptionToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private string resolution;

        private ICommand startCommand;

        private ICommand stopCommand;

        private SubscriptionToken updateCurrentPositionToken;

        private string upperBound;

        #endregion

        #region Constructors

        public VerticalAxisCalibrationViewModel(
            IHomingMachineService homingService)
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

        public string CurrentPosition
        {
            get => this.currentPosition;
            set => this.SetProperty(ref this.currentPosition, value);
        }

        public string LowerBound
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

        public string Offset
        {
            get => this.offset;
            set => this.SetProperty(ref this.offset, value);
        }

        public string Resolution
        {
            get => this.resolution;
            set => this.SetProperty(ref this.resolution, value);
        }

        public ICommand StartButtonCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(async () => await this.ExecuteStartCommandAsync()));

        public ICommand StopButtonCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(async () => await this.ExecuteStopCommandAsync()));

        public string UpperBound
        {
            get => this.upperBound;
            set => this.SetProperty(ref this.upperBound, value);
        }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task GetParameterValuesAsync()
        {
            try
            {
                const string Category = "VerticalAxis";
                this.CurrentPosition = "";
                this.UpperBound = (await this.homingService.GetDecimalConfigurationParameterAsync(Category, "UpperBound")).ToString();
                this.LowerBound = (await this.homingService.GetDecimalConfigurationParameterAsync(Category, "LowerBound")).ToString();
                this.Offset = (await this.homingService.GetDecimalConfigurationParameterAsync(Category, "Offset")).ToString();
                this.Resolution = (await this.homingService.GetDecimalConfigurationParameterAsync(Category, "Resolution")).ToString("##.##");
                await this.homingService.GetCurrentPositionAxisAsync();
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        public async Task OnEnterViewAsync()
        {
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

            this.updateCurrentPositionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<CurrentPositionMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentPosition(message.Data.CurrentPosition);
                },
                ThreadOption.PublisherThread,
                false);
        }

        public override void OnNavigated()
        {
            base.OnNavigated();
            this.SohwBack(true);
        }

        public void UnSubscribeMethodFromEvent()
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

        public void UpdateCurrentPosition(decimal currentPosition)
        {
            this.CurrentPosition = currentPosition.ToString();
        }

        private void CheckInputsCorrectness()
        {
            if (decimal.TryParse(this.LowerBound, out var _lowerBound) &&
                decimal.TryParse(this.Offset, out var _offset) &&
                decimal.TryParse(this.Resolution, out var _resolution) &&
                decimal.TryParse(this.UpperBound, out var _upperBound))
            { // TODO: DEFINE AND INSERT VALIDATION LOGIC IN HERE. THESE PROPOSITIONS ARE TEMPORARY
                var isStartButtonActive =
                    _lowerBound > 0
                    &&
                    _lowerBound < _upperBound
                    &&
                    _upperBound > 0
                    &&
                    _resolution > 0
                    &&
                    _offset > 0;
            }
            else
            {
                var isStartButtonActive = false;
            }
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

        private void UpdateCurrentActionStatus(MessageNotifiedEventArgs messageUI)
        {
            if (messageUI.NotificationMessage is NotificationMessageUI<SwitchAxisMessageData> s)
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

            if (messageUI.NotificationMessage is NotificationMessageUI<CalibrateAxisMessageData> c)
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
                        /// this.IsStopButtonActive = false;
                    }
                }
            }

            if (messageUI.NotificationMessage is NotificationMessageUI<HomingMessageData> h)
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

            if (messageUI.NotificationMessage is NotificationMessageUI<InverterExceptionMessageData> f)
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
