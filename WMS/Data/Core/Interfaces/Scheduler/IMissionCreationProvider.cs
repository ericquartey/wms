using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionCreationProvider
    {
        #region Methods

        Task<MissionExecution> CreateWithdrawalMissionAsync(LoadingUnitSchedulerRequest request);

        Task<IEnumerable<MissionExecution>> CreatePickMissionsAsync(ItemSchedulerRequest request);

        #endregion
    }
}
