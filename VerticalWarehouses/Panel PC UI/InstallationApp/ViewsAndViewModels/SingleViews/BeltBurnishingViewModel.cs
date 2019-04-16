using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class BeltBurnishingViewModel : BindableBase, IBeltBurnishingViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private string completedCycles;

        private IUnityContainer container;

        private string currentPosition;

        private IInstallationService installationService;

        private bool isStartButtonActive = true;

        private bool isStopButtonActive;

        private string lowerBound;

        private SubscriptionToken receivedUpDownRepetitiveUpdateToken;

        private string requiredCycles;

        private ICommand startButtonCommand;

        private ICommand stopButtonCommand;

        private string upperBound;

        #endregion

        #region Constructors

        public BeltBurnishingViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.InputsCorrectionControlEventHandler += this.CheckInputsCorrectness;
        }

        #endregion

        #region Delegates

        public delegate void CheckCorrectnessOnPropertyChangedEventHandler();

        #endregion

        #region Events

        public event CheckCorrectnessOnPropertyChangedEventHandler InputsCorrectionControlEventHandler;

        #endregion

        #region Properties

        public string CompletedCycles { get => this.completedCycles; set => this.SetProperty(ref this.completedCycles, value); }

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

        public string RequiredCycles
        {
            get => this.requiredCycles;
            set
            {
                this.SetProperty(ref this.requiredCycles, value);
                this.InputsCorrectionControlEventHandler();
            }
        }

        public ICommand StartButtonCommand => this.startButtonCommand ?? (this.startButtonCommand = new DelegateCommand(async () => await this.ExecuteStartButtonCommandAsync()));

        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.ExecuteStopButtonCommandAsync()));

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
                this.UpperBound = (await this.installationService.GetDecimalConfigurationParameterAsync("GeneralInfo", "UpperBound")).ToString();
                this.LowerBound = (await this.installationService.GetDecimalConfigurationParameterAsync("GeneralInfo", "LowerBound")).ToString();
            }
            catch (SwaggerException ex)
            {
            }
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.installationService = this.container.Resolve<IInstallationService>();
        }

        public async Task OnEnterViewAsync()
        {
            await this.GetParameterValuesAsync();

            this.receivedUpDownRepetitiveUpdateToken = this.eventAggregator.GetEvent<NotificationEventUI<UpDownRepetitiveMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentUI(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<UpDownRepetitiveMessageData>>().Unsubscribe(this.receivedUpDownRepetitiveUpdateToken);
        }

        private void CheckInputsCorrectness()
        {
            if (int.TryParse(this.LowerBound, out var _lowerBound) &&
                int.TryParse(this.RequiredCycles, out var _cycleQuantity) &&
                int.TryParse(this.UpperBound, out var _upperBound))
            {
                // TODO: DEFINE AND INSERT VALIDATION LOGIC IN HERE. THESE PROPOSITIONS ARE TEMPORARY
                this.IsStartButtonActive = ((_lowerBound > 0) && (_lowerBound < _upperBound) && (_upperBound > 0) && (_cycleQuantity > 0)) ? true : false;
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

                int.TryParse(this.RequiredCycles, out var reqCycles);
                await this.installationService.ExecuteBeltBurnishingAsync(10350, 100, 12);
            }
            catch (Exception)
            {
            }
        }

        private async Task ExecuteStopButtonCommandAsync()
        {
            try
            {
                await this.installationService.StopCommandAsync();
                this.IsStartButtonActive = true;
                this.IsStopButtonActive = false;
            }
            catch (Exception)
            {
            }
        }

        private void UpdateCurrentUI(MessageNotifiedEventArgs messageUI)
        {
            if (messageUI.NotificationMessage is NotificationMessageUI<UpDownRepetitiveMessageData> r)
            {
                switch (r.Status)
                {
                    case MessageStatus.OperationStart:
                        this.CompletedCycles = r.Data.NumberOfCompletedCycles.ToString();
                        this.CurrentPosition = r.Data.CurrentPosition.ToString();
                        this.IsStartButtonActive = false;
                        this.IsStopButtonActive = true;
                        break;

                    case MessageStatus.OperationEnd:
                        this.CompletedCycles = r.Data.NumberOfCompletedCycles.ToString();
                        this.CurrentPosition = r.Data.CurrentPosition.ToString();
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;
                        break;

                    case MessageStatus.OperationStop:
                        this.CompletedCycles = r.Data.NumberOfCompletedCycles.ToString();
                        this.CurrentPosition = r.Data.CurrentPosition.ToString();
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;
                        break;

                    case MessageStatus.OperationError:
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;
                        break;

                    case MessageStatus.OperationExecuting:
                        this.CompletedCycles = r.Data.NumberOfCompletedCycles.ToString();
                        this.CurrentPosition = r.Data.CurrentPosition.ToString();
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
