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

        private readonly ILoadingUnitSchedulerProvider loadingUnitProvider;

        private readonly ILogger<MissionSchedulerProvider> logger;

        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion

        #region Constructors

        public MissionSchedulerProvider(
            DatabaseContext databaseContext,
            ISchedulerRequestProvider schedulerRequestProvider,
            ICompartmentSchedulerProvider compartmentProvider,
            IItemSchedulerProvider itemProvider,
            ILoadingUnitSchedulerProvider loadingUnitProvider,
            IItemListSchedulerProvider itemListSchedulerProvider,
            ILogger<MissionSchedulerProvider> logger)
        {
            this.databaseContext = databaseContext;
            this.logger = logger;
            this.schedulerRequestProvider = schedulerRequestProvider;
            this.compartmentProvider = compartmentProvider;
            this.loadingUnitProvider = loadingUnitProvider;
            this.itemProvider = itemProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<Mission>> CompleteAsync(int id)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var mission = await this.GetByIdAsync(id);
                if (mission == null)
                {
                    return new NotFoundOperationResult<Mission>();
                }

                if (mission.Status != MissionStatus.Executing)
                {
                    return new BadRequestOperationResult<Mission>(mission);
                }

                IOperationResult<Mission> result = null;
                switch (mission.Type)
                {
                    case MissionType.Pick:
                        result = await this.CompletePickMissionAsync(mission);
                        break;

                    default:
                        throw new NotSupportedException("Only item pick operations are allowed.");
                }

                scope.Complete();

                return result;
            }
        }

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

        public async Task<IOperationResult<Mission>> ExecuteAsync(int id)
        {
            var mission = await this.GetByIdAsync(id);
            if (mission == null)
            {
                return new NotFoundOperationResult<Mission>();
            }

            if (mission.Status != MissionStatus.New
                &&
                mission.Status != MissionStatus.Waiting)
            {
                return new BadRequestOperationResult<Mission>(mission);
            }

            mission.Status = MissionStatus.Executing;

            return await this.UpdateAsync(mission);
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
                    Quantity = m.RequiredQuantity,
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
                    Quantity = m.RequiredQuantity,
                    RegistrationNumber = m.RegistrationNumber,
                    Status = (MissionStatus)m.Status,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    Type = (MissionType)m.Type
                })
                .SingleOrDefaultAsync(m => m.Id == id);
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

        private async Task<IOperationResult<Mission>> CompletePickMissionAsync(Mission mission)
        {
            if (mission.CompartmentId.HasValue == false
               || mission.ItemId.HasValue == false)
            {
                throw new InvalidOperationException();
            }

            var compartment = await this.compartmentProvider
                .GetByIdForStockUpdateAsync(mission.CompartmentId.Value);

            compartment.ReservedForPick -= mission.Quantity;
            compartment.Stock -= mission.Quantity;

            if (compartment.Stock == 0
                && compartment.IsItemPairingFixed == false)
            {
                compartment.ItemId = null;
            }

            await this.UpdateLastPickDatesAsync(mission.ItemId.Value, compartment);

            await this.compartmentProvider.UpdateAsync(compartment);

            mission.Status = MissionStatus.Completed;

            return await this.UpdateAsync(mission);
        }

        private async Task<IEnumerable<Mission>> CreateMissionsFromRequestAsync(SchedulerRequest request)
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

            await this.databaseContext.SaveChangesAsync();
        }

        private async Task UpdateLastPickDatesAsync(int itemId, StockUpdateCompartment compartment)
        {
            var now = DateTime.UtcNow;
            compartment.LastPickDate = now;

            var item = await this.itemProvider.GetByIdAsync(itemId);
            var loadingUnit = await this.loadingUnitProvider.GetByIdAsync(compartment.LoadingUnitId);

            item.LastPickDate = now;
            loadingUnit.LastPickDate = now;

            await this.loadingUnitProvider.UpdateAsync(loadingUnit);

            await this.itemProvider.UpdateAsync(item);
        }

        #endregion
    }
}
