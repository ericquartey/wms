using System;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public interface IInstallationHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion
    }
}
