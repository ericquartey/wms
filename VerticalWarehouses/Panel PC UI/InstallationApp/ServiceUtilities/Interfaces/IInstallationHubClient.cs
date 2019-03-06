using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces
{
    public interface IInstallationHubClient
    {
        #region Events

        event EventHandler<InstallationHubEventArgs> ReceivedMessageToAllConnectedClients;

        #endregion

        #region Methods

        Task ConnectAsync();

        #endregion
    }
}
