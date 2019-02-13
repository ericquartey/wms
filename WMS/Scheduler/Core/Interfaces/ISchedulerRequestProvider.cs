using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface ISchedulerRequestProvider
    {
        #region Methods

        Task<SchedulerRequest> CreateAsync(SchedulerRequest model);

        Task<IEnumerable<SchedulerRequest>> CreateRangeAsync(IEnumerable<SchedulerRequest> models);

        Task<SchedulerRequest> FullyQualifyWithdrawalRequestAsync(SchedulerRequest schedulerRequest);

        IQueryable<Compartment> GetCandidateWithdrawalCompartments(SchedulerRequest schedulerRequest);

        Task<IEnumerable<SchedulerRequest>> GetRequestsToProcessAsync();

        IQueryable<T> OrderCompartmentsByManagementType<T>(IQueryable<T> compartments, ItemManagementType type)
                    where T : IOrderableCompartment;

        Task<SchedulerRequest> UpdateAsync(SchedulerRequest request);

        Task<SchedulerRequest> WithdrawAsync(SchedulerRequest request);

        #endregion
    }
}
