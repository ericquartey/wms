using System;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces
{
    internal interface IInstallationHubClient
    {
        #region Events

        // HACK changed installationHubEventArgs with string
        event EventHandler<string> ReceivedMessageToAllConnectedClients;

        #endregion

        #region Methods

        Task ConnectAsync();

        Task DisconnectAsync();

        #endregion
    }
}
