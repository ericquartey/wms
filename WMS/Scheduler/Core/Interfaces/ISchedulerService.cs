using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ISchedulerService
    {
        #region Methods

        Task<IEnumerable<SchedulerRequest>> ExecuteListAsync(ListExecutionRequest request);

        Task ProcessPendingRequestsAsync();

        Task<SchedulerRequest> WithdrawItemAsync(SchedulerRequest request);

        #endregion
    }
}
