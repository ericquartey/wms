namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public interface IOperatorHubClient : IAutoReconnectHubClient
    {
        #region Events

        event System.EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion
    }
}
