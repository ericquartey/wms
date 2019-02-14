using System;
using System.Threading.Tasks;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    internal class InstallationHubClient : IInstallationHubClient
    {
        #region Fields

        public HubConnection connection;

        #endregion

        #region Constructors

        public InstallationHubClient()
        {
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

        public void InitializeInstallationHubClient(string url, string sensorStatePath)
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

        private void OnSendMessageToAllConnectedClients(string message)
        {
            this.ReceivedMessageToAllConnectedClients?.Invoke(this, message);
        }

        #endregion
    }
}
