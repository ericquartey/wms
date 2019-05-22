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
    public class ItemListRowExecutionProvider : IItemListRowExecutionProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly DatabaseContext dataContext;

        private readonly ISchedulerRequestPickProvider schedulerRequestPickProvider;

        private readonly ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider;

        #endregion

        #region Constructors

        public ItemListRowExecutionProvider(
            DatabaseContext databaseContext,
            ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider,
            ISchedulerRequestPickProvider schedulerRequestPickProvider,
            IBayProvider bayProvider)
        {
            this.dataContext = databaseContext;
            this.schedulerRequestSchedulerProvider = schedulerRequestSchedulerProvider;
            this.schedulerRequestPickProvider = schedulerRequestPickProvider;
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
            var result = await this.dataContext.ItemListRows
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
            if (row.CanExecuteOperation(nameof(ItemListRowPolicy.Execute)) == false)
            {
                return new BadRequestOperationResult<ItemListRowSchedulerRequest>(
                           null,
                           row.GetCanExecuteOperationReason(nameof(ItemListRowPolicy.Execute)));
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
            await this.GetByIdAsync(id);
            throw new System.NotImplementedException();
        }

        public async Task<IOperationResult<ItemListRowOperation>> UpdateAsync(ItemListRowOperation model)
        {
            return await this.UpdateAsync(
                model,
                this.dataContext.ItemListRows,
                this.dataContext,
                false);
        }

        private async Task<IOperationResult<ItemListRowSchedulerRequest>> ExecutionAsync(
            ItemListRowOperation row,
            int areaId,
            int? bayId,
            bool executeAsPartOfList,
            int? previousRowRequestPriority = null)
        {
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

            var result = await this.schedulerRequestPickProvider
                .FullyQualifyPickRequestAsync(row.ItemId, options, row, previousRowRequestPriority);

            if (result.Entity is ItemListRowSchedulerRequest rowRequest)
            {
                row.Status = ItemListRowStatus.Waiting;
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

            row.Status = ItemListRowStatus.Incomplete;
            await this.UpdateAsync(row);

            return new BadRequestOperationResult<ItemListRowSchedulerRequest>(null);
        }

        #endregion
    }
}
