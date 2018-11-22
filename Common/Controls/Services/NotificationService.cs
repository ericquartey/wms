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
        private const int RetryConnectionTimeout = 10000;
        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private readonly string healthPath;
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
                System.Diagnostics.Debug.WriteLine($"Hub connection failed. Retrying in {RetryConnectionTimeout / 1000.0} seconds ...");

                await Task.Delay(RetryConnectionTimeout);

                await this.connection?.StartAsync();
            }
        }

        private async Task ConnectAsync()
        {
            System.Diagnostics.Debug.WriteLine("Hub connecting...");
            await this.connection.StartAsync();
            System.Diagnostics.Debug.WriteLine("Hub connected.");
            await this.connection.SendAsync(HealthIsOnLineMessage);
            System.Diagnostics.Debug.WriteLine("Message to hub sent.");
        }

        private async Task InitializeAsync()
        {
            this.connection = new Microsoft.AspNetCore.SignalR.Client.HubConnectionBuilder()
             .WithUrl(new Uri(new Uri(this.url), this.healthPath).AbsoluteUri)
             .Build();

            this.connection.On(HealthIsOnLineMessage, this.OnIsOnLine_MessageReceived);

            this.connection.Closed += async (error) =>
            {
                System.Diagnostics.Debug.WriteLine("Connection to hub closed.");

                if (this.isConnected)
                {
                    this.eventService.Invoke(new StatusEventArgs() { IsSchedulerOnline = false });
                }

                this.isConnected = false;
                System.Diagnostics.Debug.WriteLine("Retrying connection to hub...");

                await Task.Delay(RetryConnectionTimeout);
                await this.ConnectAsync();
            };

            await this.ConnectAsync();
        }

        private void OnIsOnLine_MessageReceived()
        {
            System.Diagnostics.Debug.WriteLine($"Message {HealthIsOnLineMessage} received from server.");
            this.isConnected = true;
            this.eventService.Invoke(new StatusEventArgs() { IsSchedulerOnline = true });
        }

        #endregion Methods
    }
}
