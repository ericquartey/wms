using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls.Services
{
    public class NotificationServiceClient : INotificationServiceClient
    {
        #region Fields

        private const string HealthIsOnLineMessage = "IsOnline";
        private const int MaxRetryConnectionTimeout = 10000;
        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private readonly string healthPath;
        private readonly Random random = new Random();
        private readonly string url;
        private Microsoft.AspNetCore.SignalR.Client.HubConnection connection;
        private bool isConnected;

        #endregion Fields

        #region Constructors

        public NotificationServiceClient()
        {
            this.url = ConfigurationManager.AppSettings["NotificationHubEndpoint"];
            this.healthPath = ConfigurationManager.AppSettings["NotificationHubStatus"];
        }

        #endregion Constructors

        #region Methods

        public async Task EndAsync()
        {
            await this.connection.StopAsync();
        }

        public async Task StartAsync()
        {
            try
            {
                await this.InitializeAsync();
            }
            catch
            {
                var reconnectionTime = this.random.Next(0, MaxRetryConnectionTimeout);

                NLog.LogManager
                   .GetCurrentClassLogger()
                   .Error(string.Format("Connection failed. Retrying in {0} seconds...", reconnectionTime / 1000));

                await Task.Delay(reconnectionTime);

                await this.connection?.StartAsync();
            }
        }

        private async Task ConnectAsync()
        {
            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace(string.Format("Hub connecting..."));

            await this.connection.StartAsync();

            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace(string.Format("Hub connected."));

            await this.connection.SendAsync(HealthIsOnLineMessage);

            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace(string.Format("Message to hub sent."));
        }

        private async Task InitializeAsync()
        {
            this.connection = new Microsoft.AspNetCore.SignalR.Client.HubConnectionBuilder()
             .WithUrl(new Uri(new Uri(this.url), this.healthPath).AbsoluteUri)
             .Build();

            this.connection.On(HealthIsOnLineMessage, this.OnIsOnLine_MessageReceived);

            this.connection.Closed += async (error) =>
            {
                NLog.LogManager
                   .GetCurrentClassLogger()
                   .Debug(string.Format("Connection to hub closed."));

                if (this.isConnected)
                {
                    this.eventService.Invoke(new StatusEventArgs() { IsSchedulerOnline = false });
                }

                this.isConnected = false;

                var reconnectionTime = this.random.Next(0, MaxRetryConnectionTimeout);

                NLog.LogManager
                   .GetCurrentClassLogger()
                   .Error(string.Format("Retrying connection in {0} seconds...", reconnectionTime / 1000));

                await Task.Delay(reconnectionTime);
                await this.ConnectAsync();
            };

            await this.ConnectAsync();
        }

        private void OnIsOnLine_MessageReceived()
        {
            NLog.LogManager
               .GetCurrentClassLogger()
               .Error(string.Format("Message {0} received from server", HealthIsOnLineMessage));

            this.isConnected = true;
            this.eventService.Invoke(new StatusEventArgs() { IsSchedulerOnline = true });
        }

        #endregion Methods
    }
}
