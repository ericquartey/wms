using System.Linq;
using System.Threading.Tasks;
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

        public static void SetPolicies(BaseModel<int> model)
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
                    Priority = r.Priority
                })
                .SingleAsync(i => i.Id == id);

            if (result != null)
            {
                SetPolicies(result);
            }

            return result;
        }

        public async Task<IOperationResult<ItemListRowSchedulerRequest>> PrepareForExecutionAsync(
            int id,
            int areaId,
            int? bayId)
        {
            var row = await this.GetByIdAsync(id);
            if (row == null)
            {
                return new NotFoundOperationResult<ItemListRowSchedulerRequest>(
                    null,
                    $"Unable to execute the row because no row with id={id} exists.");
            }

            return await this.ExecutionAsync(row, areaId, bayId, false);
        }

        public async Task<IOperationResult<ItemListRowSchedulerRequest>> PrepareForExecutionInListAsync(
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
                    $"Unable to execute the row because no row with id={id} exists.");
            }

            if (!row.CanExecuteOperation(nameof(ItemListRowPolicy.Suspend)))
            {
                return new BadRequestOperationResult<ItemListRowOperation>(
                    null,
                    row.GetCanExecuteOperationReason(nameof(ItemListRowPolicy.Suspend)));
            }

            row.Status = ItemListRowStatus.Suspended;

            throw new System.NotImplementedException();
        }

        public async Task<IOperationResult<ItemListRowOperation>> UpdateAsync(ItemListRowOperation model)
        {
            return await this.UpdateAsync(
                model,
                this.DataContext.ItemListRows,
                this.DataContext,
                false);
        }

        private async Task<IOperationResult<ItemListRowSchedulerRequest>> ExecutionAsync(
            ItemListRowOperation row,
            int areaId,
            int? bayId,
            bool executeAsPartOfList,
            int? previousRowRequestPriority = null)
        {
            if (!row.CanExecuteOperation(nameof(ItemListRowPolicy.Execute)))
            {
                return new BadRequestOperationResult<ItemListRowSchedulerRequest>(
                    null,
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

            IOperationResult<ItemSchedulerRequest> result;
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
                    return new BadRequestOperationResult<ItemListRowSchedulerRequest>(
                        null,
                        $"Unable to execute the list row (id={row.Id}). The rows of type '{row.OperationType}' are not supported.");
            }

            if (!result.Success)
            {
                row.Status = ItemListRowStatus.Incomplete;
                await this.UpdateAsync(row);

                return new BadRequestOperationResult<ItemListRowSchedulerRequest>(
                    result.Entity as ItemListRowSchedulerRequest,
                    $"Unable to execute the list row (id={row.Id}). {result.Description}.");
            }

            System.Diagnostics.Debug.Assert(
                result.Entity is ItemListRowSchedulerRequest,
                "The request should be of type Row.");

            var rowRequest = result.Entity as ItemListRowSchedulerRequest;

            row.Status = options.BayId.HasValue ? ItemListRowStatus.Ready : ItemListRowStatus.Waiting;

            var updateResult = await this.UpdateAsync(row);
            if (!updateResult.Success)
            {
                return new BadRequestOperationResult<ItemListRowSchedulerRequest>(null, updateResult.Description);
            }

            if (!executeAsPartOfList)
            {
                if (bayId.HasValue)
                {
                    await this.bayProvider.UpdatePriorityAsync(bayId.Value, row.Priority);
                }

                await this.schedulerRequestSchedulerProvider.CreateAsync(rowRequest);
            }

            return new SuccessOperationResult<ItemListRowSchedulerRequest>(rowRequest);
        }

        #endregion
    }
}
