using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;
using Axis = Ferretto.VW.CommonUtils.Messages.Enumerations.Axis;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews
{
    public class VerticalAxisCalibrationViewModel : BindableBase, IVerticalAxisCalibrationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IHomingMachineService homingService;

        private string currentPosition;

        private bool isStartButtonActive = true;

        private bool isStopButtonActive = false;

        private string lowerBound;

        private string noteString = VW.App.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;

        private string offset;

        private SubscriptionToken receivedCalibrateAxisUpdateToken;

        private SubscriptionToken receivedSwitchAxisUpdateToken;

        private SubscriptionToken receiveExceptionToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private string resolution;

        private ICommand startButtonCommand;

        private ICommand stopButtonCommand;

        private SubscriptionToken updateCurrentPositionToken;

        private string upperBound;

        #endregion

        #region Constructors

        public VerticalAxisCalibrationViewModel(
            IEventAggregator eventAggregator,
            IHomingMachineService homingService)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (homingService == null)
            {
                throw new ArgumentNullException(nameof(homingService));
            }

            this.eventAggregator = eventAggregator;
            this.homingService = homingService;

            this.InputsCorrectionControlEventHandler += this.CheckInputsCorrectness;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Delegates

        public delegate void CheckCorrectnessOnPropertyChangedEventHandler();

        #endregion

        #region Events

        public event CheckCorrectnessOnPropertyChangedEventHandler InputsCorrectionControlEventHandler;

        #endregion

        #region Properties

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public bool IsStartButtonActive { get => this.isStartButtonActive; set => this.SetProperty(ref this.isStartButtonActive, value); }

        public bool IsStopButtonActive { get => this.isStopButtonActive; set => this.SetProperty(ref this.isStopButtonActive, value); }

        public string LowerBound
        {
            get => this.lowerBound;
            set
            {
                this.SetProperty(ref this.lowerBound, value);
                this.InputsCorrectionControlEventHandler();
            }
        }

        public BindableBase NavigationViewModel { get; set; }

        public string NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public string Offset
        {
            get => this.offset;
            set
            {
                this.SetProperty(ref this.offset, value);
                this.InputsCorrectionControlEventHandler();
            }
        }

        public string Resolution
        {
            get => this.resolution;
            set
            {
                this.SetProperty(ref this.resolution, value);
                this.InputsCorrectionControlEventHandler();
            }
        }

        public ICommand StartButtonCommand => this.startButtonCommand ?? (this.startButtonCommand = new DelegateCommand(async () => await this.ExecuteStartButtonCommandAsync()));

        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopButtonMethodAsync()));

        public string UpperBound
        {
            get => this.upperBound;
            set
            {
                this.SetProperty(ref this.upperBound, value);
                this.InputsCorrectionControlEventHandler();
            }
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
            catch (SwaggerException)
            {
                this.NoteString = VW.App.Resources.InstallationApp.ErrorRetrievingConfigurationData;
            }
        }

        public async Task OnEnterViewAsync()
        {
            await this.GetParameterValuesAsync();

            this.receivedSwitchAxisUpdateToken = this.eventAggregator.GetEvent<NotificationEventUI<SwitchAxisMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentActionStatus(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.receivedCalibrateAxisUpdateToken = this.eventAggregator.GetEvent<NotificationEventUI<CalibrateAxisMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentActionStatus(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.receiveHomingUpdateToken = this.eventAggregator.GetEvent<NotificationEventUI<HomingMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentActionStatus(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.receiveExceptionToken = this.eventAggregator.GetEvent<NotificationEventUI<InverterExceptionMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentActionStatus(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.updateCurrentPositionToken = this.eventAggregator.GetEvent<NotificationEventUI<CurrentPositionMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentPosition(message.Data.CurrentPosition);
                },
                ThreadOption.PublisherThread,
                false);
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<SwitchAxisMessageData>>().Unsubscribe(this.receivedSwitchAxisUpdateToken);
            this.eventAggregator.GetEvent<NotificationEventUI<CalibrateAxisMessageData>>().Unsubscribe(this.receivedCalibrateAxisUpdateToken);
            this.eventAggregator.GetEvent<NotificationEventUI<HomingMessageData>>().Unsubscribe(this.receiveHomingUpdateToken);
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
                this.IsStartButtonActive = (_lowerBound > 0 && _lowerBound < _upperBound && _upperBound > 0 && _resolution > 0 && _offset > 0) ? true : false;
            }
            else
            {
                this.IsStartButtonActive = false;
            }
        }

        private async Task ExecuteStartButtonCommandAsync()
        {
            try
            {
                this.IsStartButtonActive = false;
                this.IsStopButtonActive = true;

                await this.homingService.ExecuteAsync();
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from this http get request.";
                throw;
            }
        }

        private string GetStringByCalibrateAxisMessageData(Axis axisToCalibrate, MessageStatus status)
        {
            string res = string.Empty;
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

        private async Task StopButtonMethodAsync()
        {
            try
            {
                await this.homingService.StopAsync();

                this.IsStartButtonActive = true;
                this.IsStopButtonActive = false;
                this.NoteString = VW.App.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from this http get request.";
                throw;
            }
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
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;
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
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;
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
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;
                        break;

                    case MessageStatus.OperationError:
                        this.NoteString = VW.App.Resources.InstallationApp.HorizontalHomingError;
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;
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
