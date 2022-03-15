using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.Common.Hubs
{
    public abstract class AutoReconnectHubClient : IAutoReconnectHubClient
    {
        #region Fields

        private const int DefaultMaxReconnectTimeout = 5 * 1000;

        private const int DefaultRetriesBeforeThrottle = 10;

        private const int DefaultThrottledReconnectTimeout = 60 * 1000;

        private static readonly Random Random = new Random();

        private readonly Uri endpoint;

        private HubConnection connection;

        private int failedRetries;

        private WebProxy webProxy;

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

        protected AutoReconnectHubClient(Uri uri, WebProxy webProxy)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (webProxy == null)
            {
                throw new ArgumentNullException(nameof(webProxy));
            }

            this.endpoint = uri;
            this.webProxy = webProxy;
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

        public async Task ConnectAsync(bool useMessagePackProtocol)
        {
            this.failedRetries = 0;

            while (this.connection?.State != HubConnectionState.Connected)
            {
                try
                {
                    this.Initialize(useMessagePackProtocol);

                    System.Diagnostics.Debug.WriteLine($"Hub '{this.endpoint}': establishing connection ... ");

                    await this.connection.StartAsync();

                    System.Diagnostics.Debug.WriteLine($"Hub '{this.endpoint}': connection established.");

                    this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(true));

                    await this.OnConnectedAsync();
                }
                catch (Exception ex)
                {
                    this.failedRetries++;
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

        public async Task SendAsync(string methodName, object arg1)
        {
            await this.connection.SendAsync(methodName, arg1);
        }

        public async Task SendAsync(string methodName, object arg1, object arg2)
        {
            await this.connection.SendAsync(methodName, arg1, arg2);
        }

        public async Task SendAsync(string methodName, object arg1, object arg2, object arg3)
        {
            await this.connection.SendAsync(methodName, arg1, arg2, arg3);
        }

        public async Task SendAsync(string methodName, object arg1, object arg2, object arg3, object arg4)
        {
            await this.connection.SendAsync(methodName, arg1, arg2, arg3, arg4);
        }

        public async Task SetProxy(WebProxy proxy)
        {
            this.webProxy = proxy;
            if (this.IsConnected)
            {
                await this.DisconnectAsync();
            }
        }

        protected virtual Task OnConnectedAsync()
        {
            // do nothing
            return Task.CompletedTask;
        }

        protected abstract void RegisterEvents(HubConnection connection);

        private void Initialize(bool useMessagePackProtocol)
        {
            if (this.connection != null)
            {
                return;
            }

            var connectionBuilder = new HubConnectionBuilder();

            if (this.webProxy == null || this.webProxy.Address == null)
            {
                connectionBuilder.WithUrl(this.endpoint.AbsoluteUri);
            }
            else
            {
                connectionBuilder.WithUrl(this.endpoint.AbsoluteUri, h => h.Proxy = this.webProxy);
            }

            if (useMessagePackProtocol)
            {
                connectionBuilder.AddMessagePackProtocol();
            }

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

                await this.ConnectAsync(useMessagePackProtocol);
            };
        }

        private async Task WaitForReconnectionAsync()
        {
            var isReconnectionThrottled = this.failedRetries > DefaultRetriesBeforeThrottle;
            if (isReconnectionThrottled)
            {
                System.Diagnostics.Debug.WriteLine(
                  $"Hub '{this.endpoint}': reconnection is throttled.");
            }

            var reconnectionTime = isReconnectionThrottled
                ? DefaultThrottledReconnectTimeout
                : Random.Next(
                    this.MaxReconnectTimeoutMilliseconds / 2,
                    this.MaxReconnectTimeoutMilliseconds);

            System.Diagnostics.Debug.WriteLine(
                $"Hub '{this.endpoint}': reconnecting in {reconnectionTime / 1000.0:0.0} seconds (retry #{this.failedRetries}) ... ");

            await Task.Delay(reconnectionTime);
        }

        #endregion
    }
}
