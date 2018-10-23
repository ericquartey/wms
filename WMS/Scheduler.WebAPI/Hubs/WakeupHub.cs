using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Scheduler.WebAPI.Hubs
{
    public class WakeupHub : Hub
    {
        #region Methods

        public async Task WakeUp(string user, string message)
        {
            await this.Clients.All.SendAsync("WakeUp", user, message);
        }

        #endregion Methods
    }
}
