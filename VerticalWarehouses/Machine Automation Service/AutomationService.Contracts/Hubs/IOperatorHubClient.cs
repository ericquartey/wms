using System;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public interface IOperatorHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<BayStatusChangedEventArgs> BayStatusChanged;

        event EventHandler<MissionOperationStartedEventArgs> MissionOperationStarted;

        #endregion
    }
}
