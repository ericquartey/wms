using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Practices.ServiceLocation;
using NLog;

namespace Ferretto.Common.Controls.Services
{
    public class NotificationService : INotificationService
    {
        #region Fields

        private const int MaxRetryConnectionTimeout = 10000;

        private const string MissionUpdatedMessage = "MissionUpdated";

        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();

        private readonly Logger logger;

        private readonly Random random = new Random();

        private readonly string schedulerHubPath;

        private readonly string url;

        private HubConnection connection;

        private bool isServiceHubConnected;

        #endregion

        #region Constructors

        public NotificationService()
        {
            this.url = ConfigurationManager.AppSettings["NotificationHubEndpoint"];
            this.schedulerHubPath = ConfigurationManager.AppSettings["SchedulerHubPath"];
            this.logger = LogManager.GetCurrentClassLogger();
        }

        #endregion

        #region Properties

        public bool IsServiceHubConnected
        {
            get => this.isServiceHubConnected;
            private set
            {
                if (value != this.isServiceHubConnected)
                {
                    this.isServiceHubConnected = value;
                    this.eventService.Invoke(new StatusPubSubEvent() { IsSchedulerOnline = this.isServiceHubConnected });
                }
            }
        }

        #endregion

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
                await this.WaitForReconnection();
                await this.connection?.StartAsync();
            }
        }

        private async Task ConnectAsync()
        {
            while (!this.IsServiceHubConnected)
            {
                try
                {
                    this.logger.Trace("Hub connecting...");
                    await this.connection.StartAsync();
                    this.logger.Trace("Hub connected.");
                    this.IsServiceHubConnected = true;
                }
                catch (Exception ex)
                {
                    this.logger.Warn(ex, "Connection failed.");
                    await this.WaitForReconnection();
                }
            }
        }

        private async Task InitializeAsync()
        {
            this.connection = new HubConnectionBuilder()
                .WithUrl(new Uri(new Uri(this.url), this.schedulerHubPath).AbsoluteUri)
                .Build();

            this.connection.On(MissionUpdatedMessage, (int id) => this.MissionUpdated_MessageReceived(id));

            this.connection.Closed += async (error) =>
            {
                this.logger.Debug("Connection to hub closed.");
                this.IsServiceHubConnected = false;
                await this.WaitForReconnection();
                await this.ConnectAsync();
            };

            await this.ConnectAsync();
        }

        private void MissionUpdated_MessageReceived(int id)
        {
            this.logger.Debug($"Message {MissionUpdatedMessage} received from server");
            this.eventService.Invoke(new ModelChangedPubSubEvent<Mission, int>(id));
        }

        private async Task WaitForReconnection()
        {
            var reconnectionTime = this.random.Next(0, MaxRetryConnectionTimeout);
            this.logger.Debug($"Retrying connection in {reconnectionTime / 1000} seconds...");
            await Task.Delay(reconnectionTime);
        }

        #endregion
    }
}
