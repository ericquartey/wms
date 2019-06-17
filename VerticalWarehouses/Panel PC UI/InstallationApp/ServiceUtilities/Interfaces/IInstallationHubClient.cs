using System;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces
{
    public interface IInstallationHubClient
    {
        #region Events

        event EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion

        #region Methods

        Task ConnectAsync();

        #endregion
    }
}
