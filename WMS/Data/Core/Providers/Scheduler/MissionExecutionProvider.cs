using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Interfaces.Policies;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
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

        public async Task<IOperationResult<MissionExecution>> AbortItemAsync(int missionId)
        {
            var mission = await this.GetByIdAsync(missionId);
            if (mission == null)
            {
                return new NotFoundOperationResult<MissionExecution>(null, $"No mission with id '{missionId}' exists.");
            }

            if (!mission.CanExecuteOperation(nameof(MissionPolicy.Abort)))
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    mission.GetCanExecuteOperationReason(nameof(MissionPolicy.Abort)));
            }

            return await this.AbortItemMissionAsync(mission);
        }

        public async Task<IOperationResult<MissionExecution>> CompleteItemAsync(int id, double quantity)
        {
            if (quantity <= 0)
            {
                return new BadRequestOperationResult<MissionExecution>(
                    null,
                    $"Value '{quantity}' represents an invalid quantity. Dispatched mission quantity cannot be negative or zero.");
            }

            var mission = await this.GetByIdAsync(id);
            if (mission == null)
            {
                return new NotFoundOperationResult<MissionExecution>(null, $"No mission with id '{id}' exists.");
            }

            if (!mission.CanExecuteOperation(nameof(MissionPolicy.Complete)))
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    mission.GetCanExecuteOperationReason(nameof(MissionPolicy.Complete)));
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

            if (!mission.CanExecuteOperation(nameof(MissionPolicy.Complete)))
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    mission.GetCanExecuteOperationReason(nameof(MissionPolicy.Complete)));
            }

            IOperationResult<MissionExecution> result = null;
            switch (mission.Type)
            {
                case MissionType.Pick:
                    result = await this.CompleteLoadingUnitWithdrawMissionAsync(mission);
                    this.logger.LogDebug($"Completed mission id={mission.Id}");
                    break;

                default:
                    return new BadRequestOperationResult<MissionExecution>(null, "Only loading unit withdrawal operations are allowed.");
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

            if (!mission.CanExecuteOperation(nameof(MissionPolicy.Execute)))
            {
                return new BadRequestOperationResult<MissionExecution>(
                    mission,
                    mission.GetCanExecuteOperationReason(nameof(MissionPolicy.Execute)));
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
            var mission = await this.dataContext.Missions
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

            SetPolicies(mission);

            return mission;
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
                this.dataContext,
                checkForPolicies: false);
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

        private static void RemovePairingIfEmpty(CandidateCompartment compartment)
        {
            if (compartment.Stock.CompareTo(0) == 0
                && compartment.ReservedForPick.CompareTo(0) == 0
                && compartment.ReservedToPut.CompareTo(0) == 0)
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
        }

        private static void SetPolicies(BaseModel<int> model)
        {
            if (model is IMissionPolicy mission)
            {
                model.AddPolicy(mission.ComputeAbortPolicy());
                model.AddPolicy(mission.ComputeCompletePolicy());
                model.AddPolicy(mission.ComputeExecutePolicy());
            }
        }

        private static void UpdateCompartmentAfterPick(CandidateCompartment compartment, double quantity, DateTime now)
        {
            compartment.ReservedForPick -= quantity;
            compartment.Stock -= quantity;
            RemovePairingIfEmpty(compartment);

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

        private async Task<IOperationResult<MissionExecution>> AbortItemMissionAsync(MissionExecution mission)
        {
            System.Diagnostics.Debug.Assert(
                mission != null,
                $"The method argument {nameof(mission)} should not be null.");

            var compartment = await this.compartmentOperationProvider.GetByIdForStockUpdateAsync(mission.CompartmentId.Value);

            switch (mission.Type)
            {
                case MissionType.Pick:
                    compartment.ReservedForPick -= mission.RequestedQuantity;
                    break;

                case MissionType.Put:
                    compartment.ReservedToPut -= mission.RequestedQuantity;
                    RemovePairingIfEmpty(compartment);
                    break;

                default:
                    return new BadRequestOperationResult<MissionExecution>(
                        null,
                        $"Abortion is not supported for mission type '{mission.Type}'.");
            }

            mission.Status = MissionStatus.Incomplete;

            var compartmentUpdateResult = await this.compartmentOperationProvider.UpdateAsync(compartment);
            if (!compartmentUpdateResult.Success)
            {
                return new UnprocessableEntityOperationResult<MissionExecution>(compartmentUpdateResult.Description);
            }

            if (mission.ItemListRowId.HasValue)
            {
                var row = await this.rowExecutionProvider.GetByIdAsync(mission.ItemListRowId.Value);
                await this.UpdateRowStatusAsync(row, DateTime.UtcNow);
            }

            return await this.UpdateAsync(mission);
        }

        private async Task<IOperationResult<MissionExecution>> CompleteItemPickMissionAsync(MissionExecution mission, double quantity)
        {
            if (!mission.CompartmentId.HasValue)
            {
                return new UnprocessableEntityOperationResult<MissionExecution>(
                    "Unable to complete the specified mission because it has no associated compartment.");
            }

            if (!mission.ItemId.HasValue)
            {
                return new UnprocessableEntityOperationResult<MissionExecution>(
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
                if (compartment == null)
                {
                    return new UnprocessableEntityOperationResult<MissionExecution>(
                        "Unable to complete the specified mission. The associated compartment could not be retrieved.");
                }

                var loadingUnit = await this.loadingUnitProvider.GetByIdForExecutionAsync(compartment.LoadingUnitId);
                if (loadingUnit == null)
                {
                    return new UnprocessableEntityOperationResult<MissionExecution>(
                        "Unable to complete the specified mission. The associated loading unit could not be retrieved.");
                }

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
                    row.DispatchedQuantity += mission.DispatchedQuantity;
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
                return new UnprocessableEntityOperationResult<MissionExecution>(
                    "Unable to complete the specified mission. The mission has no associated compartment.");
            }

            if (mission.ItemId.HasValue == false)
            {
                return new UnprocessableEntityOperationResult<MissionExecution>(
                    "Unable to complete the specified mission. The mission has no associated item.");
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
                if (compartment == null)
                {
                    return new UnprocessableEntityOperationResult<MissionExecution>(
                        "Unable to complete the specified mission. The associated compartment could not be retrieved.");
                }

                var loadingUnit = await this.loadingUnitProvider.GetByIdForExecutionAsync(compartment.LoadingUnitId);
                if (loadingUnit == null)
                {
                    return new UnprocessableEntityOperationResult<MissionExecution>(
                        "Unable to complete the specified mission. The associated loading unit could not be retrieved.");
                }

                var item = await this.itemProvider.GetByIdForExecutionAsync(mission.ItemId.Value);

                await this.UpdateCompartmentAfterPutAsync(compartment, quantity, now);

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
                    row.DispatchedQuantity += mission.DispatchedQuantity;
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
            CandidateCompartment compartment,
            double quantity,
            DateTime now)
        {
            compartment.ReservedToPut -= quantity;
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
                         CompartmentTypeId = compartment.CompartmentTypeId,
                         MaxCapacity = compartment.Stock
                     });
            }
        }

        #endregion
    }
}
