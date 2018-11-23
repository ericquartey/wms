using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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

        public async Task<SchedulerRequest> Withdraw(SchedulerRequest request)
        {
            SchedulerRequest qualifiedRequest = null;
            using (var scope = new TransactionScope())
            {
                qualifiedRequest = await this.schedulerRequestProvider.FullyQualifyWithdrawalRequest(request);
                if (qualifiedRequest != null)
                {
                    var addedRecordCount = await this.schedulerRequestProvider.Add(qualifiedRequest);
                    if (addedRecordCount > 0)
                    {
                        scope.Complete();
                        this.logger.LogDebug($"Withdrawal request for item={request.ItemId} was accepted and stored.");

                        await this.DispatchRequests();
                    }
                }
            }

            return qualifiedRequest;
        }

        private async Task DispatchRequests()
        {
            var request = await this.schedulerRequestProvider.GetNextRequest();
            if (request == null)
            {
                return;
            }

            this.logger.LogDebug($"Request for item={request.ItemId} is the next in line to be processed.");
            switch (request.Type)
            {
                case OperationType.Withdrawal:
                    this.DispatchWithdrawalRequest(request);
                    break;

                case OperationType.Insertion:
                    throw new NotImplementedException();

                case OperationType.Replacement:
                    throw new NotImplementedException();

                case OperationType.Reorder:
                    throw new NotImplementedException();

                default:
                    throw new InvalidOperationException($"Cannot process scheduler request id={request.Id} because operation type cannot be understood.");
            }
        }

        private void DispatchWithdrawalRequest(SchedulerRequest request)
        {
            if (!request.IsInstant)
            {
                throw new NotImplementedException(); //TODO extend this method to support normal (non-instant) withdrawal requests
            }

            var compartments = this.schedulerRequestProvider.GetCandidateWithdrawalCompartments(request);

            var item = this.itemProvider.GetById(request.ItemId);

            var orderedCompartments =
                this.schedulerRequestProvider.OrderCompartmentsByManagementType(
                compartments,
                (ItemManagementType)item.ManagementType);

            var neededCompartmentsCount = orderedCompartments.Aggregate(0,
                (total, compartment) =>
                    total >= request.RequestedQuantity ? total : total + compartment.Availability
                    );

            this.logger.LogDebug($"A total of {neededCompartmentsCount} is needed to complete the request for item id={request.ItemId}");

            var missions = orderedCompartments
                .Cast<CompartmentCore>()
                .Take(neededCompartmentsCount)
                .Select(c => new Mission
                {
                    // TODO add field LoadingUnitsBufferUsage
                    BayId = c.Bays.OrderByDescending(b => b.LoadingUnitsBufferSize/*b.LoadingUnitsBufferUsage*/).First().Id, //TODO fill bays
                    ItemId = c.ItemId,
                    Quantity = c.Availability,
                    TypeId = "PK" // TODO convert table to enum
                }
                );
        }

        #endregion Methods
    }
}
