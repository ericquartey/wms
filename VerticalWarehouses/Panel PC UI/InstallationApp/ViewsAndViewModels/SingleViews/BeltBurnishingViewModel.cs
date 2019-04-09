using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Windows.Input;
using Ferretto.VW.Common_Utils.DTOs;
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

        private readonly string contentType = ConfigurationManager.AppSettings["HttpPostContentTypeJSON"];

        private string beltBurnishingController = ConfigurationManager.AppSettings.Get("InstallationExecuteBeltBurnishing");

        private IUnityContainer container;

        private string cyclesQuantity;

        private IEventAggregator eventAggregator;

        private string getDecimalValuesController = ConfigurationManager.AppSettings.Get("InstallationGetDecimalConfigurationValues");

        private string installationController = ConfigurationManager.AppSettings.Get("InstallationController");

        private bool isStartButtonActive = true;

        private bool isStopButtonActive;

        private string lowerBound;

        private ICommand startButtonCommand;

        private ICommand stopButtonCommand;

        private string stopController = ConfigurationManager.AppSettings.Get("InstallationStopCommand");

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

        public ICommand StartButtonCommand => this.startButtonCommand ?? (this.startButtonCommand = new DelegateCommand(this.ExecuteStartButtonCommand));

        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(this.ExecuteStopButtonCommand));

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

        public async void GetParameterValues()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(new Uri(this.installationController + this.getDecimalValuesController + "UpperBound"));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                this.UpperBound = response.Content.ReadAsAsync<decimal>().Result.ToString();
            }
            response = null;
            response = await client.GetAsync(new Uri(this.installationController + this.getDecimalValuesController + "LowerBound"));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                this.LowerBound = response.Content.ReadAsAsync<decimal>().Result.ToString();
            }
            response = null;
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public void OnEnterView()
        {
            this.GetParameterValues();

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

        private async void ExecuteStartButtonCommand()
        {
            try
            {
                this.IsStartButtonActive = false;
                this.IsStopButtonActive = true;

                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                var messageData = new BeltBurnishingMessageDataDTO(Convert.ToInt32(this.cyclesQuantity));
                var json = JsonConvert.SerializeObject(messageData);
                HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
                await client.PostAsync(new Uri(string.Concat(this.installationController, this.beltBurnishingController)), httpContent);
            }
            catch (Exception exc)
            {
                var message = exc.Message;
            }
        }

        private async void ExecuteStopButtonCommand()
        {
            try
            {
                var client = new HttpClient();
                await client.GetStringAsync(new Uri(this.installationController + this.stopController));

                this.IsStopButtonActive = false;
                this.IsStartButtonActive = true;
            }
            catch (Exception exc)
            {
                var message = exc.Message;
            }
        }

        #endregion
    }
}
