using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ISchedulerService
    {
        #region Methods

        Task<IOperationResult<Mission>> CompleteMissionAsync(int id);

        Task<IEnumerable<SchedulerRequest>> ExecuteListAsync(ListExecutionRequest request);

        Task<IOperationResult<SchedulerRequest>> ExecuteListRowAsync(ListRowExecutionRequest request);

        Task<IOperationResult<Mission>> ExecuteMissionAsync(int id);

        Task ProcessPendingRequestsAsync();

        Task<SchedulerRequest> WithdrawItemAsync(SchedulerRequest request);

        #endregion
    }
}
