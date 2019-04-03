using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.InstallationApp.Resources;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTVerticalEngineViewModel : BindableBase, ILSMTVerticalEngineViewModel
    {
        #region Fields

        public IUnityContainer Container;

        private readonly string contentType = ConfigurationManager.AppSettings["HttpPostContentTypeJSON"];

        private readonly string executeMovementPath = ConfigurationManager.AppSettings["InstallationExecuteMovement"];

        private readonly string installationUrl = ConfigurationManager.AppSettings["InstallationController"];

        private readonly string stopCommandPath = ConfigurationManager.AppSettings["InstallationStopCommand"];

        private string currentPosition;

        private IEventAggregator eventAggregator;

        private DelegateCommand moveDownButtonCommand;

        private DelegateCommand moveUpButtonCommand;

        private DelegateCommand stopButtonCommand;

        private SubscriptionToken updateCurrentPositionToken;

        #endregion

        #region Constructors

        public LSMTVerticalEngineViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public string CurrentPosition { get => this.currentPosition; set => this.SetProperty(ref this.currentPosition, value); }

        public DelegateCommand MoveDownButtonCommand => this.moveDownButtonCommand ?? (this.moveDownButtonCommand = new DelegateCommand(async () => await this.MoveDownVerticalAxisAsync()));

        public DelegateCommand MoveUpButtonCommand => this.moveUpButtonCommand ?? (this.moveUpButtonCommand = new DelegateCommand(async () => await this.MoveUpVerticalAxisAsync()));

        public DelegateCommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopVerticalAxisAsync()));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.Container = container;
        }

        public async Task MoveDownVerticalAxisAsync()
        {
            await new HttpClient().GetAsync(new Uri("http://localhost:5000/api/Test/UpdateCurrentPositionTest"));
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Accept.Clear();
            //var messageData = new MovementMessageDataDTO(-100m, 0, 1, 50u);
            //var json = JsonConvert.SerializeObject(messageData);
            //HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
            //await client.PostAsync(new Uri(string.Concat(this.installationUrl, this.executeMovementPath)), httpContent);
        }

        public async Task MoveUpVerticalAxisAsync()
        {
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Accept.Clear();
            //var messageData = new MovementMessageDataDTO(100m, 0, 1, 50u);
            //var json = JsonConvert.SerializeObject(messageData);
            //HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
            //await client.PostAsync(new Uri(string.Concat(this.installationUrl, this.executeMovementPath)), httpContent);
        }

        public async Task StopVerticalAxisAsync()
        {
            //await new HttpClient().GetAsync(new Uri(string.Concat(this.installationUrl, this.stopCommandPath)));
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
