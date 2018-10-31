using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Scheduler.WebAPI.Hubs
{
    public class WakeupHub : Hub<IWakeupHub>
    {
        #region Fields

        private readonly List<string> connectionIdsPerWarehouseArea = new List<string>();

        #endregion Fields

        #region Methods

        public async Task Notify()
        {
            await this.Clients.All.WakeUp("server", "notification from client received");
        }

        public override Task OnConnectedAsync()
        {
            this.connectionIdsPerWarehouseArea.Add(this.Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public async Task WakeUpAll(string user, string message)
        {
            await this.Clients.All.WakeUp(user, message);
        }

        #endregion Methods
    }
}
