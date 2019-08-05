using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemListRowExecutionProvider : BaseProvider, IItemListRowExecutionProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly ISchedulerRequestPickProvider schedulerRequestPickProvider;

        private readonly ISchedulerRequestPutProvider schedulerRequestPutProvider;

        private readonly ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider;

        #endregion

        #region Constructors

        public ItemListRowExecutionProvider(
            DatabaseContext dataContext,
            ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider,
            ISchedulerRequestPickProvider schedulerRequestPickProvider,
            ISchedulerRequestPutProvider schedulerRequestPutProvider,
            IBayProvider bayProvider,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.schedulerRequestSchedulerProvider = schedulerRequestSchedulerProvider;
            this.schedulerRequestPickProvider = schedulerRequestPickProvider;
            this.schedulerRequestPutProvider = schedulerRequestPutProvider;
            this.bayProvider = bayProvider;
        }

        #endregion

        #region Methods

        public static void SetPolicies(BasePolicyModel model)
        {
            model?.AddPolicy((model as IItemListRowExecutePolicy).ComputeExecutePolicy());
        }

        public async Task<ItemListRowOperation> GetByIdAsync(int id)
        {
            var result = await this.DataContext.ItemListRows
                .Select(r => new ItemListRowOperation
                {
                    Id = r.Id,
                    CompletionDate = r.CompletionDate,
                    ItemId = r.ItemId,
                    LastExecutionDate = r.LastExecutionDate,
                    Lot = r.Lot,
                    MaterialStatusId = r.MaterialStatusId,
                    PackageTypeId = r.PackageTypeId,
                    RegistrationNumber = r.RegistrationNumber,
                    RequestedQuantity = r.RequestedQuantity,
                    Sub1 = r.Sub1,
                    Sub2 = r.Sub2,
                    ListId = r.ItemListId,
                    OperationType = (ItemListType)r.ItemList.ItemListType,
                    Status = (ItemListRowStatus)r.Status,
                    DispatchedQuantity = r.DispatchedQuantity,
                    Priority = r.Priority,
                })
                .SingleAsync(i => i.Id == id);

            if (result != null)
            {
                SetPolicies(result);
            }

            return result;
        }

        public async Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> PrepareForExecutionAsync(
            int id,
            int areaId,
            int? bayId)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var row = await this.GetByIdAsync(id);
                if (row == null)
                {
                    return new NotFoundOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(
                        null,
                        string.Format(
                            Resources.Errors.UnableToExecuteTheListRowBecauseNoRowExists,
                            id));
                }

                var result = await this.ExecutionAsync(row, areaId, bayId, false);

                if (result.Success)
                {
                    scope.Complete();
                }

                return result;
            }
        }

        public async Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> PrepareForExecutionInListAsync(
            ItemListRowOperation row,
            int areaId,
            int? bayId,
            int? previousRowRequestPriority)
        {
            return await this.ExecutionAsync(row, areaId, bayId, true, previousRowRequestPriority);
        }

        public async Task<IOperationResult<ItemListRowOperation>> SuspendAsync(int id)
        {
            var row = await this.GetByIdAsync(id);
            if (row == null)
            {
                return new NotFoundOperationResult<ItemListRowOperation>(
                    null,
                    string.Format(
                        Resources.Errors.UnableToSuspendTheListRowBecauseNoRowExists,
                        id));
            }

            if (!row.CanExecuteOperation(nameof(ItemListRowPolicy.Suspend)))
            {
                return new BadRequestOperationResult<ItemListRowOperation>(
                    row.GetCanExecuteOperationReason(nameof(ItemListRowPolicy.Suspend)));
            }

            row.Status = ItemListRowStatus.Suspended;

            throw new System.NotImplementedException();
        }

        public async Task<IOperationResult<ItemListRowOperation>> UpdateAsync(ItemListRowOperation model)
        {
            var result = await this.UpdateAsync(
                model,
                this.DataContext.ItemListRows,
                this.DataContext,
                false);

            this.NotificationService.PushUpdate(model);
            this.NotificationService.PushUpdate(new ItemListOperation { Id = model.ListId }, model);
            this.NotificationService.PushUpdate(new Item { Id = model.ItemId }, model);

            return result;
        }

        private async Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> ExecutionAsync(
            ItemListRowOperation row,
            int areaId,
            int? bayId,
            bool executeAsPartOfList,
            int? previousRowRequestPriority = null)
        {
            if (!row.CanExecuteOperation(nameof(ItemListRowPolicy.Execute)))
            {
                return new BadRequestOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(
                    row.GetCanExecuteOperationReason(nameof(ItemListRowPolicy.Execute)));
            }

            var options = new ItemOptions
            {
                RunImmediately = false,
                BayId = bayId,
                AreaId = areaId,
                RequestedQuantity = row.RequestedQuantity,
                Lot = row.Lot,
                MaterialStatusId = row.MaterialStatusId,
                PackageTypeId = row.PackageTypeId,
                RegistrationNumber = row.RegistrationNumber,
                Sub1 = row.Sub1,
                Sub2 = row.Sub2,
            };

            IOperationResult<IEnumerable<ItemSchedulerRequest>> result;
            switch (row.OperationType)
            {
                case ItemListType.Pick:
                    {
                        result = await this.schedulerRequestPickProvider
                            .FullyQualifyPickRequestAsync(row.ItemId, options, row, previousRowRequestPriority);
                        break;
                    }

                case ItemListType.Put:
                    {
                        result = await this.schedulerRequestPutProvider
                            .FullyQualifyPutRequestAsync(row.ItemId, options, row, previousRowRequestPriority);
                        break;
                    }

                default:
                    return new BadRequestOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(
                        string.Format(
                            Resources.Errors.UnableToExecuteTheListRowBecauseOfTypeNotSupported,
                            row.Id,
                            row.OperationType));
            }

            if (!result.Success)
            {
                row.Status = ItemListRowStatus.Incomplete;
                await this.UpdateAsync(row);

                return new BadRequestOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(
                    string.Format(
                        Resources.Errors.UnableToExecuteTheListRowWithDescription,
                        row.Id,
                        result.Description),
                    result.Entity?.Cast<ItemListRowSchedulerRequest>());
            }

            System.Diagnostics.Debug.Assert(
                result.Entity.OfType<ItemListRowSchedulerRequest>().Count() == result.Entity.Count(),
                "The requests should be of type Row.");

            var rowRequests = result.Entity.Cast<ItemListRowSchedulerRequest>();

            row.Status = options.BayId.HasValue ? ItemListRowStatus.Ready : ItemListRowStatus.Waiting;

            var updateResult = await this.UpdateAsync(row);
            if (!updateResult.Success)
            {
                return new BadRequestOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(updateResult.Description);
            }

            if (!executeAsPartOfList && bayId.HasValue)
            {
                await this.bayProvider.UpdatePriorityAsync(bayId.Value, row.Priority);
            }

            var createResult = await this.schedulerRequestSchedulerProvider.CreateRangeAsync(rowRequests);
            if (!createResult.Success)
            {
                return new CreationErrorOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(createResult.Description);
            }

            return new SuccessOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(rowRequests);
        }

        #endregion
    }
}
