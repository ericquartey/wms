using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ISchedulerService
    {
        #region Methods

        Task<IOperationResult<Mission>> CompleteLoadingUnitMissionAsync(int missionId);

        Task<IOperationResult<LoadingUnitSchedulerRequest>> WithdrawLoadingUnitAsync(int loadingUnitId, int loadingUnitTypeId, int bayId);

        #endregion
    }
}
