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
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                IEnumerable<Mission> missions = new List<Mission>();

                var requests = await this.dataProvider.GetRequestsToProcessAsync();
                if (requests.Any() == false)
                {
                    this.logger.LogDebug($"No more scheduler requests are available for processing at the moment.");
                    return missions;
                }

                this.logger.LogDebug($"A total of {requests.Count()} requests need to be processed.");

                foreach (var request in requests)
                {
                    this.logger.LogDebug($"Scheduler Request (id={request.Id}) for item (id={request.ItemId}) is the next in line to be processed.");

                    switch (request.Type)
                    {
                        case OperationType.Withdrawal:
                            missions = await this.DispatchWithdrawalRequest(request);
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
                scope.Complete();

                return missions;
            }
        }

        public async Task<SchedulerRequest> ExecuteListAsync(int listId, int bayId)
        {
            throw new NotImplementedException();
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
                    this.logger.LogDebug($"Scheduler Request (id={qualifiedRequest.Id}): Withdrawal for item={qualifiedRequest.ItemId} was accepted and stored.");
                }
            }

            await this.DispatchRequests();

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
            while (request.QuantityLeftToDispatch > 0)
            {
                var compartments = this.schedulerRequestProvider.GetCandidateWithdrawalCompartments(request);

                var orderedCompartments =
                    this.schedulerRequestProvider.OrderCompartmentsByManagementType(compartments, item.ManagementType);

                var compartment = orderedCompartments.FirstOrDefault();
                if (compartment == null)
                {
                    this.logger.LogWarning($"Scheduler Request (id={request.Id}): no more compartments can fulfill the request at the moment.");
                    break;
                }

                var quantityToExtractFromCompartment = Math.Min(compartment.Availability, request.QuantityLeftToDispatch);
                compartment.ReservedForPick += quantityToExtractFromCompartment;
                request.DispatchedQuantity += quantityToExtractFromCompartment;

                this.dataProvider.Update(compartment);
                this.dataProvider.Update(request);

                var mission = new Mission
                {
                    ItemId = item.Id,
                    BayId = request.BayId.Value,
                    CellId = compartment.CellId,
                    CompartmentId = compartment.Id,
                    LoadingUnitId = compartment.LoadingUnitId,
                    ItemListId = request.ListId,
                    ItemListRowId = request.ListRowId,
                    MaterialStatusId = compartment.MaterialStatusId,
                    Sub1 = compartment.Sub1,
                    Sub2 = compartment.Sub2,
                    Quantity = quantityToExtractFromCompartment,
                    Type = MissionType.Pick
                };

                this.logger.LogWarning(
                    $"Scheduler Request (id={request.Id}): generating withdrawal mission (CompartmentId={mission.CompartmentId}, BayId={mission.BayId}, Quantity={mission.Quantity}). A total quantity of {request.QuantityLeftToDispatch} still needs to be dispatched.");

                missions.Add(mission);
            }

            this.dataProvider.AddRange(missions);
            this.logger.LogDebug($"Scheduler Request (id={request.Id}): a total of {missions.Count} mission(s) were created.");

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
