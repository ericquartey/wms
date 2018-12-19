using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Scheduler.WebAPI.Hubs
{
    public class HealthHub : Hub<IHealthHub>
    {
        #region Methods

        public Task IsOnline()
        {
            return this.Clients.Caller.IsOnline();
        }

        #endregion Methods
    }
}
