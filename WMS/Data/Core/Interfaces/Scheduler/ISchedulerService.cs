using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ISchedulerService
    {
        #region Methods

        Task<IOperationResult<MissionExecution>> CompleteItemMissionAsync(int missionId, double quantity);

        Task<IOperationResult<MissionExecution>> CompleteLoadingUnitMissionAsync(int missionId);

        Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> ExecuteListAsync(int listId, int areaId, int? bayId);

        Task<IOperationResult<ItemListRowSchedulerRequest>> ExecuteListRowAsync(int rowId, int areaId, int? bayId);

        Task<IOperationResult<MissionExecution>> ExecuteMissionAsync(int missionId);

        Task<IOperationResult<ItemList>> SuspendListAsync(int id);

        Task<IOperationResult<ItemListRow>> SuspendListRowAsync(int id);

        Task<IOperationResult<ItemSchedulerRequest>> WithdrawItemAsync(int itemId, ItemWithdrawOptions options);

        Task<IOperationResult<LoadingUnitSchedulerRequest>> WithdrawLoadingUnitAsync(int loadingUnitId, int loadingUnitTypeId, int bayId);

        #endregion
    }
}
