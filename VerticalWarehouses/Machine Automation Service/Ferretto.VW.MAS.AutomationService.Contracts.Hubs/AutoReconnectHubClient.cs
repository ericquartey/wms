using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public abstract class AutoReconnectHubClient : IAutoReconnectHubClient
    {
        #region Fields

        private const int DefaultMaxReconnectTimeout = 5000;

        private static readonly Random Random = new Random();

        private readonly Uri endpoint;

        private HubConnection connection;

        #endregion

        #region Constructors

        protected AutoReconnectHubClient(Uri uri)
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

        #endregion

        #region Properties

        public bool IsConnected => this.connection?.State == HubConnectionState.Connected;

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

                    System.Diagnostics.Debug.WriteLine($"Hub '{this.endpoint}': establishing connection ... ");

                    await this.connection.StartAsync();

                    System.Diagnostics.Debug.WriteLine($"Hub '{this.endpoint}': connection established.");

                    this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(true));
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Hub '{this.endpoint}': connection lost.");
                    await this.WaitForReconnectionAsync();
                }
            }
        }

        public async Task DisconnectAsync()
        {
            await this.connection?.StopAsync();

            this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(false));
        }

        public async Task SendAsync(string methodName)
        {
            await this.connection.SendAsync(methodName);
        }

        protected abstract void RegisterEvents(HubConnection connection);

        private void Initialize()
        {
            if (this.connection != null)
            {
                return;
            }

            var connectionBuilder = new HubConnectionBuilder()
                .WithUrl(this.endpoint.AbsoluteUri);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                connectionBuilder.ConfigureLogging(logging =>
                {
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
            }

            this.connection = connectionBuilder.Build();

            this.RegisterEvents(this.connection);

            this.connection.Closed += async (error) =>
            {
                this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(false));

                await this.ConnectAsync();
            };
        }

        private async Task WaitForReconnectionAsync()
        {
            var reconnectionTime = Random.Next(
                this.MaxReconnectTimeoutMilliseconds / 2,
                this.MaxReconnectTimeoutMilliseconds);

            System.Diagnostics.Debug.WriteLine(
                $"Hub '{this.endpoint}': reconnecting in {reconnectionTime / 1000.0:0.0} seconds ... ");

            await Task.Delay(reconnectionTime);
        }

        #endregion
    }
}
