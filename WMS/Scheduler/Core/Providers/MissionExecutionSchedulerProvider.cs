using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class MissionExecutionSchedulerProvider : IMissionExecutionSchedulerProvider
    {
        #region Fields

        private readonly ICompartmentSchedulerProvider compartmentProvider;

        private readonly IItemSchedulerProvider itemProvider;

        private readonly ILoadingUnitSchedulerProvider loadingUnitProvider;

        private readonly ILogger<MissionExecutionSchedulerProvider> logger;

        private readonly IMissionSchedulerProvider missionProvider;

        private readonly IItemListRowSchedulerProvider rowProvider;

        #endregion

        #region Constructors

        public MissionExecutionSchedulerProvider(
            ICompartmentSchedulerProvider compartmentProvider,
            IMissionSchedulerProvider missionProvider,
            IItemListRowSchedulerProvider rowProvider,
            IItemSchedulerProvider itemProvider,
            ILoadingUnitSchedulerProvider loadingUnitProvider,
            ILogger<MissionExecutionSchedulerProvider> logger)
        {
            this.logger = logger;
            this.compartmentProvider = compartmentProvider;
            this.missionProvider = missionProvider;
            this.itemProvider = itemProvider;
            this.rowProvider = rowProvider;
            this.loadingUnitProvider = loadingUnitProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<Mission>> CompleteAsync(int id, int quantity)
        {
            if (quantity <= 0)
            {
                return new BadRequestOperationResult<Mission>(null, "Quantity cannot be negative or zero.");
            }

            var mission = await this.missionProvider.GetByIdAsync(id);
            if (mission == null)
            {
                return new NotFoundOperationResult<Mission>();
            }

            if (mission.Status != MissionStatus.Executing)
            {
                return new BadRequestOperationResult<Mission>(
                    mission,
                    "Cannot complete the mission because it is not in the Executing state.");
            }

            IOperationResult<Mission> result = null;
            switch (mission.Type)
            {
                case MissionType.Pick:
                    result = await this.CompletePickMissionAsync(mission, quantity);
                    this.logger.LogDebug($"Completed mission id={mission.Id}");
                    break;

                default:
                    return new BadRequestOperationResult<Mission>(null, "Only item pick operations are allowed.");
            }

            return result;
        }

        public async Task<IOperationResult<Mission>> ExecuteAsync(int id)
        {
            var mission = await this.missionProvider.GetByIdAsync(id);
            if (mission == null)
            {
                return new NotFoundOperationResult<Mission>();
            }

            if (mission.Status != MissionStatus.New
                &&
                mission.Status != MissionStatus.Waiting)
            {
                return new BadRequestOperationResult<Mission>(
                    mission,
                    "Unable to execute mission, because it is not new or in the Waiting state");
            }

            mission.Status = MissionStatus.Executing;
            var result = await this.missionProvider.UpdateAsync(mission);

            if (mission.ItemListRowId.HasValue)
            {
                var row = await this.rowProvider.GetByIdAsync(mission.ItemListRowId.Value);
                await this.UpdateRowStatusAsync(row, DateTime.UtcNow);
            }

            return result;
        }

        public async Task UpdateRowStatusAsync(ItemListRow row, DateTime now)
        {
            var involvedMissions = await this.missionProvider.GetByListRowIdAsync(row.Id);

            var completeMissionsCount = involvedMissions.Count(m => m.Status == MissionStatus.Completed);
            var hasWaitingMissions = involvedMissions.Any(m => m.Status == MissionStatus.Waiting);
            var hasExecutingMissions = involvedMissions.Any(m => m.Status == MissionStatus.Executing);
            var hasErroredMissions = involvedMissions.Any(m => m.Status == MissionStatus.Error);
            var hasIncompleteMissions = involvedMissions.Any(m => m.Status == MissionStatus.Incomplete);

            if (involvedMissions.Any() == false)
            {
                row.Status = ItemListRowStatus.New;
            }
            else if (completeMissionsCount == involvedMissions.Count()
                && involvedMissions.Sum(m => m.DispatchedQuantity) == row.RequestedQuantity)
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

            await this.rowProvider.UpdateAsync(row);
        }

        private static void UpdateCompartment(StockUpdateCompartment compartment, int quantity, DateTime now)
        {
            compartment.ReservedForPick -= quantity;
            compartment.Stock -= quantity;

            if (compartment.Stock == 0
                && compartment.IsItemPairingFixed == false)
            {
                compartment.ItemId = null;
            }

            compartment.LastPickDate = now;
        }

        private static void UpdateItem(Item item, DateTime now)
        {
            item.LastPickDate = now;
        }

        private static void UpdateLoadingUnit(LoadingUnit loadingUnit, DateTime now)
        {
            loadingUnit.LastPickDate = now;
        }

        private static void UpdateMission(Mission mission, int quantity)
        {
            mission.DispatchedQuantity += quantity;
            mission.Status = mission.QuantityRemainingToDispatch == 0
                ? MissionStatus.Completed
                : MissionStatus.Incomplete;
        }

        private async Task<IOperationResult<Mission>> CompletePickMissionAsync(Mission mission, int quantity)
        {
            if (mission.CompartmentId.HasValue == false
               || mission.ItemId.HasValue == false)
            {
                throw new InvalidOperationException();
            }

            if (quantity > mission.QuantityRemainingToDispatch)
            {
                return new BadRequestOperationResult<Mission>(
                    mission,
                    $"Requested quantity ({quantity}) cannot be greater than the remaining quantity to dispatch ({mission.QuantityRemainingToDispatch}).");
            }

            var now = DateTime.UtcNow;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var compartment = await this.compartmentProvider.GetByIdForStockUpdateAsync(mission.CompartmentId.Value);
                var loadingUnit = await this.loadingUnitProvider.GetByIdAsync(compartment.LoadingUnitId);
                var item = await this.itemProvider.GetByIdAsync(mission.ItemId.Value);

                UpdateCompartment(compartment, quantity, now);

                UpdateLoadingUnit(loadingUnit, now);

                UpdateItem(item, now);

                UpdateMission(mission, quantity);

                var result = await this.missionProvider.UpdateAsync(mission);
                await this.loadingUnitProvider.UpdateAsync(loadingUnit);
                await this.itemProvider.UpdateAsync(item);
                await this.compartmentProvider.UpdateAsync(compartment);

                if (mission.ItemListRowId.HasValue)
                {
                    var row = await this.rowProvider.GetByIdAsync(mission.ItemListRowId.Value);
                    await this.UpdateRowStatusAsync(row, now);
                }

                scope.Complete();

                return result;
            }
        }

        #endregion
    }
}
