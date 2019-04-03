using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class ItemListSchedulerProvider : IItemListSchedulerProvider
    {
        #region Fields

        private readonly IBaySchedulerProvider bayProvider;

        private readonly DatabaseContext databaseContext;

        private readonly IItemListRowSchedulerProvider rowProvider;

        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion

        #region Constructors

        public ItemListSchedulerProvider(
            DatabaseContext databaseContext,
            IItemListRowSchedulerProvider itemListRowSchedulerProvider,
            IBaySchedulerProvider bayProvider,
            ISchedulerRequestProvider schedulerRequestProvider)
        {
            this.databaseContext = databaseContext;
            this.rowProvider = itemListRowSchedulerProvider;
            this.schedulerRequestProvider = schedulerRequestProvider;
            this.bayProvider = bayProvider;
        }

        #endregion

        #region Methods

        public async Task<ItemList> GetByIdAsync(int id)
        {
            return await this.databaseContext.ItemLists
                .Include(l => l.ItemListRows)
                .Select(i => new ItemList
                {
                    Id = i.Id,
                    Code = i.Code,
                    CompletedRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Completed),
                    ExecutingRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Executing),
                    IncompleteRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Incomplete),
                    NewRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.New),
                    SuspendedRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Suspended),
                    TotalRowsCount = i.ItemListRows.Count(),
                    WaitingRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Waiting),
                    Rows = i.ItemListRows.Select(r => new ItemListRow
                    {
                        Id = r.Id,
                        ItemId = r.ItemId,
                        ListId = r.ItemListId,
                        Lot = r.Lot,
                        Priority = r.Priority,
                        MaterialStatusId = r.MaterialStatusId,
                        PackageTypeId = r.PackageTypeId,
                        RegistrationNumber = r.RegistrationNumber,
                        RequestedQuantity = r.RequestedQuantity,
                        Status = (ItemListRowStatus)r.Status,
                        Sub1 = r.Sub1,
                        Sub2 = r.Sub2,
                    })
                })
                .SingleOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IOperationResult<IEnumerable<SchedulerRequest>>> PrepareForExecutionAsync(int id, int areaId, int? bayId)
        {
            IEnumerable<SchedulerRequest> requests = null;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var list = await this.GetByIdAsync(id);

                if (list.Status != ListStatus.New)
                {
                    if (list.Status == ListStatus.Waiting && bayId.HasValue == false)
                    {
                        return new BadRequestOperationResult<IEnumerable<SchedulerRequest>>(
                            null,
                            "Cannot execute the list because no bay was specified.");
                    }
                    else if (list.Status != ListStatus.Waiting)
                    {
                        return new BadRequestOperationResult<IEnumerable<SchedulerRequest>>(
                            null,
                            $"Cannot execute the list bacause its current state is {list.Status}.");
                    }
                }

                requests = await this.BuildRequestsAsync(list, areaId, bayId);

                await this.UpdateAsync(list);
                await this.schedulerRequestProvider.CreateRangeAsync(requests);
                if (bayId.HasValue)
                {
                    await this.bayProvider.UpdatePriorityAsync(bayId.Value, list.Rows.Max(r => r.Priority));
                }

                scope.Complete();

                return new SuccessOperationResult<IEnumerable<SchedulerRequest>>(requests);
            }
        }

        public async Task<IOperationResult<ItemList>> SuspendAsync(int id)
        {
            await this.GetByIdAsync(id);
            throw new NotImplementedException();
        }

        public async Task<IOperationResult<ItemList>> UpdateAsync(ItemList model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingList = this.databaseContext.ItemLists.Find(model.Id);
            this.databaseContext.Entry(existingList).CurrentValues.SetValues(model);

            await this.databaseContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemList>(model);
        }

        private async Task<IEnumerable<SchedulerRequest>> BuildRequestsAsync(ItemList list, int areaId, int? bayId)
        {
            var requests = new List<SchedulerRequest>(list.Rows.Count());

            foreach (var row in list.Rows)
            {
                var result = await this.rowProvider.PrepareForExecutionInListAsync(row, areaId, bayId);

                if (result.Success)
                {
                    requests.Add(result.Entity);
                }
            }

            return requests;
        }

        #endregion
    }
}
