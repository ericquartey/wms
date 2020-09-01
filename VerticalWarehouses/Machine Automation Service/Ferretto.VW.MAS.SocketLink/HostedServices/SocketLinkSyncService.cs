using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.SocketLink
{
    internal sealed class SocketLinkSyncService : BackgroundService
    {
        #region Fields

        private const int DEFAULT_PORT = 7075;

        private const int PERIODIC_RESPONSE_SECONDS = 120;

        private const int SOCKET_POOL_TIMEOUT_MILLI_SECONDS = 50000;

        private const int SOCKET_TIMEOUT_SECONDS = 600;

        private readonly IDataLayerService dataLayerService;

        private readonly TcpListener listenerSocketLink = new TcpListener(IPAddress.Any, DEFAULT_PORT);

        private readonly ILogger<SocketLinkSyncService> logger;

        private readonly NotificationEvent notificationEvent;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private CancellationTokenSource cancellationTokenSource;

        #endregion

        #region Constructors

        public SocketLinkSyncService(
            IEventAggregator eventAggregator,
            IDataLayerService dataLayerService,
            ILogger<SocketLinkSyncService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.dataLayerService = dataLayerService ?? throw new ArgumentNullException(nameof(dataLayerService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
        }

        #endregion

        #region Methods

        public async override Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            if (this.dataLayerService.IsReady)
            {
                this.OnDataLayerReady(null);
            }
            else
            {
                this.notificationEvent.Subscribe(
                    this.OnDataLayerReady,
                    ThreadOption.PublisherThread,
                    false,
                    m => m.Type is CommonUtils.Messages.Enumerations.MessageType.DataLayerReady);
            }

            this.notificationEvent.Subscribe(
                   this.OnDataLayerReady,
                   ThreadOption.PublisherThread,
                   false,
                   m => m.Type is CommonUtils.Messages.Enumerations.MessageType.WmsEnableChanged);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            this.cancellationTokenSource?.Cancel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        private void Disable()
        {
            this.cancellationTokenSource?.Cancel();
            this.cancellationTokenSource = null;
        }

        private void Enable()
        {
            this.Disable();

            Task.Run(this.ExecutePollingAsync);
        }

        private async Task ExecutePollingAsync()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = this.cancellationTokenSource.Token;

            try
            {
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    this.listenerSocketLink.Start();

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        if (this.listenerSocketLink.Pending())
                        {
                            var client = this.listenerSocketLink.AcceptTcpClient();
                            _ = Task.Run(() => this.ManageClient(client));
                        }
                        else
                        {
                            Thread.Sleep(100); //<--- timeout
                        }
                    }

                    do
                    {
                        await Task.Delay(5000).ConfigureAwait(true);
                        this.logger.LogTrace("SocketLink ExecutePollingAsync");
                    }
                    while (!cancellationToken.IsCancellationRequested);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
            {
                this.logger.LogTrace("SocketLink Stopping Socket");
                return;
            }
        }

        private void ManageClient(TcpClient client)
        {
            var timeout = false;
            var lastActivity = DateTime.Now;
            var periodicActivity = DateTime.MinValue;

            var socket = client.Client;
            var buffer = new byte[1024];

            while (client.Connected && !timeout)
            {
                if (socket != null && socket.Connected)
                {
                    if (socket.Poll(SOCKET_POOL_TIMEOUT_MILLI_SECONDS, SelectMode.SelectRead))
                    {
                        var bytes = socket.Receive(buffer);

                        if (bytes > 0)
                        {
                            var msgReceived = Encoding.ASCII.GetString(buffer, 0, bytes);
                            lastActivity = DateTime.Now;
                            this.logger.LogTrace("SocketLink Recived " + msgReceived);

                            var msgResponse = "";
                            using (var scope = this.serviceScopeFactory.CreateScope())
                            {
                                var socketLinkSyncProvider = scope.ServiceProvider.GetRequiredService<ISocketLinkSyncProvider>();
                                msgResponse = socketLinkSyncProvider.ProcessCommands(msgReceived);
                            }

                            if (!string.IsNullOrEmpty(msgResponse))
                            {
                                var outStream = Encoding.ASCII.GetBytes(msgResponse);
                                socket.Send(outStream);
                                this.logger.LogTrace("SocketLink Send " + msgResponse);
                            }
                        }
                    }
                }
                else
                {
                    break;
                }

                if (this.PeriodicCheckTimeoutIsExpired(lastActivity))
                {
                    timeout = true;
                }
                else
                {
                    this.PeridicAction(socket, ref periodicActivity);
                }
            }
        }

        private void OnDataLayerReady(NotificationMessage message)
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                if (true)
                {
                    this.logger.LogDebug("SocketLink Starting  service");

                    this.Enable();
                }
                else
                {
                    this.logger.LogDebug("SocketLink Stopping sync service");

                    this.Disable();
                }
            }
        }

        private void PeridicAction(Socket socket, ref DateTime periodicActivity)
        {
            if (PERIODIC_RESPONSE_SECONDS > 0)
            {
                if (DateTime.Now > periodicActivity.AddSeconds(PERIODIC_RESPONSE_SECONDS))
                {
                    periodicActivity = DateTime.Now;
                    var msgResponse = "";

                    using (var scope = this.serviceScopeFactory.CreateScope())
                    {
                        var socketLinkSyncProvider = scope.ServiceProvider.GetRequiredService<ISocketLinkSyncProvider>();
                        var periodicResponseHeder = new List<SocketLinkCommand.HeaderType>() { SocketLinkCommand.HeaderType.STATUS_EXT_REQUEST_CMD };
                        msgResponse = socketLinkSyncProvider.PeriodicResponse(periodicResponseHeder);

                        if (!string.IsNullOrEmpty(msgResponse))
                        {
                            var outStream = Encoding.ASCII.GetBytes(msgResponse);
                            socket.Send(outStream);
                            this.logger.LogTrace("SocketLink Periodic Send " + msgResponse);
                        }
                    }
                }
            }
        }

        private bool PeriodicCheckTimeoutIsExpired(DateTime lastActivity)
        {
            var timeout = false;

            if (SOCKET_TIMEOUT_SECONDS > 0)
            {
                if (DateTime.Now > lastActivity.AddSeconds(SOCKET_TIMEOUT_SECONDS))
                {
                    timeout = true;
                    this.logger.LogTrace("SocketLink socket Timeout " + SOCKET_TIMEOUT_SECONDS);
                }
            }

            return timeout;
        }

        #endregion
    }
}
