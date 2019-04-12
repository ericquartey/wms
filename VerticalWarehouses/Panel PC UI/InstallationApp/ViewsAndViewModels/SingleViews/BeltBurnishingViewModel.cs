using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class BeltBurnishingViewModel : BindableBase, IBeltBurnishingViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private string cyclesQuantity;

        private IInstallationService installationService;

        private bool isStartButtonActive = true;

        private bool isStopButtonActive;

        private string lowerBound;

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

        public string CyclesQuantity
        {
            get => this.cyclesQuantity;
            set
            {
                this.SetProperty(ref this.cyclesQuantity, value);
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
            this.UpperBound = (await this.installationService.GetDecimalConfigurationParameterAsync("GeneralInfo", "UpperBound")).ToString();
            this.LowerBound = (await this.installationService.GetDecimalConfigurationParameterAsync("GeneralInfo", "LowerBound")).ToString();
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.installationService = this.container.Resolve<IInstallationService>();
        }

        public async Task OnEnterViewAsync()
        {
            await this.GetParameterValuesAsync();

            //TEMP this.receivedActionUpdateToken = this.eventAggregator.GetEvent<MAS_Event>().Subscribe(
            //    (msg) => this.UpdateCurrentActionStatus(msg),
            //    ThreadOption.PublisherThread,
            //    false,
            //    message => message.NotificationType == NotificationType.CurrentActionStatus && (message.ActionType == ActionType.BeltBurnishing));
        }

        public void UnSubscribeMethodFromEvent()
        {
            //TEMP this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.receivedActionUpdateToken);
        }

        private void CheckInputsCorrectness()
        {
            if (int.TryParse(this.LowerBound, out var _lowerBound) &&
                int.TryParse(this.CyclesQuantity, out var _cycleQuantity) &&
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

                int.TryParse(this.CyclesQuantity, out var reqCycles);
                var messageData = new BeltBurnishingMessageDataDTO { CyclesQuantity = reqCycles };
                await this.installationService.ExecuteBeltBurnishingAsync(messageData);
            }
            catch (Exception exc)
            {
            }
        }

        private async Task ExecuteStopButtonCommandAsync()
        {
            try
            {
                await this.installationService.StopCommandAsync();

                this.IsStopButtonActive = false;
                this.IsStartButtonActive = true;
            }
            catch (Exception exc)
            {
            }
        }

        #endregion
    }
}
