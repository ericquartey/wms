using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ISchedulerRequestProvider
    {
        #region Methods

        Task<SchedulerRequest> CreateAsync(SchedulerRequest model);

        Task<IEnumerable<SchedulerRequest>> CreateRangeAsync(IEnumerable<SchedulerRequest> models);

        Task<SchedulerRequest> FullyQualifyWithdrawalRequestAsync(SchedulerRequest schedulerRequest);

        Task<IEnumerable<SchedulerRequest>> GetRequestsToProcessAsync();

        Task<SchedulerRequest> UpdateAsync(SchedulerRequest request);

        Task<SchedulerRequest> WithdrawAsync(SchedulerRequest request);

        #endregion
    }
}
