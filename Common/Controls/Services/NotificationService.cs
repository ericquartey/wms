using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Practices.ServiceLocation;
using NLog;

namespace Ferretto.Common.Controls.Services
{
    public class NotificationServiceClient : INotificationServiceClient
    {
        #region Fields

        private const string HealthIsOnLineMessage = "IsOnline";
        private const int MaxRetryConnectionTimeout = 10000;
        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private readonly string healthPath;
        private readonly Logger logger;
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
            this.logger = NLog.LogManager.GetCurrentClassLogger();
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

                this.logger.Warn($"Connection failed. Retrying in {reconnectionTime / 1000} seconds...");

                await Task.Delay(reconnectionTime);

                await this.connection?.StartAsync();
            }
        }

        private async Task ConnectAsync()
        {
            try
            {
                this.logger.Trace("Hub connecting...");

                await this.connection.StartAsync();

                this.logger.Trace("Hub connected.");

                await this.connection.SendAsync(HealthIsOnLineMessage);

                this.logger.Trace("Message to hub sent.");
            }
            catch (Exception ex)
            {
                this.logger.Warn(ex, "Connection failed.");
            }
        }

        private async Task InitializeAsync()
        {
            this.connection = new Microsoft.AspNetCore.SignalR.Client.HubConnectionBuilder()
                .WithUrl(new Uri(new Uri(this.url), this.healthPath).AbsoluteUri)
                .Build();

            this.connection.On(HealthIsOnLineMessage, this.OnIsOnLine_MessageReceived);

            this.connection.Closed += async (error) =>
            {
                this.logger.Debug("Connection to hub closed.");

                if (this.isConnected)
                {
                    this.eventService.Invoke(new StatusPubSubEvent() { IsSchedulerOnline = false });
                }

                this.isConnected = false;

                var reconnectionTime = this.random.Next(0, MaxRetryConnectionTimeout);

                this.logger.Error(string.Format("Retrying connection in {0} seconds...", reconnectionTime / 1000));

                await Task.Delay(reconnectionTime);
                await this.ConnectAsync();
            };

            await this.ConnectAsync();
        }

        private void OnIsOnLine_MessageReceived()
        {
            this.logger.Error(string.Format("Message {0} received from server", HealthIsOnLineMessage));

            this.isConnected = true;
            this.eventService.Invoke(new StatusPubSubEvent() { IsSchedulerOnline = true });
        }

        #endregion Methods
    }
}
