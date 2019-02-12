using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.Core
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Blocker Code Smell",
        "S4462:Calls to \"async\" methods should not be blocking",
        Justification = "Blocking task execution inside Select lambda will be refactored within data service")]
    public class Warehouse : IWarehouse
    {
        #region Fields

        private readonly IDataProvider dataProvider;

        private readonly ILogger<Warehouse> logger;

        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion

        #region Constructors

        public Warehouse(
           IDataProvider dataProvider,
           ISchedulerRequestProvider schedulerRequestProvider,
           ILogger<Warehouse> logger)
        {
            this.dataProvider = dataProvider;
            this.logger = logger;
            this.schedulerRequestProvider = schedulerRequestProvider;
        }

        #endregion

        #region Methods

        public async Task<SchedulerRequest> WithdrawAsync(SchedulerRequest request)
        {
            SchedulerRequest qualifiedRequest = null;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                qualifiedRequest = await this.schedulerRequestProvider.FullyQualifyWithdrawalRequestAsync(request);
                if (qualifiedRequest != null)
                {
                    this.dataProvider.Add(qualifiedRequest);

                    scope.Complete();
                    this.logger.LogDebug($"Scheduler Request (id={qualifiedRequest.Id}): Withdrawal for item={qualifiedRequest.ItemId} was accepted and stored.");
                }
            }

            await this.CreateMissionsForPendingRequestsAsync();

            return qualifiedRequest;
        }

        private async Task<Bay> GetNextEmptyBayAsync(int areaId, int? bayId)
        {
            if (bayId.HasValue)
            {
                var bay = await this.dataProvider.GetBayByIdAsync(bayId.Value);

                if (bay.LoadingUnitsBufferSize > bay.LoadingUnitsBufferUsage)
                {
                    return bay;
                }
            }
            else
            {
                var area = await this.dataProvider.GetAreaByIdAsync(areaId);

                return area.Bays
                    .Where(b => b.LoadingUnitsBufferSize > b.LoadingUnitsBufferUsage)
                    .OrderBy(b => b.LoadingUnitsBufferUsage)
                    .FirstOrDefault();
            }

            return null;
        }

        #endregion
    }
}
