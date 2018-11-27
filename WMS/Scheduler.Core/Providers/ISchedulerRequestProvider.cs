using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface ISchedulerRequestProvider
    {
        #region Methods

        Task<SchedulerRequest> FullyQualifyWithdrawalRequest(SchedulerRequest schedulerRequest);

        IQueryable<Compartment> GetCandidateWithdrawalCompartments(SchedulerRequest schedulerRequest);

        IQueryable<T> OrderCompartmentsByManagementType<T>(IQueryable<T> compartments, ItemManagementType type)
            where T : IOrderableCompartment;

        #endregion Methods
    }
}
