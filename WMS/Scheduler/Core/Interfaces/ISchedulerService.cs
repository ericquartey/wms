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

        Task<IOperationResult<IEnumerable<SchedulerRequest>>> ExecuteListAsync(int listId, int areaId, int? bayId);

        Task<IOperationResult<SchedulerRequest>> ExecuteListRowAsync(int rowId, int areaId, int? bayId);

        Task<IOperationResult<Mission>> ExecuteMissionAsync(int missionId);

        Task<IOperationResult<ItemList>> SuspendListAsync(int id);

        Task<IOperationResult<ItemListRow>> SuspendListRowAsync(int id);

        Task<IOperationResult<SchedulerRequest>> WithdrawItemAsync(int itemId, ItemWithdrawOptions options);

        #endregion
    }
}
