using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Ferretto.VW.Common_Utils.DTOs;
using Newtonsoft.Json;

namespace Ferretto.VW.InstallationApp
{
    public partial class LSMTHorizontalEngineView : UserControl
    {
        #region Fields

        private string contentType = ConfigurationManager.AppSettings["HttpPostContentTypeJSON"];

        private string executeMovementPath = ConfigurationManager.AppSettings["InstallationExecuteMovement"];

        private string installationUrl = ConfigurationManager.AppSettings["InstallationController"];

        private string stopCommandPath = ConfigurationManager.AppSettings["InstallationStopCommand"];

        #endregion

        #region Constructors

        public LSMTHorizontalEngineView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private async void MoveBackHorizontalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            var messageData = new MovementMessageDataDTO(-100m, 0, 1, 50u);
            var json = JsonConvert.SerializeObject(messageData);
            HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
            await client.PostAsync(new Uri(string.Concat(this.installationUrl, this.executeMovementPath)), httpContent);
        }

        private async void MoveForwardHorizontalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            var messageData = new MovementMessageDataDTO(100m, 0, 1, 50u);
            var json = JsonConvert.SerializeObject(messageData);
            HttpContent httpContent = new StringContent(json, Encoding.UTF8, this.contentType);
            await client.PostAsync(new Uri(string.Concat(this.installationUrl, this.executeMovementPath)), httpContent);
        }

        private async void StopHorizontalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            await new HttpClient().GetAsync(new Uri(string.Concat(this.installationUrl, this.stopCommandPath)));
        }

        #endregion
    }
}
