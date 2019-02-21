using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Data.WebAPI.Hubs
{
    public class HealthHub : Hub<IHealthHub>
    {
        #region Methods

        public Task IsOnlineAsync()
        {
            return this.Clients.Caller.IsOnlineAsync();
        }

        #endregion
    }
}
