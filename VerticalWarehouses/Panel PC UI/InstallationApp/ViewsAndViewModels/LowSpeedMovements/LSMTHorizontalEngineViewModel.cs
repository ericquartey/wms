using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.InstallationApp.Interfaces;
using Ferretto.VW.InstallationApp.Resources;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTHorizontalEngineViewModel : BindableBase, ILSMTHorizontalEngineViewModel
    {
        #region Fields

        private readonly string contentType = ConfigurationManager.AppSettings["HttpPostContentTypeJSON"];

        private readonly IEventAggregator eventAggregator;

        private readonly string executeMovementPath = ConfigurationManager.AppSettings["InstallationExecuteMovement"];

        private readonly string installationUrl = ConfigurationManager.AppSettings["InstallationController"];

        private readonly string stopCommandPath = ConfigurationManager.AppSettings["InstallationStopCommand"];

        private string currentPosition;

        private DelegateCommand moveBackwardButtonCommand;

        private DelegateCommand moveForwardButtonCommand;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateCurrentPositionToken;

        #endregion

        #region Constructors

        public LSMTHorizontalEngineViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public DelegateCommand MoveBackwardButtonCommand => this.moveBackwardButtonCommand ??
                    (this.moveBackwardButtonCommand = new DelegateCommand(async () => await this.MoveBackHorizontalAxisHandlerAsync()));

        public DelegateCommand MoveForwardButtonCommand => this.moveForwardButtonCommand ??
            (this.moveForwardButtonCommand = new DelegateCommand(async () => await this.MoveForwardHorizontalAxisHandlerAsync()));

        public DelegateCommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopHorizontalAxisHandlerAsync()));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void OnEnterView()
        {
            this.updateCurrentPositionToken = this.eventAggregator.GetEvent<MAS_Event>()
                .Subscribe(
                message => this.UpdateCurrentPosition(message.Data),
                ThreadOption.PublisherThread,
                false,
                message => message.NotificationType == NotificationType.CurrentPosition || message.NotificationType == NotificationType.CurrentActionStatus);
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.updateCurrentPositionToken);
        }

        public void UpdateCurrentPosition(INotificationMessageData data)
        {
            if (data is INotificationActionUpdatedMessageData parsedData)
            {
                this.CurrentPosition = parsedData.CurrentEncoderPosition.ToString();
            }
        }

        private async Task MoveBackHorizontalAxisHandlerAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            var messageData = new MovementMessageDataDTO(0, 1, 50u, -100m);
            var json = JsonConvert.SerializeObject(messageData);
            HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
            await client.PostAsync(new Uri(string.Concat(this.installationUrl, this.executeMovementPath)), httpContent);
        }

        private async Task MoveForwardHorizontalAxisHandlerAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            var messageData = new MovementMessageDataDTO(0, 1, 50u, 100m);
            var json = JsonConvert.SerializeObject(messageData);
            HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
            await client.PostAsync(new Uri(string.Concat(this.installationUrl, this.executeMovementPath)), httpContent);
        }

        private async Task StopHorizontalAxisHandlerAsync()
        {
            await new HttpClient().GetAsync(new Uri(string.Concat(this.installationUrl, this.stopCommandPath)));
        }

        #endregion
    }
}
