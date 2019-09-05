using System;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public interface IOperatorHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<BayStatusChangedEventArgs> BayStatusChanged;

        event EventHandler<ErrorStatusChangedEventArgs> ErrorStatusChanged;

        event EventHandler<MissionOperationAvailableEventArgs> MissionOperationAvailable;

        #endregion
    }
}
