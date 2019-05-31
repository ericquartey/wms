using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.MAS_AutomationService.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_AutomationService.Hubs
{
    public class OperatorHub : Hub<IOperatorHub>
    {
        #region Fields

        private static ConcurrentDictionary<string, ConnectedClient> connectedClients = new ConcurrentDictionary<string, ConnectedClient>();

        private readonly ILogger<OperatorHub> logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorHub"/> class.
        ///  An instance of this class is created every time a client connects or disconnects
        /// </summary>
        public OperatorHub(ILogger<OperatorHub> logger)
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
