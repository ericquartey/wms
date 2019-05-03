using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListRowExecutionProvider :
        IReadSingleAsyncProvider<ItemListRowExecution, int>,
        IUpdateAsyncProvider<ItemListRowExecution, int>
    {
        #region Methods

        Task<IOperationResult<ItemListRowSchedulerRequest>> PrepareForExecutionAsync(int id, int areaId, int? bayId);

        Task<IOperationResult<ItemListRowSchedulerRequest>> PrepareForExecutionInListAsync(ItemListRowExecution row, int areaId, int? bayId, int? previousRowRequestPriority);

        Task<IOperationResult<ItemListRowExecution>> SuspendAsync(int id);

        #endregion
    }
}
