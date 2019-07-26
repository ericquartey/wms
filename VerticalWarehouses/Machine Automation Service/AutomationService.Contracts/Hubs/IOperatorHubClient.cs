using System;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public interface IOperatorHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<BayStatusChangedEventArgs> BayStatusChanged;

        event EventHandler<MissionOperationAvailableEventArgs> MissionOperationAvailable;

        #endregion
    }
}
