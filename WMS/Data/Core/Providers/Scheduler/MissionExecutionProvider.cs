using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MissionExecutionProvider : IMissionExecutionProvider
    {
        #region Fields

        private readonly ICompartmentOperationProvider compartmentOperationProvider;

        private readonly DatabaseContext dataContext;

        private readonly IItemProvider itemProvider;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        private readonly ILogger<MissionExecutionProvider> logger;

        private readonly IMissionCreationProvider missionCreationProvider;

        private readonly IItemListRowExecutionProvider rowExecutionProvider;

        #endregion

        #region Constructors

        public MissionExecutionProvider(
            ICompartmentOperationProvider compartmentOperationProvider,
            IMissionCreationProvider missionCreationProvider,
            IItemListRowExecutionProvider rowExecutionProvider,
            IItemProvider itemProvider,
            ILoadingUnitProvider loadingUnitProvider,
            ILogger<MissionExecutionProvider> logger,
            DatabaseContext dataContext)
        {
            this.logger = logger;
            this.compartmentOperationProvider = compartmentOperationProvider;
            this.missionCreationProvider = missionCreationProvider;
            this.itemProvider = itemProvider;
            this.rowExecutionProvider = rowExecutionProvider;
            this.loadingUnitProvider = loadingUnitProvider;
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<MissionExecution>> CompleteItemAsync(int id, double quantity)
        {
            if (quantity <= 0)
            {
                return new BadRequestOperationResult<MissionExecution>(null, "Quantity cannot be negative or zero.");
            }

            var mission = await this.GetByIdAsync(id);
            if (mission == null)
            {
                return new NotFoundOperationResult<MissionExecution>();
            }

            if (mission.Status != MissionStatus.Executing)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    "Cannot complete the mission because it is not in the Executing state.");
            }

            IOperationResult<MissionExecution> result = null;
            switch (mission.Type)
            {
                case MissionType.Pick:
                    result = await this.CompleteItemPickMissionAsync(mission, quantity);
                    this.logger.LogDebug($"Completed mission id={mission.Id}");
                    break;

                default:
                    return new BadRequestOperationResult<MissionExecution>(null, "Only item pick operations are allowed.");
            }

            return result;
        }

        public async Task<IOperationResult<MissionExecution>> CompleteLoadingUnitAsync(int id)
        {
            var mission = await this.GetByIdAsync(id);
            if (mission == null)
            {
                return new NotFoundOperationResult<MissionExecution>();
            }

            if (mission.Status != MissionStatus.Executing)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    "Cannot complete the mission because it is not in the Executing state.");
            }

            IOperationResult<MissionExecution> result = null;
            switch (mission.Type)
            {
                case MissionType.Pick:
                    result = await this.CompleteLoadingUnitPickMissionAsync(mission);
                    this.logger.LogDebug($"Completed mission id={mission.Id}");
                    break;

                default:
                    return new BadRequestOperationResult<MissionExecution>(null, "Only item pick operations are allowed.");
            }

            return result;
        }

        public async Task<IEnumerable<MissionExecution>> CreateForRequestsAsync(IEnumerable<ISchedulerRequest> requests)
        {
            if (!requests.Any())
            {
                this.logger.LogDebug($"No scheduler requests are available for processing at the moment.");

                return new List<MissionExecution>();
            }

            this.logger.LogDebug($"A total of {requests.Count()} requests need to be processed.");
            var missions = new List<MissionExecution>();
            foreach (var request in requests)
            {
                this.logger.LogDebug($"Scheduler Request (id={request.Id}, type={request.Type}) is the next in line to be processed.");

                switch (request.OperationType)
                {
                    case OperationType.Withdrawal:
                        {
                            if (request is ItemSchedulerRequest itemRequest)
                            {
                                missions.AddRange(await this.missionCreationProvider.CreatePickMissionsAsync(itemRequest));
                            }
                            else if (request is LoadingUnitSchedulerRequest loadingUnitRequest)
                            {
                                missions.Add(await this.missionCreationProvider.CreateWithdrawalMissionAsync(loadingUnitRequest));
                            }

                            break;
                        }

                    case OperationType.Insertion:
                        this.logger.LogWarning($"Cannot process scheduler request id={request.Id} because insertion requests are not yet implemented.");
                        break;

                    case OperationType.Replacement:
                        this.logger.LogWarning($"Cannot process scheduler request id={request.Id} because replacement requests are not yet implemented.");
                        break;

                    case OperationType.Reorder:
                        this.logger.LogWarning($"Cannot process scheduler request id={request.Id} because reorder requests are not yet implemented.");
                        break;

                    default:
                        this.logger.LogError($"Cannot process scheduler request id={request.Id} because operation type cannot be understood.");
                        break;
                }
            }

            return missions;
        }

        public async Task<IOperationResult<MissionExecution>> ExecuteAsync(int id)
        {
            var mission = await this.GetByIdAsync(id);
            if (mission == null)
            {
                return new NotFoundOperationResult<MissionExecution>();
            }

            if (mission.Status != MissionStatus.New)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    "Unable to execute mission, because it is not new or in the Waiting state");
            }

            mission.Status = MissionStatus.Executing;
            var result = await this.UpdateAsync(mission);

            if (mission.ItemListRowId.HasValue)
            {
                var row = await this.rowExecutionProvider.GetByIdAsync(mission.ItemListRowId.Value);
                await this.UpdateRowStatusAsync(row, DateTime.UtcNow);
            }

            return result;
        }

        public async Task<IEnumerable<MissionExecution>> GetAllAsync()
        {
            return await this.dataContext.Missions
                .Select(m => new MissionExecution
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
                .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.dataContext.Missions
                .CountAsync();
        }

        public async Task<MissionExecution> GetByIdAsync(int id)
        {
            return await this.dataContext.Missions
                .Select(m => new MissionExecution
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

        public async Task<IEnumerable<MissionExecution>> GetByListRowIdAsync(int listRowId)
        {
            return await this.dataContext.Missions
                .Where(m => m.ItemListRowId == listRowId)
                .Select(m => new MissionExecution
                {
                    Id = m.Id,
                    Status = (MissionStatus)m.Status,
                    DispatchedQuantity = m.DispatchedQuantity,
                    RequestedQuantity = m.RequestedQuantity,
                    Priority = m.Priority
                })
                .ToArrayAsync();
        }

        public async Task<IOperationResult<MissionExecution>> UpdateAsync(MissionExecution model)
        {
            return await this.UpdateAsync(
                model,
                this.dataContext.Missions,
                this.dataContext);
        }

        public async Task UpdateRowStatusAsync(ItemListRowOperation row, DateTime now)
        {
            var involvedMissions = await this.GetByListRowIdAsync(row.Id);

            var completeMissionsCount = involvedMissions.Count(m => m.Status == MissionStatus.Completed);
            var hasExecutingMissions = involvedMissions.Any(m => m.Status == MissionStatus.Executing);
            var hasErroredMissions = involvedMissions.Any(m => m.Status == MissionStatus.Error);
            var hasIncompleteMissions = involvedMissions.Any(m => m.Status == MissionStatus.Incomplete);

            if (!involvedMissions.Any())
            {
                row.Status = ItemListRowStatus.New;
            }
            else if (completeMissionsCount == involvedMissions.Count()
                && involvedMissions.Sum(m => m.DispatchedQuantity).CompareTo(row.RequestedQuantity) == 0)
            {
                row.Status = ItemListRowStatus.Completed;
                row.CompletionDate = now;
            }
            else if (hasErroredMissions)
            {
                row.Status = ItemListRowStatus.Error;
            }
            else if (hasExecutingMissions)
            {
                row.Status = ItemListRowStatus.Executing;
                row.LastExecutionDate = now;
            }
            else if (hasIncompleteMissions)
            {
                row.Status = ItemListRowStatus.Incomplete;
            }

            await this.rowExecutionProvider.UpdateAsync(row);
        }

        private static void UpdateCompartment(StockUpdateCompartment compartment, double quantity, DateTime now)
        {
            compartment.ReservedForPick -= quantity;
            compartment.Stock -= quantity;

            if (compartment.Stock.CompareTo(0) == 0
                && !compartment.IsItemPairingFixed)
            {
                compartment.ItemId = null;
            }

            compartment.LastPickDate = now;
        }

        private static void UpdateItem(ItemAvailable item, DateTime now)
        {
            item.LastPickDate = now;
        }

        private static void UpdateLoadingUnit(LoadingUnitOperation loadingUnit, DateTime now)
        {
            loadingUnit.LastPickDate = now;
        }

        private static void UpdateMission(MissionExecution mission, double? quantity)
        {
            if (quantity.HasValue)
            {
                mission.DispatchedQuantity += quantity.Value;
            }

            mission.Status = mission.QuantityRemainingToDispatch.CompareTo(0) == 0
                ? MissionStatus.Completed
                : MissionStatus.Incomplete;
        }

        private async Task<IOperationResult<MissionExecution>> CompleteItemPickMissionAsync(MissionExecution mission, double quantity)
        {
            if (!mission.CompartmentId.HasValue
               || !mission.ItemId.HasValue)
            {
                throw new InvalidOperationException();
            }

            if (quantity > mission.QuantityRemainingToDispatch)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    $"Requested quantity ({quantity}) cannot be greater than the remaining quantity to dispatch ({mission.QuantityRemainingToDispatch}).");
            }

            var now = DateTime.UtcNow;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var compartment = await this.compartmentOperationProvider.GetByIdForStockUpdateAsync(mission.CompartmentId.Value);
                var loadingUnit = await this.loadingUnitProvider.GetByIdForExecutionAsync(compartment.LoadingUnitId);
                var item = await this.itemProvider.GetByIdForExecutionAsync(mission.ItemId.Value);

                UpdateCompartment(compartment, quantity, now);

                UpdateLoadingUnit(loadingUnit, now);

                UpdateItem(item, now);

                UpdateMission(mission, quantity);

                var result = await this.UpdateAsync(mission);
                await this.loadingUnitProvider.UpdateAsync(loadingUnit);
                await this.itemProvider.UpdateAsync(item);
                await this.compartmentOperationProvider.UpdateAsync(compartment);

                if (mission.ItemListRowId.HasValue)
                {
                    var row = await this.rowExecutionProvider.GetByIdAsync(mission.ItemListRowId.Value);
                    await this.UpdateRowStatusAsync(row, now);
                }

                scope.Complete();

                return result;
            }
        }

        private async Task<IOperationResult<MissionExecution>> CompleteLoadingUnitPickMissionAsync(MissionExecution mission)
        {
            if (!mission.LoadingUnitId.HasValue)
            {
                throw new InvalidOperationException();
            }

            var now = DateTime.UtcNow;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var loadingUnit = await this.loadingUnitProvider.GetByIdForExecutionAsync(mission.LoadingUnitId.Value);

                UpdateLoadingUnit(loadingUnit, now);
                UpdateMission(mission, null);

                var result = await this.UpdateAsync(mission);
                await this.loadingUnitProvider.UpdateAsync(loadingUnit);
                scope.Complete();

                return result;
            }
        }

        #endregion
    }
}
