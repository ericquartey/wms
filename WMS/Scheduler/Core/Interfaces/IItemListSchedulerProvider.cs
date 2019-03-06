using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemListSchedulerProvider :
        IUpdateAsyncProvider<ItemList, int>
    {
        #region Methods

        Task<IEnumerable<SchedulerRequest>> PrepareForExecutionAsync(ListExecutionRequest request);

        #endregion
    }
}
