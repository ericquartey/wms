using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.SignalR.Client;
using NLog;

namespace Ferretto.Common.Controls.Services
{
    public class NotificationService : INotificationService
    {
        #region Fields

        private const string BUSINESSMODELNAMESPACE = "Ferretto.Common";

        private const int MaxRetryConnectionTimeout = 10000;

        private readonly IEventService eventService;

        private readonly Logger logger;

        private readonly Random random = new Random();

        private readonly string schedulerHubPath;

        private readonly string url;

        private HubConnection connection;

        private bool isServiceHubConnected;

        #endregion

        #region Constructors

        public NotificationService(IEventService eventService)
        {
            this.eventService = eventService;
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

        public static IPubSubEvent GetInstanceOfModelChanged(EntityChangedHubEvent entityChanged)
        {
            var entity = Type.GetType($"{BUSINESSMODELNAMESPACE}.{nameof(Ferretto.WMS.App.Core.Models)}.{entityChanged.EntityType}," +
                                      $"{BUSINESSMODELNAMESPACE}.{nameof(Ferretto.WMS.App.Core.Models)}");
            var constructedClass = typeof(ModelChangedPubSubEvent<,>).MakeGenericType(entity, typeof(int));
            return Activator.CreateInstance(constructedClass, entityChanged.Id) as IPubSubEvent;
        }

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

            this.connection.On(nameof(ISchedulerHub.EntityUpdated), (EntityChangedHubEvent entityChangedHubEvent) => this.MessageReceived(entityChangedHubEvent));

            this.connection.Closed += async (error) =>
            {
                this.logger.Debug("Connection to hub closed.");
                this.IsServiceHubConnected = false;
                await this.WaitForReconnection();
                await this.ConnectAsync();
            };

            await this.ConnectAsync();
        }

        private void MessageReceived(EntityChangedHubEvent entityChanged)
        {
            this.logger.Debug($"Message {entityChanged.EntityType}, operation {entityChanged.Operation} received from server");
            switch (entityChanged.Operation)
            {
                case HubEntityOperation.Updated:
                    var modelInstance = GetInstanceOfModelChanged(entityChanged);
                    this.eventService.DynamicInvoke(modelInstance);
                    break;
            }
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
