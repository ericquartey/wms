using System;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public interface IOperatorHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<BayStatusChangedEventArgs> BayStatusChanged;

        event EventHandler<MissionOperationStartedEventArgs> MissionOperationStarted;

        #endregion

        #region Methods

        Task RetrieveCurrentMissionOperationAsync();

        #endregion
    }
}
