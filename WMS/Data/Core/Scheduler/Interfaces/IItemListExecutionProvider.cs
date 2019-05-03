using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListExecutionProvider :
        IUpdateAsyncProvider<ItemListExecution, int>,
        IReadSingleAsyncProvider<ItemListExecution, int>
    {
        #region Methods

        Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> PrepareForExecutionAsync(int id, int areaId, int? bayId);

        Task<IOperationResult<ItemListExecution>> SuspendAsync(int id);

        #endregion
    }
}
