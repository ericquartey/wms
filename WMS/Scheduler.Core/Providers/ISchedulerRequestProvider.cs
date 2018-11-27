using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface ISchedulerRequestProvider : IBusinessProvider<SchedulerRequest, SchedulerRequest>
    {
        #region Methods

        Task<SchedulerRequest> FullyQualifyWithdrawalRequest(SchedulerRequest schedulerRequest);

        IQueryable<Compartment> GetCandidateWithdrawalCompartments(SchedulerRequest schedulerRequest);

        Task<SchedulerRequest> GetNextRequest();

        IQueryable<T> OrderCompartmentsByManagementType<T>(IQueryable<T> compartments, ItemManagementType type)
            where T : IOrderableCompartment;

        #endregion Methods
    }
}
