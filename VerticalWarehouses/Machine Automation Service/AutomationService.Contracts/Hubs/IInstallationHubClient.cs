using System;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public interface IInstallationHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion
    }
}
