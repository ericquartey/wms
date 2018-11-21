using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;

namespace Ferretto.WMS.Scheduler.Core
{
    public class Warehouse : IWarehouse
    {
        #region Fields

        private readonly IItemProvider itemProvider;
        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion Fields

        #region Constructors

        public Warehouse(
           IItemProvider itemProvider,
           ISchedulerRequestProvider schedulerRequestProvider
           )
        {
            this.itemProvider = itemProvider;
            this.schedulerRequestProvider = schedulerRequestProvider;
        }

        #endregion Constructors

        #region Methods

        public async Task<SchedulerRequest> Withdraw(SchedulerRequest schedulerRequest)
        {
            var qualifiedSchedulerRequest = await this.schedulerRequestProvider.FullyQualifyWithdrawalRequest(schedulerRequest);
            if (qualifiedSchedulerRequest != null)
            {
                var addedRecordCount = await this.schedulerRequestProvider.Add(qualifiedSchedulerRequest);
                if (addedRecordCount > 0)
                {
                    return schedulerRequest;
                }
            }

            return null;
        }

        #endregion Methods
    }
}
