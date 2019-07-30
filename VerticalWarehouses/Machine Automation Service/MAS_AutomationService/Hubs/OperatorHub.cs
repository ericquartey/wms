using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public class OperatorHub : Hub<IOperatorHub>
    {
        #region Fields

        private const string BayIdEntry = "bayId";

        private readonly IBaysProvider baysProvider;

        #endregion

        #region Constructors

        public OperatorHub(IBaysProvider baysProvider)
        {
            if (baysProvider == null)
            {
                throw new ArgumentNullException(nameof(baysProvider));
            }

            this.baysProvider = baysProvider;
        }

        #endregion

        #region Methods

        public override async Task OnConnectedAsync()
        {
            //var bay = this.baysProvider.GetByIpAddress(this.Context.GetHttpContext().Connection.RemoteIpAddress);

            //if (this.Context.Items.ContainsKey(BayIdEntry))
            //{
            //    this.Context.Items[BayIdEntry] = bay.Id;
            //}
            //else
            //{
            //    this.Context.Items.Add(BayIdEntry, bay.Id);
            //}

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
