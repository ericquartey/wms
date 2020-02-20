using System;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public interface IOperatorHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<AssignedMissionChangedEventArgs> AssignedMissionChanged;

        event EventHandler<AssignedMissionOperationChangedEventArgs> AssignedMissionOperationChanged;

        event EventHandler<BayStatusChangedEventArgs> BayStatusChanged;

        event EventHandler<ErrorStatusChangedEventArgs> ErrorStatusChanged;

        #endregion
    }
}
