using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class MissionSchedulerProvider : IMissionSchedulerProvider
    {
        #region Fields

        private readonly ICompartmentSchedulerProvider compartmentSchedulerProvider;

        private readonly DatabaseContext databaseContext;

        private readonly IItemSchedulerProvider itemSchedulerProvider;

        private readonly ILogger<MissionSchedulerProvider> logger;

        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion

        #region Constructors

        public MissionSchedulerProvider(
            DatabaseContext databaseContext,
            ISchedulerRequestProvider schedulerRequestProvider,
            ICompartmentSchedulerProvider compartmentSchedulerProvider,
            IItemSchedulerProvider itemSchedulerProvider,
            ILogger<MissionSchedulerProvider> logger)
        {
            this.databaseContext = databaseContext;
            this.logger = logger;
            this.schedulerRequestProvider = schedulerRequestProvider;
            this.compartmentSchedulerProvider = compartmentSchedulerProvider;
            this.itemSchedulerProvider = itemSchedulerProvider;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Mission>> CreateForPendingRequestsAsync()
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                IEnumerable<Mission> missions = new List<Mission>();

                var requests = await this.schedulerRequestProvider.GetRequestsToProcessAsync();
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
                            missions = await this.CreateMissionsFromRequestAsync(request);
                            break;

                        case OperationType.Insertion:
                            throw new NotImplementedException($"Cannot process scheduler request id={request.Id} because insertion requests are not yet implemented.");

                        case OperationType.Replacement:
                            throw new NotImplementedException($"Cannot process scheduler request id={request.Id} because replacement requests are not yet implemented.");

                        case OperationType.Reorder:
                            throw new NotImplementedException($"Cannot process scheduler request id={request.Id} because reorder requests are not yet implemented.");

                        default:
                            throw new InvalidOperationException($"Cannot process scheduler request id={request.Id} because operation type cannot be understood.");
                    }
                }

                scope.Complete();

                return missions;
            }
        }

        public async Task<Mission> UpdateAsync(Mission mission)
        {
            if (mission == null)
            {
                throw new ArgumentNullException(nameof(mission));
            }

            var existingModel = this.databaseContext.Missions.Find(mission.Id);
            this.databaseContext.Entry(existingModel).CurrentValues.SetValues(mission);

            await this.databaseContext.SaveChangesAsync();

            return mission;
        }

        private async Task<IEnumerable<Mission>> CreateMissionsFromRequestAsync(SchedulerRequest request)
        {
            var item = await this.itemSchedulerProvider.GetByIdAsync(request.ItemId);

            var missions = new List<Mission>();
            var availableCompartments = true;
            while (request.QuantityLeftToDispatch > 0 && availableCompartments)
            {
                var compartments = this.schedulerRequestProvider.GetCandidateWithdrawalCompartments(request);

                var orderedCompartments =
                    this.schedulerRequestProvider.OrderCompartmentsByManagementType(compartments, item.ManagementType);

                var compartment = orderedCompartments.FirstOrDefault();
                if (compartment == null)
                {
                    this.logger.LogWarning($"Scheduler Request (id={request.Id}): no more compartments can fulfill the request at the moment.");
                    availableCompartments = false;
                }
                else
                {
                    var quantityToExtractFromCompartment = Math.Min(compartment.Availability, request.QuantityLeftToDispatch);
                    compartment.ReservedForPick += quantityToExtractFromCompartment;
                    request.DispatchedQuantity += quantityToExtractFromCompartment;

                    await this.compartmentSchedulerProvider.UpdateAsync(compartment);
                    await this.schedulerRequestProvider.UpdateAsync(request);

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
                        $"Scheduler Request (id={request.Id}): generating withdrawal mission (CompartmentId={mission.CompartmentId}, " +
                        $"BayId={mission.BayId}, Quantity={mission.Quantity}). " +
                        $"A total quantity of {request.QuantityLeftToDispatch} still needs to be dispatched.");

                    missions.Add(mission);
                }
            }

            await this.CreateRangeAsync(missions);
            this.logger.LogDebug($"Scheduler Request (id={request.Id}): a total of {missions.Count} mission(s) were created.");

            return missions;
        }

        private async Task CreateRangeAsync(IEnumerable<Mission> models)
        {
            var missions = models.Select(
                m => new Common.DataModels.Mission
                {
                    BayId = m.BayId,
                    CellId = m.CellId,
                    CompartmentId = m.CompartmentId,
                    ItemId = m.ItemId,
                    ItemListId = m.ItemListId,
                    ItemListRowId = m.ItemListRowId,
                    LoadingUnitId = m.LoadingUnitId,
                    MaterialStatusId = m.MaterialStatusId,
                    PackageTypeId = m.PackageTypeId,

                    // TODO: add Priority field
                    RegistrationNumber = m.RegistrationNumber,
                    RequiredQuantity = m.Quantity,
                    Status = (Common.DataModels.MissionStatus)m.Status,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    Type = (Common.DataModels.MissionType)m.Type
                });

            await this.databaseContext.Missions.AddRangeAsync(missions);
        }

        #endregion
    }
}
