using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class MissionExecutionSchedulerProvider : IMissionExecutionSchedulerProvider
    {
        #region Fields

        private readonly ICompartmentSchedulerProvider compartmentProvider;

        private readonly IItemSchedulerProvider itemProvider;

        private readonly ILogger<MissionExecutionSchedulerProvider> logger;

        private readonly IMissionSchedulerProvider missionProvider;

        private readonly IServiceScopeFactory scopeFactory;

        #endregion

        #region Constructors

        public MissionExecutionSchedulerProvider(
            IServiceScopeFactory scopeFactory,
            ICompartmentSchedulerProvider compartmentProvider,
            IMissionSchedulerProvider missionProvider,
            IItemSchedulerProvider itemProvider,
            IItemListSchedulerProvider itemListSchedulerProvider,
            ILogger<MissionExecutionSchedulerProvider> logger)
        {
            this.logger = logger;
            this.compartmentProvider = compartmentProvider;
            this.missionProvider = missionProvider;
            this.scopeFactory = scopeFactory;
            this.itemProvider = itemProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<Mission>> CompleteAsync(int id, int quantity)
        {
            if (quantity <= 0)
            {
                return new BadRequestOperationResult<Mission>(null, "Quantity cannot be negative or zero.");
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var mission = await this.missionProvider.GetByIdAsync(id);
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
                        result = await this.CompletePickMissionAsync(mission, quantity);
                        this.logger.LogDebug($"Completed mission id={mission.Id}");
                        break;

                    default:
                        return new BadRequestOperationResult<Mission>(null, "Only item pick operations are allowed.");
                }

                scope.Complete();

                return result;
            }
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
                return new BadRequestOperationResult<Mission>(mission);
            }

            mission.Status = MissionStatus.Executing;

            return await this.missionProvider.UpdateAsync(mission);
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

            await this.UpdateRowAsync(mission, now);

            var compartment = await this.UpdateCompartmentAsync(mission, quantity, now);
            await this.UpdateLoadingUnitAsync(compartment.LoadingUnitId, now);

            await this.UpdateItemAsync(mission, now);

            mission.DispatchedQuantity += quantity;
            mission.Status = mission.QuantityRemainingToDispatch == 0
                ? MissionStatus.Completed
                : MissionStatus.Incomplete;

            return await this.missionProvider.UpdateAsync(mission);
        }

        private async Task<StockUpdateCompartment> UpdateCompartmentAsync(Mission mission, int quantity, DateTime now)
        {
            var compartment = await this.compartmentProvider
               .GetByIdForStockUpdateAsync(mission.CompartmentId.Value);

            compartment.ReservedForPick -= quantity;
            compartment.Stock -= quantity;

            if (compartment.Stock == 0
                && compartment.IsItemPairingFixed == false)
            {
                compartment.ItemId = null;
            }

            compartment.LastPickDate = now;

            await this.compartmentProvider.UpdateAsync(compartment);

            return compartment;
        }

        private async Task UpdateItemAsync(Mission mission, DateTime now)
        {
            var item = await this.itemProvider.GetByIdAsync(mission.ItemId.Value);

            item.LastPickDate = now;

            await this.itemProvider.UpdateAsync(item);
        }

        private async Task UpdateLoadingUnitAsync(int loadingUnitId, DateTime now)
        {
            using (var scope = this.scopeFactory.CreateScope())
            {
                var loadingUnitProvider = scope.ServiceProvider.GetRequiredService<ILoadingUnitSchedulerProvider>();

                var loadingUnit = await loadingUnitProvider.GetByIdAsync(loadingUnitId);

                loadingUnit.LastPickDate = now;

                await loadingUnitProvider.UpdateAsync(loadingUnit);
            }
        }

        private async Task UpdateRowAsync(Mission mission, DateTime now)
        {
            if (mission.ItemListRowId.HasValue == false)
            {
                return;
            }

            using (var scope = this.scopeFactory.CreateScope())
            {
                var itemListRowProvider = scope.ServiceProvider.GetRequiredService<IItemListRowSchedulerProvider>();

                var listRow = await itemListRowProvider.GetByIdAsync(mission.ItemListRowId.Value);
                var currentStatus = listRow.Status;
                var involvedMissions = await this.missionProvider.GetByListRowIdAsync(mission.ItemListRowId.Value);

                var completeMissionsCount = involvedMissions.Count(m => m.Status == MissionStatus.Completed);
                var waitingMissionsCount = involvedMissions.Count(m => m.Status == MissionStatus.Waiting);
                var hasExecutingMissions = involvedMissions.Any(m => m.Status == MissionStatus.Executing);
                var hasErroredMissions = involvedMissions.Any(m => m.Status == MissionStatus.Error);
                var hasIncompleteMissions = involvedMissions.Any(m => m.Status == MissionStatus.Incomplete);

                if (completeMissionsCount == involvedMissions.Count())
                {
                    listRow.Status = ListRowStatus.Completed;
                    listRow.CompletionDate = now;
                }
                else if (waitingMissionsCount == involvedMissions.Count())
                {
                    listRow.Status = ListRowStatus.Waiting;
                }
                else if (hasErroredMissions)
                {
                    listRow.Status = ListRowStatus.Error;
                }
                else if (hasExecutingMissions)
                {
                    listRow.Status = ListRowStatus.Executing;
                    listRow.LastExecutionDate = now;
                }
                else if (hasIncompleteMissions)
                {
                    listRow.Status = ListRowStatus.Incomplete;
                }

                if (currentStatus != listRow.Status)
                {
                    await itemListRowProvider.UpdateAsync(listRow);
                }
            }
        }

        #endregion
    }
}
