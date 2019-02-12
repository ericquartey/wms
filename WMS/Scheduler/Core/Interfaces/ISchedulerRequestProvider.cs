using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface ISchedulerRequestProvider
    {
        #region Methods

        Task CreateRangeAsync(IEnumerable<SchedulerRequest> requests);

        Task<SchedulerRequest> FullyQualifyWithdrawalRequestAsync(SchedulerRequest schedulerRequest);

        IQueryable<Compartment> GetCandidateWithdrawalCompartments(SchedulerRequest schedulerRequest);

        Task<IEnumerable<SchedulerRequest>> GetRequestsToProcessAsync();

        IQueryable<T> OrderCompartmentsByManagementType<T>(IQueryable<T> compartments, ItemManagementType type)
                    where T : IOrderableCompartment;

        #endregion
    }
}
