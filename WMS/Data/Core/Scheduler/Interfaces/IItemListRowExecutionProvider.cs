using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListRowExecutionProvider :
        IReadSingleAsyncProvider<ItemListRowOperation, int>,
        IUpdateAsyncProvider<ItemListRowOperation, int>
    {
        #region Methods

        Task<IOperationResult<ItemListRowSchedulerRequest>> PrepareForExecutionAsync(int id, int areaId, int? bayId);

        Task<IOperationResult<ItemListRowSchedulerRequest>> PrepareForExecutionInListAsync(ItemListRowOperation row, int areaId, int? bayId, int? previousRowRequestPriority);

        Task<IOperationResult<ItemListRowOperation>> SuspendAsync(int id);

        #endregion
    }
}
