using System;
using System.Threading.Tasks;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    public class InstallationHubClient : IContainerInstallationHubClient
    {
        #region Fields

        private readonly HubConnection hubConnection;

        #endregion

        #region Constructors

        public InstallationHubClient(string url, string sensorStatePath)
        {
            this.hubConnection = new HubConnectionBuilder()
              .WithUrl(new Uri(new Uri(url), sensorStatePath).AbsoluteUri)
              .Build();

            this.hubConnection.On<string>("OnSendMessageToAllConnectedClients", this.OnSendMessageToAllConnectedClients);
            this.hubConnection.On<bool[]>("OnSensorsChangedToAllConnectedClients", this.OnSensorsChangedToAllConnectedClients);

            this.hubConnection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await this.hubConnection.StartAsync();
            };
        }

        #endregion

        #region Events

        public event EventHandler<string> ReceivedMessage;

        public event EventHandler<bool[]> SensorsChanged;

        #endregion

        #region Methods

        public async Task ConnectAsync()
        {
            await this.hubConnection.StartAsync();
        }

        public async Task DisconnectAsync()
        {
            await this.hubConnection.DisposeAsync();
        }

        private void OnSendMessageToAllConnectedClients(string message)
        {
            this.ReceivedMessage?.Invoke(this, message);
        }

        private void OnSensorsChangedToAllConnectedClients(bool[] sensorsStates)
        {
            this.SensorsChanged?.Invoke(this, sensorsStates);
        }

        #endregion
    }
}
