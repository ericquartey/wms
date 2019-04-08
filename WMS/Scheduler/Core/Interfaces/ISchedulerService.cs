using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ISchedulerService
    {
        #region Methods

        Task<IOperationResult<Mission>> CompleteMissionAsync(int missionId, int quantity);

        Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> ExecuteListAsync(int listId, int areaId, int? bayId);

        Task<IOperationResult<ItemListRowSchedulerRequest>> ExecuteListRowAsync(int rowId, int areaId, int? bayId);

        Task<IOperationResult<Mission>> ExecuteMissionAsync(int missionId);

        Task<IOperationResult<ItemList>> SuspendListAsync(int id);

        Task<IOperationResult<ItemListRow>> SuspendListRowAsync(int id);

        Task<IOperationResult<ItemSchedulerRequest>> WithdrawItemAsync(int itemId, ItemWithdrawOptions options);

        Task<IOperationResult<LoadingUnitSchedulerRequest>> WithdrawLoadingUnitAsync(int loadingUnitId, int loadingUnitTypeId);

        #endregion
    }
}
