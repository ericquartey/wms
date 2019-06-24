using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ISchedulerService
    {
        #region Methods

        Task<IOperationResult<MissionOperation>> AbortMissionOperationAsync(int operationId);

        Task<IOperationResult<MissionOperation>> CompleteItemOperationAsync(int operationId, double quantity);

        Task<IOperationResult<Mission>> CompleteLoadingUnitMissionAsync(int missionId);

        Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> ExecuteListAsync(int listId, int areaId, int? bayId);

        Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> ExecuteListRowAsync(int rowId, int areaId, int? bayId);

        Task<IOperationResult<MissionOperation>> ExecuteMissionOperationAsync(int operationId);

        Task<IOperationResult<double>> GetPickAvailabilityAsync(int itemId, ItemOptions options);

        Task<IOperationResult<double>> GetPutCapacityAsync(int itemId, ItemOptions options);

        Task<IOperationResult<IEnumerable<ItemSchedulerRequest>>> PickItemAsync(int itemId, ItemOptions options);

        Task<IOperationResult<IEnumerable<ItemSchedulerRequest>>> PutItemAsync(int itemId, ItemOptions options);

        Task<IOperationResult<ItemList>> SuspendListAsync(int listId);

        Task<IOperationResult<ItemListRow>> SuspendListRowAsync(int listId);

        Task<IOperationResult<LoadingUnitSchedulerRequest>> WithdrawLoadingUnitAsync(int loadingUnitId, int loadingUnitTypeId, int bayId);

        #endregion
    }
}
