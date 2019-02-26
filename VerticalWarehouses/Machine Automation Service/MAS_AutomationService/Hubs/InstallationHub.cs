using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS_AutomationService.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.VW.MAS_AutomationService.Hubs
{
    public class InstallationHub : Hub<IInstallationHub>
    {
        #region Methods

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task SendMessageToAllConnectedClients(string message)
        {
            await this.Clients.All.OnSendMessageToAllConnectedClients(message);
        }

        #endregion
    }
}
