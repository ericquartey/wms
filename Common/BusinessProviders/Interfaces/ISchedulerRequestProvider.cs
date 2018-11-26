using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ISchedulerRequestProvider : IBusinessProvider<SchedulerRequest, SchedulerRequest>
    {
        #region Methods

        Task<SchedulerRequest> FullyQualifyWithdrawalRequest(SchedulerRequest schedulerRequest);

        IQueryable<CompartmentCore> GetCandidateWithdrawalCompartments(SchedulerRequest schedulerRequest);

        Task<SchedulerRequest> GetNextRequest();

        IQueryable<IOrderableCompartment> OrderCompartmentsByManagementType(IQueryable<IOrderableCompartment> compartments, ItemManagementType type);

        #endregion Methods
    }
}
