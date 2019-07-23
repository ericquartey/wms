using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Microsoft.AspNetCore.SignalR;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public class OperatorHub : Hub<IOperatorHub>
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public OperatorHub(IEventAggregator eventAggregator)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public override async Task OnConnectedAsync()
        {
            this.eventAggregator
                .GetEvent<ClientConnectionChangedPubSubEvent>()
                .Publish(new ClientConnectionChangedPayload(
                    this.Context.ConnectionId,
                    this.Context.GetHttpContext().Connection.LocalIpAddress,
                    true));

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            this.eventAggregator
               .GetEvent<ClientConnectionChangedPubSubEvent>()
               .Publish(new ClientConnectionChangedPayload(
                    this.Context.ConnectionId,
                    this.Context.GetHttpContext().Connection.LocalIpAddress,
                    false));

            await base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}
