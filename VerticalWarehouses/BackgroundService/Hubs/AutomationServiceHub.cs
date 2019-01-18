using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BackgroundService
{
    public class AutomationServiceHub : Hub<IAutomationServiceHub>
    {
        #region Methods

        public async Task ExecutingNewActionMethod(string message)
        {
            await this.Clients.All.OnExecutingNewAction(message);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        #endregion Methods
    }
}
