using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BusinessModels;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.Core
{
    public class Warehouse : IWarehouse
    {
        #region Fields

        private readonly ILogger<Warehouse> logger;
        private readonly IMissionProvider missionProvider;
        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion Fields

        #region Constructors

        public Warehouse(
           IMissionProvider missionProvider,
           ISchedulerRequestProvider schedulerRequestProvider,
           ILogger<Warehouse> logger)
        {
            this.missionProvider = missionProvider;
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

        private async void DispatchWithdrawalRequest(SchedulerRequest request)
        {
            if (!request.IsInstant)
            {
                throw new NotImplementedException(); //TODO extend this method to support normal (non-instant) withdrawal requests
            }

            var compartments = this.schedulerRequestProvider.GetCandidateWithdrawalCompartments(request);

            var item = await this.missionProvider.GetItemByIdAsync(request.ItemId);

            var orderedCompartments =
                this.schedulerRequestProvider.OrderCompartmentsByManagementType(compartments, item.ManagementType);

            var neededCompartmentsCount = orderedCompartments.Aggregate(
                new Tuple<int, int>(0, 0),
                (Tuple<int, int> total, IOrderableCompartment compartment) =>
                    total.Item1 >= request.RequestedQuantity
                    ?
                    total
                    : new Tuple<int, int>(total.Item1 + compartment.Availability, total.Item2 + 1)
            );

            this.logger.LogDebug($"A total of {neededCompartmentsCount} is needed to complete the request for item id={request.ItemId}");

            var missions = orderedCompartments
                .Cast<Compartment>()
                .Take(neededCompartmentsCount.Item2)
                .Select(c => new Mission
                {
                    BayId = c.Bays.OrderByDescending(b => b.LoadingUnitsBufferSize).First().Id, // TODO: TASK-786 do proper selection of bay based on actual buffer status
                    ItemId = c.ItemId,
                    CellId = c.CellId,
                    CompartmentId = c.Id,
                    ItemListId = request.ListId,
                    ItemListRowId = request.ListRowId,
                    MaterialStatusId = c.MaterialStatusId,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    Quantity = c.Availability, // TODO: TASK-787 take only as much items as needed to satisfy the request
                    Type = MissionType.Pick
                }
            );

            //
            // TODO: TASK-788 select and save only the missions that can be queued, given the current buffer status of the bays
            //
            // TODO: TASK-789 update the request when all the quantity that still was not satisfied, or delete it if it was fully satisfied
            //

            await this.missionProvider.AddRange(missions);
        }

        #endregion Methods
    }
}
