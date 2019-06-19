using System;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.Hubs.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    internal class DataHubClient : IDataHubClient
    {
        #region Fields

        private const int DefaultMaxReconnectTimeout = 5000;

        private readonly Uri endpoint;

        private readonly Random random = new Random();

        private HubConnection connection;

        #endregion

        #region Constructors

        public DataHubClient(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            this.endpoint = uri;
        }

        #endregion

        #region Events

        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        public event EventHandler<EntityChangedEventArgs> EntityChanged;

        public event EventHandler<MachineStatusUpdatedEventArgs> MachineStatusUpdated;

        #endregion

        #region Properties

        public int MaxReconnectTimeoutMilliseconds { get; set; } = DefaultMaxReconnectTimeout;

        #endregion

        #region Methods

        public async Task ConnectAsync()
        {
            while (this.connection?.State != HubConnectionState.Connected)
            {
                try
                {
                    this.Initialize();

                    await this.connection.StartAsync();

                    this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(true));
                }
                catch
                {
                    await this.WaitForReconnectionAsync();
                }
            }
        }

        public async Task DisconnectAsync()
        {
            await this.connection?.StopAsync();

            this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(false));
        }

        private void EntityChangedMessageReceived(EntityChangedHubEvent e)
        {
            this.EntityChanged?.Invoke(this, new EntityChangedEventArgs(e.EntityType, e.Id, e.Operation));
        }

        private void Initialize()
        {
            if (this.connection != null)
            {
                return;
            }

            this.connection = new HubConnectionBuilder()
                .WithUrl(this.endpoint.AbsoluteUri)
                .Build();

            this.connection.On<EntityChangedHubEvent>(
                nameof(IDataHub.EntityUpdated),
                this.EntityChangedMessageReceived);

            this.connection.On<Hubs.Models.MachineStatus>(
                nameof(IDataHub.MachineStatusUpdated),
                this.MachineStatusUpdatedMessageReceived);

            this.connection.Closed += async (error) =>
            {
                this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(false));

                await this.ConnectAsync();
            };
        }

        private void MachineStatusUpdatedMessageReceived(Hubs.Models.MachineStatus machineStatus)
        {
            this.MachineStatusUpdated?.Invoke(this, new MachineStatusUpdatedEventArgs(machineStatus));
        }

        private async Task WaitForReconnectionAsync()
        {
            var reconnectionTime = this.random.Next(0, this.MaxReconnectTimeoutMilliseconds);

            await Task.Delay(reconnectionTime);
        }

        #endregion
    }
}
