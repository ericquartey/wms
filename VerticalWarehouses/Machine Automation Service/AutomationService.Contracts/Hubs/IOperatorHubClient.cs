using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public interface IOperatorHubClient : IAutoReconnectHubClient
    {
        #region Events

        event System.EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion
    }
}
