using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Scheduler.WebAPI.Hubs
{
    public class WakeupHub : Hub
    {
        #region Methods

        public async Task SendMessage(string user, string message)
        {
            await this.Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        #endregion Methods
    }
}
