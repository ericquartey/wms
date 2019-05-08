using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListExecutionProvider :
        IUpdateAsyncProvider<ItemListOperation, int>,
        IReadSingleAsyncProvider<ItemListOperation, int>
    {
        #region Methods

        Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> PrepareForExecutionAsync(int id, int areaId, int? bayId);

        Task<IOperationResult<ItemListOperation>> SuspendAsync(int id);

        #endregion
    }
}
