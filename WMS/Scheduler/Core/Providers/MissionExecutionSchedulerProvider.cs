using System;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class MissionExecutionSchedulerProvider : IMissionExecutionSchedulerProvider
    {
        #region Fields

        private readonly ICompartmentSchedulerProvider compartmentProvider;

        private readonly DatabaseContext databaseContext;

        private readonly IItemSchedulerProvider itemProvider;

        private readonly ILogger<MissionExecutionSchedulerProvider> logger;

        private readonly IMissionSchedulerProvider missionProvider;

        private readonly IServiceScopeFactory scopeFactory;

        #endregion

        #region Constructors

        public MissionExecutionSchedulerProvider(
            IServiceScopeFactory scopeFactory,
            DatabaseContext databaseContext,
            ICompartmentSchedulerProvider compartmentProvider,
            IMissionSchedulerProvider missionProvider,
            IItemSchedulerProvider itemProvider,
            ILogger<MissionExecutionSchedulerProvider> logger)
        {
            this.logger = logger;
            this.compartmentProvider = compartmentProvider;
            this.missionProvider = missionProvider;
            this.scopeFactory = scopeFactory;
            this.itemProvider = itemProvider;
            this.databaseContext = databaseContext;
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

            return await this.missionProvider.UpdateAsync(mission);
        }

        public async Task<IOperationResult<T>> UpdateEntityAsync<T, TDataModel>(T model, DbSet<TDataModel> dbSet)
            where T : Model
            where TDataModel : class
        {
            if (model == null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            var existingModel = dbSet.Find(model.Id);
            this.databaseContext.Entry(existingModel).CurrentValues.SetValues(model);

            await this.databaseContext.SaveChangesAsync();

            return new SuccessOperationResult<T>(model);
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

            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var rowProvider = serviceScope.ServiceProvider.GetRequiredService<IItemListRowSchedulerProvider>();
                var loadingUnitProvider = serviceScope.ServiceProvider.GetRequiredService<ILoadingUnitSchedulerProvider>();

                var compartment = await this.compartmentProvider.GetByIdForStockUpdateAsync(mission.CompartmentId.Value);
                var loadingUnit = await loadingUnitProvider.GetByIdAsync(compartment.LoadingUnitId);
                var item = await this.itemProvider.GetByIdAsync(mission.ItemId.Value);

                UpdateCompartment(compartment, quantity, now);

                UpdateLoadingUnit(loadingUnit, now);

                UpdateItem(item, now);

                UpdateMission(mission, quantity);

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (mission.ItemListRowId.HasValue)
                    {
                        var row = await rowProvider.GetByIdAsync(mission.ItemListRowId.Value);
                        await rowProvider.UpdateRowStatusAsync(row, now);
                    }

                    var result = await this.UpdateEntityAsync(mission, this.databaseContext.Missions);
                    await this.UpdateEntityAsync(loadingUnit, this.databaseContext.LoadingUnits);
                    await this.UpdateEntityAsync(item, this.databaseContext.Items);
                    await this.UpdateEntityAsync(compartment, this.databaseContext.Compartments);

                    scope.Complete();

                    return result;
                }
            }
        }

        #endregion
    }
}
