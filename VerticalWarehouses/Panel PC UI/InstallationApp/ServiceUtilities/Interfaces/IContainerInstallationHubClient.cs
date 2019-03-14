using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Interfaces;

namespace Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces
{
    public interface IContainerInstallationHubClient
    {
        #region Events

        event EventHandler<ActionUpdateData> ActionUpdated;

        event EventHandler<string> ReceivedMessage;

        event EventHandler<bool[]> SensorsChanged;

        #endregion
    }
}
