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

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews
{
    public class BeltBurnishingViewModel : BindableBase, IBeltBurnishingViewModel
    {
        #region Fields

        private readonly IMachineBeltBurnishingProcedureService beltBurnishingService;

        private readonly IEventAggregator eventAggregator;

        private string completedCycles;

        private string currentPosition;

        private string cycleQuantity;

        private bool isStartButtonActive = true;

        private bool isStopButtonActive;

        private string lowerBound;

        private SubscriptionToken receivedActionUpdateToken;

        private string requiredCycles;

        private ICommand startButtonCommand;

        private ICommand stopButtonCommand;

        private string upperBound;

        #endregion

        #region Constructors

        public BeltBurnishingViewModel(
            IEventAggregator eventAggregator,
            IMachineBeltBurnishingProcedureService beltBurnishingService)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (beltBurnishingService == null)
            {
                throw new ArgumentNullException(nameof(beltBurnishingService));
            }

            this.eventAggregator = eventAggregator;
            this.beltBurnishingService = beltBurnishingService;

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

        public string CompletedCycles { get => this.completedCycles; set => this.SetProperty(ref this.completedCycles, value); }

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public string CycleQuantity
        {
            get => this.cycleQuantity;
            set
            {
                this.SetProperty(ref this.cycleQuantity, value);
                this.InputsCorrectionControlEventHandler();
            }
        }

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
                var category = "VerticalAxis";
                this.UpperBound = (await this.beltBurnishingService.GetDecimalConfigurationParameterAsync(category, "UpperBound")).ToString();
                this.LowerBound = (await this.beltBurnishingService.GetDecimalConfigurationParameterAsync(category, "LowerBound")).ToString();

                category = "BeltBurnishing";
                this.CycleQuantity = (await this.beltBurnishingService.GetIntegerConfigurationParameterAsync(category, "CycleQuantity")).ToString();
            }
            catch (SwaggerException)
            {
            }
        }

        public async Task OnEnterViewAsync()
        {
            await this.GetParameterValuesAsync();

            this.receivedActionUpdateToken = this.eventAggregator
                .GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(async
                    message =>
                    {
                        await this.UpdateCompletion(new MessageNotifiedEventArgs(message));
                    },
                    ThreadOption.PublisherThread,
                    false);
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>().Unsubscribe(this.receivedActionUpdateToken);
        }

        private void CheckInputsCorrectness()
        {
            if (decimal.TryParse(this.LowerBound, out var _lowerBound) &&
                int.TryParse(this.CycleQuantity, out var _cycleQuantity) &&
                decimal.TryParse(this.UpperBound, out var _upperBound))
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

                int.TryParse(this.CycleQuantity, out var reqCycles);
                decimal.TryParse(this.LowerBound, out var lowerBound);
                decimal.TryParse(this.UpperBound, out var upperBound);

                await this.beltBurnishingService.StartAsync(upperBound, lowerBound, reqCycles);
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        private async Task ExecuteStopButtonCommandAsync()
        {
            try
            {
                await this.beltBurnishingService.StopAsync();
                this.IsStartButtonActive = true;
                this.IsStopButtonActive = false;
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        private async Task UpdateCompletion(MessageNotifiedEventArgs messageUI)
        {
            if (messageUI.NotificationMessage is NotificationMessageUI<PositioningMessageData> cp)
            {
                this.CompletedCycles = cp.Data.ExecutedCycles.ToString();
                this.CurrentPosition = cp.Data.CurrentPosition.ToString();

                switch (cp.Status)
                {
                    case MessageStatus.OperationStart:
                        this.IsStartButtonActive = false;
                        this.IsStopButtonActive = true;
                        break;

                    case MessageStatus.OperationEnd:
                    case MessageStatus.OperationStop:
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;

                        await this.beltBurnishingService.MarkAsCompletedAsync();
                        break;

                    case MessageStatus.OperationError:
                        this.IsStartButtonActive = true;
                        this.IsStopButtonActive = false;
                        break;
                }
            }
        }

        #endregion
    }
}
