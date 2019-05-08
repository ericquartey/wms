using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core
{
    public class ItemListRowExecutionProvider : IItemListRowExecutionProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly DatabaseContext dataContext;

        private readonly ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider;

        #endregion

        #region Constructors

        public ItemListRowExecutionProvider(
            DatabaseContext databaseContext,
            ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider,
            IBayProvider bayProvider)
        {
            this.dataContext = databaseContext;
            this.schedulerRequestSchedulerProvider = schedulerRequestSchedulerProvider;
            this.bayProvider = bayProvider;
        }

        #endregion

        #region Methods

        public async Task<ItemListRowOperation> GetByIdAsync(int id)
        {
            return await this.dataContext.ItemListRows
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
        }

        public async Task<IOperationResult<ItemListRowSchedulerRequest>> PrepareForExecutionAsync(
            int id,
            int areaId,
            int? bayId)
        {
            var row = await this.GetByIdAsync(id);

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
            if (model == null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            var existingRow = this.dataContext.ItemListRows.Find(model.Id);
            this.dataContext.Entry(existingRow).CurrentValues.SetValues(model);

            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemListRowOperation>(model);
        }

        private async Task<IOperationResult<ItemListRowSchedulerRequest>> ExecutionAsync(
            ItemListRowOperation row,
            int areaId,
            int? bayId,
            bool executeAsPartOfList,
            int? previousRowRequestPriority = null)
        {
            var options = new ItemWithdrawOptions
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

            var qualifiedRequest = await this.schedulerRequestSchedulerProvider
                .FullyQualifyWithdrawalRequestAsync(row.ItemId, options, row, previousRowRequestPriority);

            if (qualifiedRequest is ItemListRowSchedulerRequest rowRequest)
            {
                row.Status = ItemListRowStatus.Waiting;

                await this.UpdateAsync(row);

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
            else
            {
                return new BadRequestOperationResult<ItemListRowSchedulerRequest>(null);
            }
        }

        #endregion
    }
}
