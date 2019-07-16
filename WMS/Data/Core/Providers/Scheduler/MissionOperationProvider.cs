using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MissionOperationProvider : BaseProvider, IMissionOperationProvider
    {
        #region Fields

        private readonly ICompartmentOperationProvider compartmentOperationProvider;

        private readonly IItemCompartmentTypeProvider itemCompartmentTypeProvider;

        private readonly IItemProvider itemProvider;

        private readonly IItemListProvider itemListProvider;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        private readonly ILogger<MissionOperationProvider> logger;

        private readonly IMapper mapper;

        private readonly IMissionProvider missionProvider;

        private readonly IItemListRowExecutionProvider rowExecutionProvider;

        #endregion

        #region Constructors

        public MissionOperationProvider(
            DatabaseContext dataContext,
            INotificationService notificationService,
            IMapper mapper,
            ICompartmentOperationProvider compartmentOperationProvider,
            IItemListRowExecutionProvider rowExecutionProvider,
            IItemProvider itemProvider,
            IMissionProvider missionProvider,
            ILoadingUnitProvider loadingUnitProvider,
            IItemCompartmentTypeProvider itemCompartmentTypeProvider,
            IItemListProvider itemListProvider,
            ILogger<MissionOperationProvider> logger)
        : base(dataContext, notificationService)
        {
            this.mapper = mapper;
            this.logger = logger;
            this.compartmentOperationProvider = compartmentOperationProvider;
            this.itemProvider = itemProvider;
            this.missionProvider = missionProvider;
            this.rowExecutionProvider = rowExecutionProvider;
            this.loadingUnitProvider = loadingUnitProvider;
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
            this.itemListProvider = itemListProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<MissionOperation>> AbortAsync(int id)
        {
            var operation = await this.GetByIdAsync(id);
            if (operation == null)
            {
                return new NotFoundOperationResult<MissionOperation>(
                    null,
                    string.Format(
                        Resources.Errors.NoMissionWithIdExists,
                        id));
            }

            if (!operation.CanExecuteOperation(nameof(MissionOperationPolicy.Abort)))
            {
                return new BadRequestOperationResult<MissionOperation>(
                    operation.GetCanExecuteOperationReason(nameof(MissionOperationPolicy.Abort)), operation);
            }

            return await this.AbortAsync(operation);
        }

        public async Task<IOperationResult<MissionOperation>> CompleteAsync(int id, double quantity)
        {
            if (quantity <= 0)
            {
                return new BadRequestOperationResult<MissionOperation>(
                    string.Format(
                        Resources.Errors.ValueRepresentsAnInvalidQuantity,
                        quantity));
            }

            var operation = await this.GetByIdAsync(id);
            if (operation == null)
            {
                return new NotFoundOperationResult<MissionOperation>(
                    null,
                    string.Format(
                        Resources.Errors.NoMissionWithIdExists,
                        id));
            }

            if (!operation.CanExecuteOperation(nameof(MissionOperationPolicy.Complete)))
            {
                return new BadRequestOperationResult<MissionOperation>(
                    operation.GetCanExecuteOperationReason(nameof(MissionOperationPolicy.Complete)),
                    operation);
            }

            IOperationResult<MissionOperation> result = null;
            switch (operation.Type)
            {
                case MissionOperationType.Pick:
                    result = await this.CompletePickOperationAsync(operation, quantity);
                    this.logger.LogDebug($"Completed pick operation id={operation.Id}");
                    break;

                case MissionOperationType.Put:
                    result = await this.CompletePutOperationAsync(operation, quantity);
                    this.logger.LogDebug($"Completed put operation id={operation.Id}");
                    break;

                default:
                    return new BadRequestOperationResult<MissionOperation>(
                        string.Format(
                            Resources.Errors.CompletionIsNotSupportedForMissionType,
                            operation.Type));
            }

            return result;
        }

        public async Task<IOperationResult<IEnumerable<MissionOperation>>> CreateRangeAsync(
            IEnumerable<MissionOperation> models)
        {
            if (models == null)
            {
                throw new ArgumentNullException(nameof(models));
            }

            await this.DataContext.MissionOperations.AddRangeAsync(
                this.mapper.Map<IEnumerable<Common.DataModels.MissionOperation>>(models));

            foreach (var model in models)
            {
                this.NotificationService.PushCreate(model);
            }

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount <= 0)
            {
                return new CreationErrorOperationResult<IEnumerable<MissionOperation>>();
            }

            return new SuccessOperationResult<IEnumerable<MissionOperation>>(models);
        }

        public async Task<IOperationResult<MissionOperation>> ExecuteAsync(int id)
        {
            var operation = await this.GetByIdAsync(id);
            if (operation == null)
            {
                return new NotFoundOperationResult<MissionOperation>();
            }

            if (!operation.CanExecuteOperation(nameof(MissionOperationPolicy.Execute)))
            {
                return new BadRequestOperationResult<MissionOperation>(
                    operation.GetCanExecuteOperationReason(nameof(MissionOperationPolicy.Execute)),
                    operation);
            }

            operation.Status = MissionOperationStatus.Executing;
            var result = await this.UpdateAsync(operation);

            await this.UpdateMissionStatusAsync(operation);

            await this.UpdateRowStatusAsync(operation, DateTime.UtcNow);

            return result;
        }

        public async Task<IEnumerable<MissionOperation>> GetAllAsync(int skip, int take, IEnumerable<SortOption> orderBySortOptions = null, string whereString = null, string searchString = null)
        {
            var operations = await this.DataContext.MissionOperations
                .ProjectTo<MissionOperation>(this.mapper.ConfigurationProvider)
                .ToArrayAsync<MissionOperation, Common.DataModels.MissionOperation>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    null);

            SetPolicies(operations);

            return operations;
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            return await this.DataContext.MissionOperations
                .ProjectTo<MissionOperation>(this.mapper.ConfigurationProvider)
                .CountAsync<MissionOperation, Common.DataModels.MissionOperation>(
                    whereString,
                    null);
        }

        public async Task<MissionOperation> GetByIdAsync(int id)
        {
            var operation = await this.DataContext.MissionOperations
                .ProjectTo<MissionOperation>(this.mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(o => o.Id == id);

            SetPolicies(operation);

            return operation;
        }

        public async Task<IEnumerable<MissionOperation>> GetByListRowIdAsync(int listRowId)
        {
            return await this.DataContext.MissionOperations
                .Where(o => o.ItemListRowId == listRowId)
                .ProjectTo<MissionOperation>(this.mapper.ConfigurationProvider)
                .ToArrayAsync();
        }

        public async Task<IOperationResult<MissionOperation>> UpdateAsync(MissionOperation model)
        {
            if (model == null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            var result = await this.UpdateAsync(
                model,
                this.DataContext.MissionOperations,
                this.DataContext,
                checkForPolicies: false);

            this.NotificationService.PushUpdate(model);
            this.NotificationService.PushUpdate(new Item { Id = model.ItemId });
            this.NotificationService.PushUpdate(new Compartment { Id = model.CompartmentId });

            return result;
        }

        public async Task UpdateMissionStatusAsync(MissionOperation operation)
        {
            var mission = await this.missionProvider.GetByIdAsync(operation.MissionId);

            mission.Status = Mission.GetStatus(
                mission.OperationsCount,
                mission.NewOperationsCount,
                mission.ExecutingOperationsCount,
                mission.CompletedOperationsCount,
                mission.IncompleteOperationsCount,
                mission.ErrorOperationsCount);

            await this.missionProvider.UpdateAsync(mission);
        }

        public async Task UpdateRowStatusAsync(MissionOperation operation, DateTime now)
        {
            if (!operation.ItemListRowId.HasValue)
            {
                return;
            }

            var row = await this.rowExecutionProvider.GetByIdAsync(operation.ItemListRowId.Value);
            row.DispatchedQuantity += operation.DispatchedQuantity;

            var involvedOperations = await this.GetByListRowIdAsync(row.Id);

            var completeOperationsCount = involvedOperations.Count(o => o.Status == MissionOperationStatus.Completed);
            var hasExecutingOperations = involvedOperations.Any(o => o.Status == MissionOperationStatus.Executing);
            var hasErroredOperations = involvedOperations.Any(o => o.Status == MissionOperationStatus.Error);
            var hasIncompleteOperations = involvedOperations.Any(o => o.Status == MissionOperationStatus.Incomplete);

            if (!involvedOperations.Any())
            {
                row.Status = ItemListRowStatus.New;
            }
            else if (completeOperationsCount == involvedOperations.Count()
                && involvedOperations.Sum(o => o.DispatchedQuantity).CompareTo(row.RequestedQuantity) == 0)
            {
                row.Status = ItemListRowStatus.Completed;
                row.CompletionDate = now;
            }
            else if (hasErroredOperations)
            {
                row.Status = ItemListRowStatus.Error;
            }
            else if (hasExecutingOperations)
            {
                row.Status = ItemListRowStatus.Executing;
                row.LastExecutionDate = now;
            }
            else if (hasIncompleteOperations)
            {
                row.Status = ItemListRowStatus.Incomplete;
            }

            await this.rowExecutionProvider.UpdateAsync(row);

            var list = await this.itemListProvider.GetByIdAsync(operation.ItemListId.Value);
            if (list.Status == ItemListStatus.Completed)
            {
                list.ExecutionEndDate = now;
            }

            if (hasExecutingOperations)
            {
                list.FirstExecutionDate = list.FirstExecutionDate ?? now;
            }

            await this.itemListProvider.UpdateAsync(list);
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

        private static void SetPolicies(IEnumerable<MissionOperation> operations)
        {
            foreach (var operation in operations)
            {
                SetPolicies(operation);
            }
        }

        private static void SetPolicies(BaseModel<int> operation)
        {
            if (operation is IMissionOperationPolicy operationPolicyDescriptor)
            {
                operation.AddPolicy(operationPolicyDescriptor.ComputeCompletePolicy());
                operation.AddPolicy(operationPolicyDescriptor.ComputeAbortPolicy());
                operation.AddPolicy(operationPolicyDescriptor.ComputeExecutePolicy());
            }
        }

        private static void UpdateCompartmentAfterPick(CandidateCompartment compartment, double quantity, DateTime now)
        {
            compartment.ReservedForPick -= quantity;
            compartment.Stock -= quantity;
            compartment.PickMissionOperationCount++;
            RemovePairingIfEmpty(compartment);

            compartment.LastPickDate = now;
        }

        private async Task<IOperationResult<MissionOperation>> AbortAsync(MissionOperation operation)
        {
            System.Diagnostics.Debug.Assert(
                operation != null,
                $"The method argument {nameof(operation)} should not be null.");

            var compartment = await this.compartmentOperationProvider
                .GetByIdForStockUpdateAsync(operation.CompartmentId);

            switch (operation.Type)
            {
                case MissionOperationType.Pick:
                    compartment.ReservedForPick -= operation.RequestedQuantity;
                    break;

                case MissionOperationType.Put:
                    compartment.ReservedToPut -= operation.RequestedQuantity;
                    RemovePairingIfEmpty(compartment);
                    break;

                default:
                    return new BadRequestOperationResult<MissionOperation>(
                        string.Format(
                            Resources.Errors.AbortionIsNotSupportedForMissionType,
                            operation.Type));
            }

            operation.Status = MissionOperationStatus.Incomplete;
            var updateResult = await this.UpdateAsync(operation);

            await this.UpdateMissionStatusAsync(operation);

            var compartmentUpdateResult = await this.compartmentOperationProvider.UpdateAsync(compartment);
            if (!compartmentUpdateResult.Success)
            {
                return new UnprocessableEntityOperationResult<MissionOperation>(
                    compartmentUpdateResult.Description);
            }

            await this.UpdateRowStatusAsync(operation, DateTime.UtcNow);

            return updateResult;
        }

        private async Task<IOperationResult<MissionOperation>> CompletePickOperationAsync(MissionOperation operation, double quantity)
        {
            if (quantity > operation.QuantityRemainingToDispatch)
            {
                return new BadRequestOperationResult<MissionOperation>(
                    string.Format(
                        Resources.Errors.ActualPickedQuantityCannotBeGreaterThanTheRemainingQuantityToDispatch,
                        quantity,
                        operation.QuantityRemainingToDispatch),
                    operation);
            }

            if (quantity <= 0)
            {
                return new BadRequestOperationResult<MissionOperation>(
                    string.Format(
                        Resources.Errors.UnableToCompleteTheSpecifiedMissionBecauseOfActualPutQuantity,
                        operation),
                    operation);
            }

            var now = DateTime.UtcNow;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var compartment = await this.compartmentOperationProvider
                    .GetByIdForStockUpdateAsync(operation.CompartmentId);
                if (compartment == null)
                {
                    return new UnprocessableEntityOperationResult<MissionOperation>(
                        Resources.Errors.UnableToCompleteTheSpecifiedMissionBecauseOfAssociatedCompartmentNotRetrieved);
                }

                var loadingUnit = await this.loadingUnitProvider
                    .GetByIdForExecutionAsync(compartment.LoadingUnitId);
                if (loadingUnit == null)
                {
                    return new UnprocessableEntityOperationResult<MissionOperation>(
                        Resources.Errors.UnableToCompleteTheSpecifiedMissionBecauseOfAssociatedLoadingUnitNotRetrieved);
                }

                var item = await this.itemProvider.GetByIdForExecutionAsync(operation.ItemId);

                UpdateCompartmentAfterPick(compartment, quantity, now);

                loadingUnit.LastPickDate = now;

                item.LastPickDate = now;

                operation.DispatchedQuantity += quantity;
                operation.Status = operation.QuantityRemainingToDispatch.Equals(0)
                    ? MissionOperationStatus.Completed
                    : MissionOperationStatus.Incomplete;
                var result = await this.UpdateAsync(operation);

                await this.UpdateMissionStatusAsync(operation);

                await this.loadingUnitProvider.UpdateAsync(loadingUnit);
                await this.itemProvider.UpdateAsync(item);
                await this.compartmentOperationProvider.UpdateAsync(compartment);
                await this.UpdateRowStatusAsync(operation, now);

                scope.Complete();

                return result;
            }
        }

        private async Task<IOperationResult<MissionOperation>> CompletePutOperationAsync(
            MissionOperation operation,
            double quantity)
        {
            if (quantity <= 0)
            {
                return new BadRequestOperationResult<MissionOperation>(
                    string.Format(
                        Resources.Errors.UnableToCompleteTheSpecifiedMissionBecauseOfActualPutQuantity,
                        quantity),
                    operation);
            }

            var now = DateTime.UtcNow;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var compartment = await this.compartmentOperationProvider
                    .GetByIdForStockUpdateAsync(operation.CompartmentId);
                if (compartment == null)
                {
                    return new UnprocessableEntityOperationResult<MissionOperation>(
                        Resources.Errors.UnableToCompleteTheSpecifiedMissionBecauseOfAssociatedCompartmentNotRetrieved);
                }

                var loadingUnit = await this.loadingUnitProvider
                    .GetByIdForExecutionAsync(compartment.LoadingUnitId);
                if (loadingUnit == null)
                {
                    return new UnprocessableEntityOperationResult<MissionOperation>(
                        Resources.Errors.UnableToCompleteTheSpecifiedMissionBecauseOfAssociatedLoadingUnitNotRetrieved);
                }

                var item = await this.itemProvider.GetByIdForExecutionAsync(operation.ItemId);

                await this.UpdateCompartmentAfterPutAsync(compartment, quantity, now);

                loadingUnit.LastPutDate = now;

                item.LastPutDate = now;

                operation.DispatchedQuantity += quantity;
                operation.Status = operation.QuantityRemainingToDispatch.Equals(0)
                    ? MissionOperationStatus.Completed
                    : MissionOperationStatus.Incomplete;

                var result = await this.UpdateAsync(operation);

                await this.loadingUnitProvider.UpdateAsync(loadingUnit);

                await this.itemProvider.UpdateAsync(item);

                await this.compartmentOperationProvider.UpdateAsync(compartment);

                await this.UpdateMissionStatusAsync(operation);

                await this.UpdateRowStatusAsync(operation, now);

                scope.Complete();

                return result;
            }
        }

        private async Task UpdateCompartmentAfterPutAsync(
            CandidateCompartment compartment,
            double quantity,
            DateTime now)
        {
            compartment.ReservedToPut -= Math.Min(quantity, compartment.ReservedToPut);
            compartment.Stock += quantity;
            compartment.LastPutDate = now;
            compartment.PutMissionOperationCount++;

            if (compartment.MaxCapacity.HasValue
                &&
                compartment.Stock > compartment.MaxCapacity.Value)
            {
                await this.itemCompartmentTypeProvider.UpdateAsync(
                     new ItemCompartmentType
                     {
                         ItemId = compartment.ItemId.Value,
                         CompartmentTypeId = compartment.CompartmentTypeId,
                         MaxCapacity = compartment.Stock,
                     });
            }
        }

        #endregion
    }
}
