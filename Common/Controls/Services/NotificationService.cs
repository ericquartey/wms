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

        private const string MissionCompletedMessage = "MissionCompleted";

        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();

        private readonly Logger logger;

        private readonly Random random = new Random();

        private readonly string schedulerHubPath;

        private readonly string url;

        private HubConnection connection;

        private bool isConnected;

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

        public bool IsConnected
        {
            get => this.isConnected;
            private set
            {
                if (value != this.isConnected)
                {
                    this.isConnected = value;
                    this.eventService.Invoke(new StatusPubSubEvent() { IsSchedulerOnline = this.isConnected });
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
            while (!this.IsConnected)
            {
                try
                {
                    this.logger.Trace("Hub connecting...");
                    await this.connection.StartAsync();
                    this.logger.Trace("Hub connected.");
                    this.IsConnected = true;
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

            this.connection.On(MissionCompletedMessage, (int id) => this.MissionCompleted_MessageReceived(id));

            this.connection.Closed += async (error) =>
            {
                this.logger.Debug("Connection to hub closed.");
                this.IsConnected = false;
                await this.WaitForReconnection();
                await this.ConnectAsync();
            };

            await this.ConnectAsync();
        }

        private void MissionCompleted_MessageReceived(int id)
        {
            this.logger.Debug($"Message {MissionCompletedMessage} received from server");
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
