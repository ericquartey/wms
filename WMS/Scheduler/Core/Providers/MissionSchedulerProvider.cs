using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class MissionSchedulerProvider : IMissionSchedulerProvider
    {
        #region Fields

        private readonly ICompartmentSchedulerProvider compartmentProvider;

        private readonly DatabaseContext databaseContext;

        private readonly IItemSchedulerProvider itemProvider;

        private readonly ILogger<MissionSchedulerProvider> logger;

        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion

        #region Constructors

        public MissionSchedulerProvider(
            DatabaseContext databaseContext,
            ISchedulerRequestProvider schedulerRequestProvider,
            ICompartmentSchedulerProvider compartmentProvider,
            IItemSchedulerProvider itemProvider,
            IItemListSchedulerProvider itemListSchedulerProvider,
            ILogger<MissionSchedulerProvider> logger)
        {
            this.databaseContext = databaseContext;
            this.logger = logger;
            this.schedulerRequestProvider = schedulerRequestProvider;
            this.compartmentProvider = compartmentProvider;
            this.itemProvider = itemProvider;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Mission>> CreateForRequestsAsync(IEnumerable<SchedulerRequest> requests)
        {
            if (!requests.Any())
            {
                this.logger.LogDebug($"No scheduler requests are available for processing at the moment.");

                return new List<Mission>();
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                IEnumerable<Mission> missions = new List<Mission>();

                this.logger.LogDebug($"A total of {requests.Count()} requests need to be processed.");

                foreach (var request in requests)
                {
                    this.logger.LogDebug($"Scheduler Request (id={request.Id}) for item (id={request.ItemId}) is the next in line to be processed.");

                    switch (request.Type)
                    {
                        case OperationType.Withdrawal:
                            missions = await this.CreateWithdrawalMissionsAsync(request);
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

        public async Task<IEnumerable<Mission>> GetAllAsync()
        {
            return await this.databaseContext.Missions
                .Select(m => new Mission
                {
                    Id = m.Id,
                    BayId = m.BayId,
                    CellId = m.CellId,
                    CompartmentId = m.CompartmentId,
                    ItemId = m.ItemId,
                    ItemListId = m.ItemListId,
                    ItemListRowId = m.ItemListRowId,
                    LoadingUnitId = m.LoadingUnitId,
                    MaterialStatusId = m.MaterialStatusId,
                    PackageTypeId = m.PackageTypeId,
                    Lot = m.Lot,
                    RequestedQuantity = m.RequestedQuantity,
                    DispatchedQuantity = m.DispatchedQuantity,
                    RegistrationNumber = m.RegistrationNumber,
                    Status = (MissionStatus)m.Status,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    Type = (MissionType)m.Type
                })
                .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.databaseContext.Missions
                .CountAsync();
        }

        public async Task<Mission> GetByIdAsync(int id)
        {
            return await this.databaseContext.Missions
                .Select(m => new Mission
                {
                    Id = m.Id,
                    BayId = m.BayId,
                    CellId = m.CellId,
                    CompartmentId = m.CompartmentId,
                    ItemId = m.ItemId,
                    ItemListId = m.ItemListId,
                    ItemListRowId = m.ItemListRowId,
                    LoadingUnitId = m.LoadingUnitId,
                    MaterialStatusId = m.MaterialStatusId,
                    PackageTypeId = m.PackageTypeId,
                    Lot = m.Lot,
                    Priority = m.Priority,
                    RequestedQuantity = m.RequestedQuantity,
                    DispatchedQuantity = m.DispatchedQuantity,
                    RegistrationNumber = m.RegistrationNumber,
                    Status = (MissionStatus)m.Status,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    Type = (MissionType)m.Type
                })
                .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Mission>> GetByListRowIdAsync(int listRowId)
        {
            return await this.databaseContext.Missions
                .Where(m => m.ItemListRowId == listRowId)
                .Select(m => new Mission
                {
                    Status = (MissionStatus)m.Status
                })
                .ToArrayAsync();
        }

        public async Task<IOperationResult<Mission>> UpdateAsync(Mission model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.databaseContext.Missions.Find(model.Id);
            this.databaseContext.Entry(existingModel).CurrentValues.SetValues(model);

            await this.databaseContext.SaveChangesAsync();

            return new SuccessOperationResult<Mission>(model);
        }

        private static int ComputePriority(SchedulerRequest request)
        {
            if (request.IsInstant)
            {
                return 1;
            }

            return 0;
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
                    Priority = m.Priority,
                    RegistrationNumber = m.RegistrationNumber,
                    RequestedQuantity = m.RequestedQuantity,
                    Status = (Common.DataModels.MissionStatus)m.Status,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    Type = (Common.DataModels.MissionType)m.Type
                });

            await this.databaseContext.Missions.AddRangeAsync(missions);

            await this.databaseContext.SaveChangesAsync();
        }

        private async Task<IEnumerable<Mission>> CreateWithdrawalMissionsAsync(SchedulerRequest request)
        {
            var item = await this.itemProvider.GetByIdAsync(request.ItemId);

            var missions = new List<Mission>();
            var availableCompartments = true;
            while (request.QuantityLeftToDispatch > 0 && availableCompartments)
            {
                var compartments = this.compartmentProvider
                    .GetCandidateWithdrawalCompartments(request);

                var orderedCompartments = this.compartmentProvider
                    .OrderCompartmentsByManagementType(compartments, item.ManagementType);

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

                    await this.compartmentProvider.UpdateAsync(compartment);
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
                        Priority = ComputePriority(request),
                        RequestedQuantity = quantityToExtractFromCompartment,
                        Type = MissionType.Pick
                    };

                    this.logger.LogWarning(
                        $"Scheduler Request (id={request.Id}): generating withdrawal mission (CompartmentId={mission.CompartmentId}, " +
                        $"BayId={mission.BayId}, Quantity={mission.RequestedQuantity}). " +
                        $"A total quantity of {request.QuantityLeftToDispatch} still needs to be dispatched.");

                    missions.Add(mission);
                }
            }

            await this.CreateRangeAsync(missions);
            this.logger.LogDebug($"Scheduler Request (id={request.Id}): a total of {missions.Count} mission(s) were created.");

            return missions;
        }

        #endregion
    }
}
