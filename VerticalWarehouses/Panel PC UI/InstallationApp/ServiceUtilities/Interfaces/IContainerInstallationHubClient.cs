using System;

namespace Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces
{
    public interface IContainerInstallationHubClient
    {
        #region Events

        event EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion
    }
}
