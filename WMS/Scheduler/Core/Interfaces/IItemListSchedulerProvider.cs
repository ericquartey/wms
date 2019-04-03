using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemListSchedulerProvider :
        IUpdateAsyncProvider<ItemList, int>,
        IReadSingleAsyncProvider<ItemList, int>
    {
        #region Methods

        Task<IOperationResult<IEnumerable<SchedulerRequest>>> PrepareForExecutionAsync(int id, int areaId, int? bayId);

        Task<IOperationResult<ItemList>> SuspendAsync(int id);

        #endregion
    }
}
