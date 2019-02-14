using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ISchedulerRequestProvider : IBusinessProvider<SchedulerRequest, SchedulerRequest, int>
    {
        #region Methods

        IQueryable<SchedulerRequest> GetWithOperationTypeInsertion();

        int GetWithOperationTypeInsertionCount();

        IQueryable<SchedulerRequest> GetWithOperationTypeWithdrawal();

        int GetWithOperationTypeWithdrawalCount();

        #endregion
    }
}
