using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ISchedulerRequestProvider : IBusinessProvider<SchedulerRequest, SchedulerRequest>
    {
        #region Methods

        Task<SchedulerRequest> FullyQualifyRequest(SchedulerRequest schedulerRequest);

        Task<int> GetAvailableQuantity(SchedulerRequest schedulerRequest);

        #endregion Methods
    }
}
