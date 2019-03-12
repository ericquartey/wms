using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Interfaces;
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

        public async Task SendActionUpdateToAllConnectedClients(ActionUpdateData data)
        {
            await this.Clients.All.OnActionUpdateToAllConnectedClients(data);
        }

        public async Task SendMessageToAllConnectedClients(string message)
        {
            await this.Clients.All.OnSendMessageToAllConnectedClients(message);
        }

        public async Task SendSensorsStatesToAllConnectedClients(bool[] sensors)
        {
            await this.Clients.All.OnSensorsChangedToAllConnectedClients(sensors);
        }

        #endregion
    }
}
