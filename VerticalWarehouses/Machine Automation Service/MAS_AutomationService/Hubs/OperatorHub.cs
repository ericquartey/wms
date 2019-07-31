using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public class OperatorHub : Hub<IOperatorHub>
    {
        #region Fields

        private const string BayIdEntry = "bayId";

        private readonly IBaysProvider baysProvider;

        private readonly ILogger<OperatorHub> logger;

        #endregion

        #region Constructors

        public OperatorHub(
            IBaysProvider baysProvider,
            ILogger<OperatorHub> logger)
        {
            if (baysProvider == null)
            {
                throw new ArgumentNullException(nameof(baysProvider));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.baysProvider = baysProvider;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public override async Task OnConnectedAsync()
        {
            var ipAddress = this.Context.GetHttpContext().Connection.RemoteIpAddress;
            var bay = this.baysProvider.GetByIpAddress(ipAddress);

            if (bay != null)
            {
                this.logger.LogDebug($"Client on bay {bay.Id} connected to signalR hub.");
                if (this.Context.Items.ContainsKey(BayIdEntry))
                {
                    this.Context.Items[BayIdEntry] = bay.Id;
                }
                else
                {
                    this.Context.Items.Add(BayIdEntry, bay.Id);
                }
            }
            else
            {
                this.logger.LogWarning(
                    $"The client with IP Address '{ipAddress}' connected to the signalR hub, "
                    + "but no bay is configured to serve from the given address.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //var bayId = (int)this.Context.Items[BayIdEntry];

            //this.baysProvider.Deactivate(bayId);

            await base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}
