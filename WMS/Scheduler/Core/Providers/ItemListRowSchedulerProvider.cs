using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class ItemListRowSchedulerProvider : IItemListRowSchedulerProvider
    {
        #region Fields

        private readonly IBaySchedulerProvider bayProvider;

        private readonly DatabaseContext databaseContext;

        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion

        #region Constructors

        public ItemListRowSchedulerProvider(
            DatabaseContext databaseContext,
            ISchedulerRequestProvider schedulerRequestProvider,
            IBaySchedulerProvider bayProvider)
        {
            this.databaseContext = databaseContext;
            this.schedulerRequestProvider = schedulerRequestProvider;
            this.bayProvider = bayProvider;
        }

        #endregion

        #region Methods

        public async Task<ItemListRow> GetByIdAsync(int id)
        {
            return await this.databaseContext.ItemListRows
                .Select(r => new ItemListRow
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
            ItemListRow row,
            int areaId,
            int? bayId)
        {
            return await this.ExecutionAsync(row, areaId, bayId, true);
        }

        public async Task<IOperationResult<ItemListRow>> SuspendAsync(int id)
        {
            await this.GetByIdAsync(id);
            throw new System.NotImplementedException();
        }

        public async Task<IOperationResult<ItemListRow>> UpdateAsync(ItemListRow model)
        {
            if (model == null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            var existingRow = this.databaseContext.ItemListRows.Find(model.Id);
            this.databaseContext.Entry(existingRow).CurrentValues.SetValues(model);

            await this.databaseContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemListRow>(model);
        }

        private async Task<IOperationResult<ItemListRowSchedulerRequest>> ExecutionAsync(ItemListRow row, int areaId, int? bayId, bool executeAsPartOfList)
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

            var qualifiedRequest = await this.schedulerRequestProvider
                .FullyQualifyWithdrawalRequestAsync(row.ItemId, options, row);

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

                    await this.schedulerRequestProvider.CreateAsync(rowRequest);
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
