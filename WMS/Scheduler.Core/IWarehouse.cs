using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface IWarehouse
    {
        #region Methods

        Task<IEnumerable<SchedulerRequest>> PrepareListForExecutionAsync(int listId, int areaId, int? bayId);

        Task<SchedulerRequest> WithdrawAsync(SchedulerRequest request);

        #endregion Methods
    }
}
