using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.DTOs;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTCarouselViewModel : BindableBase, ILSMTCarouselViewModel
    {
        #region Fields

        private readonly string contentType = ConfigurationManager.AppSettings["HttpPostContentTypeJSON"];

        private readonly string executeMovementPath = ConfigurationManager.AppSettings["InstallationExecuteMovement"];

        private readonly string installationUrl = ConfigurationManager.AppSettings["InstallationController"];

        private readonly string stopCommandPath = ConfigurationManager.AppSettings["InstallationStopCommand"];

        private DelegateCommand closeButtonCommand;

        private IUnityContainer container;

        private IEventAggregator eventAggregator;

        private DelegateCommand openButtonCommand;

        private DelegateCommand stopButtonCommand;

        #endregion

        #region Constructors

        public LSMTCarouselViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public DelegateCommand CloseButtonCommand => this.closeButtonCommand ?? (this.closeButtonCommand = new DelegateCommand(async () => await this.CloseCarouselAsync()));

        public DelegateCommand OpenButtonCommand => this.openButtonCommand ?? (this.openButtonCommand = new DelegateCommand(async () => await this.OpenCarouselAsync()));

        public DelegateCommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopCarouselAsync()));

        #endregion

        #region Methods

        public async Task CloseCarouselAsync()
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

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public async Task OpenCarouselAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            var messageData = new MovementMessageDataDTO(100m, 0, 1, 50u);
            var json = JsonConvert.SerializeObject(messageData);
            HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
            await client.PostAsync(new Uri(string.Concat(this.installationUrl, this.executeMovementPath)), httpContent);
        }

        public async Task StopCarouselAsync()
        {
            await new HttpClient().GetAsync(new Uri(string.Concat(this.installationUrl, this.stopCommandPath)));
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
