using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS_Utils.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.InstallationApp
{
    public class VerticalAxisCalibrationViewModel : BindableBase, IVerticalAxisCalibrationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private IHomingService homingService;

        private bool isStartButtonActive = true;

        private bool isStopButtonActive = true;

        private string lowerBound;

        private string noteString = App.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;

        private string offset;

        private SubscriptionToken receivedCalibrateAxisUpdateToken;

        private SubscriptionToken receivedSwitchAxisUpdateToken;

        private SubscriptionToken receiveExceptionToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private string resolution;

        private ICommand startButtonCommand;

        private ICommand stopButtonCommand;

        private string upperBound;

        #endregion

        #region Constructors

        public VerticalAxisCalibrationViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
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
                this.UpperBound = (await this.homingService.GetDecimalConfigurationParameterAsync(Category, "UpperBound")).ToString();
                this.LowerBound = (await this.homingService.GetDecimalConfigurationParameterAsync(Category, "LowerBound")).ToString();
                this.Offset = (await this.homingService.GetDecimalConfigurationParameterAsync(Category, "Offset")).ToString();
                this.Resolution = (await this.homingService.GetDecimalConfigurationParameterAsync(Category, "Resolution")).ToString("##.##");
            }
            catch (SwaggerException)
            {
                this.NoteString = App.Resources.InstallationApp.ErrorRetrievingConfigurationData;
            }
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.homingService = this.container.Resolve<IHomingService>();
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
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<SwitchAxisMessageData>>().Unsubscribe(this.receivedSwitchAxisUpdateToken);
            this.eventAggregator.GetEvent<NotificationEventUI<CalibrateAxisMessageData>>().Unsubscribe(this.receivedCalibrateAxisUpdateToken);
            this.eventAggregator.GetEvent<NotificationEventUI<HomingMessageData>>().Unsubscribe(this.receiveHomingUpdateToken);
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

        private async Task StopButtonMethodAsync()
        {
            try
            {
                await this.homingService.StopAsync();

                this.IsStartButtonActive = true;
                this.IsStopButtonActive = false;
                this.NoteString = App.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;
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
                        this.NoteString = App.Resources.InstallationApp.SwitchEngineStarted;
                        break;

                    case MessageStatus.OperationEnd:
                        this.NoteString = App.Resources.InstallationApp.SwitchEngineCompleted;
                        break;

                    case MessageStatus.OperationError:
                        this.NoteString = App.Resources.InstallationApp.SwitchEngineError;
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;
                        break;
                }
            }

            if (messageUI.NotificationMessage is NotificationMessageUI<CalibrateAxisMessageData> c)
            {
                var type = c.Type;
                switch (c.Status)
                {
                    case MessageStatus.OperationStart:
                        this.NoteString = App.Resources.InstallationApp.HomingStarted;
                        break;

                    case MessageStatus.OperationEnd:
                        this.NoteString = App.Resources.InstallationApp.HomingCompleted;
                        break;

                    case MessageStatus.OperationError:
                        this.NoteString = App.Resources.InstallationApp.HomingError;
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;
                        break;
                }
            }

            if (messageUI.NotificationMessage is NotificationMessageUI<HomingMessageData> h)
            {
                switch (h.Status)
                {
                    case MessageStatus.OperationStart:
                        this.NoteString = App.Resources.InstallationApp.HorizontalHomingStarted;
                        break;

                    case MessageStatus.OperationExecuting:
                        this.NoteString = App.Resources.InstallationApp.HorizontalHomingExecuting;
                        break;

                    case MessageStatus.OperationEnd:
                        this.NoteString = App.Resources.InstallationApp.HorizontalHomingCompleted;
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;
                        break;

                    case MessageStatus.OperationError:
                        this.NoteString = App.Resources.InstallationApp.HorizontalHomingError;
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
