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

        private readonly IItemCompartmentTypeProvider itemCompartmentTypeProvider;

        private readonly IItemProvider itemProvider;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        private readonly ILogger<MissionExecutionProvider> logger;

        private readonly IItemListRowExecutionProvider rowExecutionProvider;

        #endregion

        #region Constructors

        public MissionExecutionProvider(
            ICompartmentOperationProvider compartmentOperationProvider,
            IItemListRowExecutionProvider rowExecutionProvider,
            IItemProvider itemProvider,
            ILoadingUnitProvider loadingUnitProvider,
            IItemCompartmentTypeProvider itemCompartmentTypeProvider,
            ILogger<MissionExecutionProvider> logger,
            DatabaseContext dataContext)
        {
            this.logger = logger;
            this.compartmentOperationProvider = compartmentOperationProvider;
            this.itemProvider = itemProvider;
            this.rowExecutionProvider = rowExecutionProvider;
            this.loadingUnitProvider = loadingUnitProvider;
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<MissionExecution>> CompleteItemAsync(int id, double quantity)
        {
            var mission = await this.GetByIdAsync(id);
            if (mission == null)
            {
                return new NotFoundOperationResult<MissionExecution>(null, $"No mission with id '{id}' exists.");
            }

            if (quantity <= 0)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    $"Value '{quantity}' represents an invalid quantity. Dispatched mission quantity cannot be negative or zero.");
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
                    this.logger.LogDebug($"Completed pick mission id={mission.Id}");
                    break;

                case MissionType.Put:
                    result = await this.CompleteItemPutMissionAsync(mission, quantity);
                    this.logger.LogDebug($"Completed put mission id={mission.Id}");
                    break;

                default:
                    return new BadRequestOperationResult<MissionExecution>(
                        null,
                        $"Completion is not supported for mission type '{mission.Type}'.");
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
                    result = await this.CompleteLoadingUnitWithdrawMissionAsync(mission);
                    this.logger.LogDebug($"Completed mission id={mission.Id}");
                    break;

                default:
                    return new BadRequestOperationResult<MissionExecution>(null, "Only item pick operations are allowed.");
            }

            return result;
        }

        public async Task<IOperationResult<MissionExecution>> ExecuteAsync(int id)
        {
            var mission = await this.GetByIdAsync(id);
            if (mission == null)
            {
                return new NotFoundOperationResult<MissionExecution>();
            }

            if (mission.Status != MissionStatus.New
                &&
                mission.Status != MissionStatus.Waiting)
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
            var hasWaitingMissions = involvedMissions.Any(m => m.Status == MissionStatus.Waiting);
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
            else if (hasWaitingMissions)
            {
                row.Status = ItemListRowStatus.Waiting;
            }
            else if (hasIncompleteMissions)
            {
                row.Status = ItemListRowStatus.Incomplete;
            }

            await this.rowExecutionProvider.UpdateAsync(row);
        }

        private static void UpdateCompartmentAfterPick(StockUpdateCompartment compartment, double quantity, DateTime now)
        {
            compartment.ReservedForPick -= quantity;
            compartment.Stock -= quantity;

            if (compartment.Stock.CompareTo(0) == 0)
            {
                if (!compartment.IsItemPairingFixed)
                {
                    compartment.ItemId = null;
                }

                compartment.Lot = null;
                compartment.MaterialStatusId = null;
                compartment.PackageTypeId = null;
                compartment.RegistrationNumber = null;
                compartment.Sub1 = null;
                compartment.Sub2 = null;
            }

            compartment.LastPickDate = now;
        }

        private static void UpdateMissionQuantity(MissionExecution mission, double? quantity)
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
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    "Unable to complete the specified mission because it has no associated compartment.");
            }

            if (mission.ItemId.HasValue == false)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    "Unable to complete the specified mission because it has no associated item.");
            }

            if (quantity > mission.QuantityRemainingToDispatch)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    $"Actual picked quantity ({quantity}) cannot be greater than the remaining quantity to dispatch ({mission.QuantityRemainingToDispatch}).");
            }

            if (quantity <= 0)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    "Unable to complete the specified mission. " +
                    $"Actual put quantity ({quantity}) cannot be negative or zero.");
            }

            var now = DateTime.UtcNow;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var compartment = await this.compartmentOperationProvider.GetByIdForStockUpdateAsync(mission.CompartmentId.Value);
                var loadingUnit = await this.loadingUnitProvider.GetByIdForExecutionAsync(compartment.LoadingUnitId);
                var item = await this.itemProvider.GetByIdForExecutionAsync(mission.ItemId.Value);

                UpdateCompartmentAfterPick(compartment, quantity, now);

                loadingUnit.LastPickDate = now;

                item.LastPickDate = now;

                UpdateMissionQuantity(mission, quantity);

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

        private async Task<IOperationResult<MissionExecution>> CompleteItemPutMissionAsync(
            MissionExecution mission,
            double quantity)
        {
            if (mission.CompartmentId.HasValue == false)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    "Unable to complete the specified mission. The mission has no associated compartment.");
            }

            if (mission.ItemId.HasValue == false)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    "Unable to complete the specified mission. The mission has no associated item.");
            }

            if (quantity <= 0)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    "Unable to complete the specified mission. " +
                    $"Actual put quantity ({quantity}) cannot be negative or zero.");
            }

            if (quantity > mission.QuantityRemainingToDispatch)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    "Unable to complete the specified mission. " +
                    $"Actual put quantity ({quantity}) cannot be greater than the remaining quantity to dispatch ({mission.QuantityRemainingToDispatch}).");
            }

            var now = DateTime.UtcNow;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var compartment = await this.compartmentOperationProvider.GetByIdForStockUpdateAsync(mission.CompartmentId.Value);
                var loadingUnit = await this.loadingUnitProvider.GetByIdForExecutionAsync(compartment.LoadingUnitId);
                var item = await this.itemProvider.GetByIdForExecutionAsync(mission.ItemId.Value);

                await this.UpdateCompartmentAfterPutAsync(compartment, item.Id, mission, quantity, now);

                loadingUnit.LastPutDate = now;

                item.LastPutDate = now;

                UpdateMissionQuantity(mission, quantity);

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

        private async Task<IOperationResult<MissionExecution>> CompleteLoadingUnitWithdrawMissionAsync(MissionExecution mission)
        {
            System.Diagnostics.Debug.Assert(
                mission != null,
                $"The method argument {nameof(mission)} should not be null.");

            if (!mission.LoadingUnitId.HasValue)
            {
                throw new InvalidOperationException(
                    "Unable to complete the specified mission. The mission has no associated loading unit.");
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var loadingUnit = await this.loadingUnitProvider.GetByIdForExecutionAsync(mission.LoadingUnitId.Value);

                UpdateMissionQuantity(mission, null);

                var result = await this.UpdateAsync(mission);
                await this.loadingUnitProvider.UpdateAsync(loadingUnit);
                scope.Complete();

                return result;
            }
        }

        private async Task UpdateCompartmentAfterPutAsync(
            StockUpdateCompartment compartment,
            int itemId,
            MissionExecution mission,
            double quantity,
            DateTime now)
        {
            if (compartment.Stock.Equals(0))
            {
                compartment.ItemId = itemId;
                compartment.MaterialStatusId = mission.MaterialStatusId;
                compartment.PackageTypeId = mission.PackageTypeId;
                compartment.RegistrationNumber = mission.RegistrationNumber;
                compartment.Sub1 = mission.Sub1;
                compartment.Sub2 = mission.Sub2;
            }

            compartment.ReservedForPut -= quantity;
            compartment.Stock += quantity;
            compartment.LastPutDate = now;

            if (compartment.MaxCapacity.HasValue
                &&
                compartment.Stock > compartment.MaxCapacity.Value)
            {
                await this.itemCompartmentTypeProvider.UpdateAsync(
                     new ItemCompartmentType
                     {
                         ItemId = compartment.ItemId.Value,
                         CompartmentTypeId = compartment.ItemCompartmentTypeId,
                         MaxCapacity = compartment.Stock
                     });
            }
        }

        #endregion
    }
}
