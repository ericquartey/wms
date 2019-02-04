using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ISchedulerRequestProvider : IBusinessProvider<SchedulerRequest, SchedulerRequest>
    {
        #region Methods

        IQueryable<SchedulerRequest> GetWithOperationTypeInsertion();

        int GetWithOperationTypeInsertionCount();

        IQueryable<SchedulerRequest> GetWithOperationTypeWithdrawal();

        int GetWithOperationTypeWithdrawalCount();

        #endregion
    }
}
