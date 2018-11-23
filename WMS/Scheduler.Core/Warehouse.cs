using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.Core
{
    public class Warehouse : IWarehouse
    {
        #region Fields

        private readonly IBayProvider bayProvider;
        private readonly IItemProvider itemProvider;
        private readonly ILogger<Warehouse> logger;
        private readonly IMissionProvider missionProvider;
        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion Fields

        #region Constructors

        public Warehouse(
           IItemProvider itemProvider,
           IBayProvider bayProvider,
           IMissionProvider missionProvider,
           ISchedulerRequestProvider schedulerRequestProvider,
           ILogger<Warehouse> logger)
        {
            this.itemProvider = itemProvider;
            this.missionProvider = missionProvider;
            this.bayProvider = bayProvider;
            this.logger = logger;
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
