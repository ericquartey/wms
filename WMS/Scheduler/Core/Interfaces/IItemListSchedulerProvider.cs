using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemListSchedulerProvider
    {
        #region Methods

        Task<IEnumerable<SchedulerRequest>> PrepareForExecutionAsync(ListExecutionRequest request);

        Task<IOperationResult<ItemList>> UpdateAsync(ItemList model);

        #endregion
    }
}
