using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemListSchedulerProvider
    {
        #region Methods

        Task<IEnumerable<SchedulerRequest>> PrepareForExecutionAsync(ListExecutionRequest request);

        #endregion
    }
}
