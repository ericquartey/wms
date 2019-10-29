using System;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public interface IInstallationHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<MessageNotifiedEventArgs> MessageReceived;

        #endregion
    }
}
