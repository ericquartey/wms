using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Utilities.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public class OperatorHub : Hub<IOperatorHub>
    {
        #region Fields

        private readonly IBaysManager baysManager;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<AutomationService> logger;

        private readonly IHubContext<OperatorHub, IOperatorHub> operatorHub;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorHub"/> class.
        /// </summary>
        public OperatorHub(
            ILogger<AutomationService> logger,
            IEventAggregator eventAggregator,
            IBaysManager baysManager,
            IHubContext<OperatorHub,
                IOperatorHub> operatorHub)
        {
            this.logger = logger;
            this.eventAggregator = eventAggregator;
            this.baysManager = baysManager;
            this.operatorHub = operatorHub;
        }

        #endregion

        #region Methods

        public override async Task OnConnectedAsync()
        {
            var localIpAddress = this.Context.GetHttpContext().Connection.LocalIpAddress;

            var bay = this.baysManager.Bays.SingleOrDefault(b => b.IpAddress == localIpAddress.ToString());
            if (bay == null)
            {
                return;
            }

            bay.ConnectionId = this.Context.ConnectionId;
            bay.Status = BayStatus.Idle;
            bay.Id = 2; // TODO get actual bay ID from WMS

            var messageData = new BayConnectedMessageData
            {
                BayId = bay.Id,
                BayType = (int)bay.Type,
                PendingMissionsCount = bay.PendingMissions.Count()
            };

            var notificationMessage = new NotificationMessage(messageData, "Bay Connected", MessageActor.Any, MessageActor.WebApi, MessageType.BayConnected, MessageStatus.NoStatus);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
            this.logger.LogDebug($"AS-OH Bay connected id: {bay.Id}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var localIpAddress = this.Context.GetHttpContext().Connection.LocalIpAddress;

            var bay = this.baysManager.Bays.SingleOrDefault(b => b.IpAddress == localIpAddress.ToString());
            if (bay != null)
            {
                bay.ConnectionId = null;
                bay.Status = BayStatus.Unavailable;
            }

            await base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}
