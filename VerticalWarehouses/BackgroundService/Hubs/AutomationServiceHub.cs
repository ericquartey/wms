using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Debug.Print("Automation hub client connected\n");
            return base.OnConnectedAsync();
        }

        [HubMethodName("testing")]
        public void TestMethodCalledFromClient(string message)
        {
            Console.WriteLine("TestMethodCalledFromClient executed. String received: {0}\n", message);
        }

        #endregion Methods
    }
}
