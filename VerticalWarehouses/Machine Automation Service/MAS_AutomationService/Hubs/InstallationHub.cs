using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public class InstallationHub : Hub<IInstallationHub>
    {
        #region Fields

        private static ConcurrentDictionary<string, ConnectedClient> connectedClients = new ConcurrentDictionary<string, ConnectedClient>();

        private readonly ILogger<InstallationHub> logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallationHub"/> class.
        ///  An instance of this class is created every time a client connects or disconnects
        /// </summary>
        public InstallationHub(ILogger<InstallationHub> logger)
        {
            this.logger = logger;
        }

        #endregion

        #region Methods

        public override Task OnConnectedAsync()
        {
            var remoteIP = this.Context.GetHttpContext().Connection.RemoteIpAddress;
            var localIP = this.Context.GetHttpContext().Connection.LocalIpAddress;
            connectedClients.TryAdd(this.Context.ConnectionId, new ConnectedClient(this.Context.ConnectionId));
            this.logger.LogTrace($"Connection OPENED with client on remoteIP: {remoteIP}, localIP: {localIP}, connection ID: {this.Context.ConnectionId}, there are now {connectedClients.Count} connected clients.");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var remoteIP = this.Context.GetHttpContext().Connection.RemoteIpAddress;
            var localIP = this.Context.GetHttpContext().Connection.LocalIpAddress;
            connectedClients.TryRemove(this.Context.ConnectionId, out var disconnectedClient);
            this.logger.LogTrace($"Connection CLOSED with client on remoteIP: {remoteIP}, localIP: {localIP}, connection ID: {this.Context.ConnectionId}, there are now {connectedClients.Count} connected clients.");
            return base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}
