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
using Ferretto.VW.MAS_DataLayer.Interfaces;
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

        private IHubContext<OperatorHub, IOperatorHub> operatorHub;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorHub"/> class.
        ///  An instance of this class is created every time a client connects or disconnects
        /// </summary>
        public OperatorHub(ILogger<OperatorHub> logger, IEventAggregator eventAggregator, IBaysManager baysManager, IHubContext<OperatorHub, IOperatorHub> operatorHub)
        {
            this.logger = logger;
            this.eventAggregator = eventAggregator;
            this.baysManager = baysManager;
            this.operatorHub = operatorHub;
        }

        #endregion

        #region Methods

        public override Task OnConnectedAsync()
        {
            var remoteIP = this.Context.GetHttpContext().Connection.RemoteIpAddress;
            var localIP = this.Context.GetHttpContext().Connection.LocalIpAddress;

            if (this.baysManager.Bays != null && this.baysManager.Bays.Count > 0)
            {
                for (int i = 0; i < this.baysManager.Bays.Count; i++)
                {
                    if (this.baysManager.Bays[i].IpAddress == localIP.ToString())
                    {
                        this.baysManager.Bays[i].ConnectionId = this.Context.ConnectionId;
                        this.baysManager.Bays[i].IsConnected = true;
                        this.baysManager.Bays[i].Status = MAS_Utils.Enumerations.BayStatus.Available;
                        this.baysManager.Bays[i].Id = 2;

                        var messageData = new BayConnectedMessageData
                        {
                            Id = this.baysManager.Bays[i].Id,
                            BayType = (int)this.baysManager.Bays[i].Type,
                            MissionQuantity = this.baysManager.Bays[i].Missions == null ? 0 : this.baysManager.Bays[i].Missions.Count
                        };
                        var notificationMessage = new NotificationMessage(messageData, "Bay Connected", MessageActor.AutomationService, MessageActor.WebApi, MessageType.BayConnected, MessageStatus.NoStatus);
                        this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                    }
                }
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var remoteIP = this.Context.GetHttpContext().Connection.RemoteIpAddress;
            var localIP = this.Context.GetHttpContext().Connection.LocalIpAddress;

            if (this.baysManager.Bays != null && this.baysManager.Bays.Count > 0)
            {
                for (int i = 0; i < this.baysManager.Bays.Count; i++)
                {
                    if (this.baysManager.Bays[i].IpAddress == localIP.ToString())
                    {
                        this.baysManager.Bays[i].ConnectionId = string.Empty;
                        this.baysManager.Bays[i].IsConnected = false;
                        this.baysManager.Bays[i].Status = MAS_Utils.Enumerations.BayStatus.Unavailable;
                    }
                }
            }

            return base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}
