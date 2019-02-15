using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces
{
    public interface IAutomationHubClient
    {
        #region Events

        // HACK changed installationHubEventArgs with string
        event EventHandler<AutomationHubEventArgs> ReceivedMessageToAllConnectedClients;

        #endregion

        #region Methods

        Task ConnectAsync();

        Task DisconnectAsync();

        #endregion
    }
}
