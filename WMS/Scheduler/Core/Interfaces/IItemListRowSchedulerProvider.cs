using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemListRowSchedulerProvider :
        IReadSingleAsyncProvider<ItemListRow, int>,
        IUpdateAsyncProvider<ItemListRow, int>
    {
        #region Methods

        Task<IOperationResult<SchedulerRequest>> PrepareForExecutionAsync(int id, int areaId, int? bayId);

        Task<IOperationResult<SchedulerRequest>> PrepareForExecutionAsync(ItemListRow row, int areaId, int? bayId);

        Task<IOperationResult<ItemListRow>> SuspendAsync(int id);

        Task UpdateRowStatusAsync(ItemListRow row, System.DateTime now);

        #endregion
    }
}
