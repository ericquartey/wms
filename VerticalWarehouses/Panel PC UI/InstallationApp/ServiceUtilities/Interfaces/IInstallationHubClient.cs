using System;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces
{
    public interface IInstallationHubClient
    {
        #region Events

        event EventHandler<ActionUpdatedEventArgs> ActionUpdated;

        event EventHandler<ReceivedMessageEventArgs> ReceivedMessageToAllConnectedClients;

        event EventHandler<SensorsChangedEventArgs> SensorsChanged;

        #endregion

        #region Methods

        Task ConnectAsync();

        #endregion
    }
}
