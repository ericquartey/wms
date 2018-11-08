using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Scheduler.WebAPI.Hubs
{
    public class WakeupHub : Hub<IWakeupHub>
    {
        #region Methods

        public async Task WakeUp(string user, string message)
        {
            await this.Clients.All.WakeUp(user, message);
        }

        #endregion Methods
    }
}
