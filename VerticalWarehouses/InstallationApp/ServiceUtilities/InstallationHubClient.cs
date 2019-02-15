using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    public class InstallationHubClient
    {
        #region Fields

        public HubConnection connection;

        #endregion

        #region Constructors

        public InstallationHubClient(string url, string sensorStatePath)
        {
            this.connection = new HubConnectionBuilder()
              .WithUrl(new Uri(new Uri(url), sensorStatePath).AbsoluteUri)
              .Build();

            this.connection.On<string>("OnSendMessageToAllConnectedClients", this.OnSendMessageToAllConnectedClients);

            this.connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await this.connection.StartAsync();
            };

            this.connection.StartAsync();
        }

        #endregion

        #region Events

        public event EventHandler<string> ReceivedMessageToAllConnectedClients;

        #endregion

        #region Methods

        public async Task ConnectAsync()
        {
            await this.connection.StartAsync();
        }

        public async Task DisconnectAsync()
        {
            await this.connection.DisposeAsync();
        }

        private void OnSendMessageToAllConnectedClients(string message)
        {
            this.ReceivedMessageToAllConnectedClients?.Invoke(this, message);
        }

        #endregion
    }
}
