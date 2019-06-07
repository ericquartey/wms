using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MachineAutomationService.Hubs;
using Ferretto.VW.MAS_AutomationService.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Utilities.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Hubs
{
    public class OperatorHub : Hub<IOperatorHub>
    {
        #region Fields

        private static int baysCounter = 0;

        private readonly IBaysManager baysManager;

        private readonly ILogger<OperatorHub> logger;

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorHub"/> class.
        ///  An instance of this class is created every time a client connects or disconnects
        /// </summary>
        public OperatorHub(ILogger<OperatorHub> logger, IEventAggregator eventAggregator, IBaysManager baysManager)
        {
            this.logger = logger;
            this.eventAggregator = eventAggregator;
            this.baysManager = baysManager;
        }

        #endregion

        #region Methods

        public override Task OnConnectedAsync()
        {
            var remoteIP = this.Context.GetHttpContext().Connection.RemoteIpAddress;
            var localIP = this.Context.GetHttpContext().Connection.LocalIpAddress;

            var messageData = new NewConnectedClientMessageData { localIPAddress = localIP.ToString() };

            var notificationMessage = new NotificationMessage(messageData, "New client connected", MessageActor.MissionsManager, MessageActor.WebApi, MessageType.NewClientConnected, MessageStatus.NoStatus);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var remoteIP = this.Context.GetHttpContext().Connection.RemoteIpAddress;
            var localIP = this.Context.GetHttpContext().Connection.LocalIpAddress;

            return base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}
