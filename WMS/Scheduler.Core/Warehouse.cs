using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.Core
{
    public class Warehouse : IWarehouse
    {
        #region Fields

        private readonly IDataProvider dataProvider;
        private readonly ILogger<Warehouse> logger;
        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion Fields

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

        #endregion Constructors

        #region Methods

        public async Task<IEnumerable<Mission>> DispatchRequests()
        {
            var request = await this.dataProvider.GetNextRequestToProcessAsync();
            if (request == null)
            {
                return null;
            }

            this.logger.LogDebug($"Request for item={request.ItemId} is the next in line to be processed.");
            switch (request.Type)
            {
                case OperationType.Withdrawal:
                    return await this.DispatchWithdrawalRequest(request);

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

        public async Task<SchedulerRequest> Withdraw(SchedulerRequest request)
        {
            SchedulerRequest qualifiedRequest = null;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                qualifiedRequest = await this.schedulerRequestProvider.FullyQualifyWithdrawalRequest(request);
                if (qualifiedRequest != null)
                {
                    this.dataProvider.Add(qualifiedRequest);

                    scope.Complete();
                    this.logger.LogDebug($"Withdrawal request for item={request.ItemId} was accepted and stored.");
                }
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await this.DispatchRequests();
                scope.Complete();
            }

            return qualifiedRequest;
        }

        private async Task<IEnumerable<Mission>> DispatchWithdrawalRequest(SchedulerRequest request)
        {
            if (!request.IsInstant)
            // TODO: extend this method to support normal (non-instant) withdrawal requests
            {
                throw new NotImplementedException("Only instant requests are supported.");
            }

            var item = await this.dataProvider.GetItemByIdAsync(request.ItemId);

            var missions = new List<Mission>();
            while (request.RequestedQuantity > request.DispatchedQuantity)
            {
                var bay = await this.GetNextEmptyBay(request.AreaId, request.BayId);
                if (bay == null)
                {
                    break;
                }

                var compartments = this.schedulerRequestProvider.GetCandidateWithdrawalCompartments(request);

                var orderedCompartments =
                    this.schedulerRequestProvider.OrderCompartmentsByManagementType(compartments, item.ManagementType);

                var compartment = orderedCompartments.FirstOrDefault();
                if (compartment == null)
                {
                    this.logger.LogWarning($"Request id={request.Id} cannot be satisfied because no matching compartments were found.");
                    break;
                }

                var quantityLeftToDispatch = request.RequestedQuantity - request.DispatchedQuantity;
                var quantityToExtractFromCompartment = Math.Min(compartment.Availability, quantityLeftToDispatch);
                compartment.ReservedForPick += quantityToExtractFromCompartment;
                request.DispatchedQuantity += quantityToExtractFromCompartment;

                this.dataProvider.Update(compartment);
                this.dataProvider.Update(request);

                var mission = new Mission
                {
                    ItemId = item.Id,
                    BayId = bay.Id,
                    CellId = compartment.CellId,
                    CompartmentId = compartment.Id,
                    LoadingUnitId = compartment.LoadingUnitId,
                    // ItemListId = request.ListId, // TODO: extend this method to support normal (non-instant) withdrawal requests
                    // ItemListRowId = request.ListRowId, // TODO: extend this method to support normal (non-instant) withdrawal requests
                    MaterialStatusId = compartment.MaterialStatusId,
                    Sub1 = compartment.Sub1,
                    Sub2 = compartment.Sub2,
                    Quantity = quantityToExtractFromCompartment,
                    Type = MissionType.Pick
                };

                this.logger.LogWarning($"Generating withdrawal mission from request id={request.Id}.");

                missions.Add(mission);
            }

            this.dataProvider.AddRange(missions);

            return missions;
        }

        private async Task<Bay> GetNextEmptyBay(int areaId, int? bayId)
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

        #endregion Methods
    }
}
