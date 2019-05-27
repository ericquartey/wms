using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionCreationProvider
    {
        #region Methods

        Task<IEnumerable<MissionExecution>> CreatePickMissionsAsync(ItemSchedulerRequest request);

        Task<IEnumerable<MissionExecution>> CreatePutMissionsAsync(ItemSchedulerRequest request);

        Task<IEnumerable<MissionExecution>> CreateForRequestsAsync(IEnumerable<ISchedulerRequest> requests);

        Task<MissionExecution> CreateWithdrawalMissionAsync(LoadingUnitSchedulerRequest request);

        #endregion
    }
}
