using System;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages;

namespace Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces
{
    public interface IContainerInstallationHubClient
    {
        #region Events

        event EventHandler<ActionUpdateData> ActionUpdated;

        event EventHandler<string> ReceivedMessage;

        event EventHandler<bool[]> SensorsChanged;

        event EventHandler<bool> ShutterControlEnd;

        #endregion
    }
}
