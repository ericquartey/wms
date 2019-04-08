using System;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.InstallationApp.Resources;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTShutterEngineViewModel : BindableBase, ILSMTShutterEngineViewModel
    {
        #region Fields

        private readonly string contentType = ConfigurationManager.AppSettings["HttpPostContentTypeJSON"];

        private readonly string executeMovementPath = ConfigurationManager.AppSettings["InstallationExecuteMovement"];

        private readonly string installationUrl = ConfigurationManager.AppSettings["InstallationController"];

        private readonly string stopCommandPath = ConfigurationManager.AppSettings["InstallationStopCommand"];

        private DelegateCommand closeButtonCommand;

        private string currentPosition;

        private IEventAggregator eventAggregator;

        private DelegateCommand openButtonCommand;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateCurrentPositionToken;

        #endregion

        #region Constructors

        public LSMTShutterEngineViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public DelegateCommand CloseButtonCommand => this.closeButtonCommand ?? (this.closeButtonCommand = new DelegateCommand(async () => await this.CloseShutterAsync()));

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public DelegateCommand OpenButtonCommand => this.openButtonCommand ?? (this.openButtonCommand = new DelegateCommand(async () => await this.OpenShutterAsync()));

        public DelegateCommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopShutterAsync()));

        #endregion

        #region Methods

        public async Task CloseShutterAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            var messageData = new MovementMessageDataDTO(-100m, 0, 1, 50u);
            var json = JsonConvert.SerializeObject(messageData);
            HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
            await client.PostAsync(new Uri(string.Concat(this.installationUrl, this.executeMovementPath)), httpContent);
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OpenShutterAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            var messageData = new MovementMessageDataDTO(100m, 0, 1, 50u);
            var json = JsonConvert.SerializeObject(messageData);
            HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
            await client.PostAsync(new Uri(string.Concat(this.installationUrl, this.executeMovementPath)), httpContent);
        }

        public async Task StopShutterAsync()
        {
            await new HttpClient().GetAsync(new Uri(string.Concat(this.installationUrl, this.stopCommandPath)));
        }

        public void SubscribeMethodToEvent()
        {
            this.updateCurrentPositionToken = this.eventAggregator.GetEvent<MAS_Event>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data.CurrentPosition),
                ThreadOption.PublisherThread,
                false,
                message => message.NotificationType == NotificationType.CurrentPosition || message.NotificationType == NotificationType.CurrentActionStatus);
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.updateCurrentPositionToken);
        }

        public void UpdateCurrentPosition(decimal? currentPosition)
        {
            this.CurrentPosition = currentPosition?.ToString();
        }

        #endregion
    }
}
