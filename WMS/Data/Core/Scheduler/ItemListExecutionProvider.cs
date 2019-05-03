using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemListExecutionProvider : IItemListExecutionProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly DatabaseContext databaseContext;

        private readonly IItemListRowExecutionProvider rowExecutionProvider;

        private readonly ISchedulerRequestExecutionProvider schedulerRequestExecutionProvider;

        #endregion

        #region Constructors

        public ItemListExecutionProvider(
            DatabaseContext databaseContext,
            IItemListRowExecutionProvider rowExecutionProvider,
            IBayProvider bayProvider,
            ISchedulerRequestExecutionProvider schedulerRequestExecutionProvider)
        {
            this.databaseContext = databaseContext;
            this.rowExecutionProvider = rowExecutionProvider;
            this.schedulerRequestExecutionProvider = schedulerRequestExecutionProvider;
            this.bayProvider = bayProvider;
        }

        #endregion

        #region Methods

        public async Task<ItemListExecution> GetByIdAsync(int id)
        {
            return await this.databaseContext.ItemLists
                .Include(l => l.ItemListRows)
                .Select(i => new ItemListExecution
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
                    Rows = i.ItemListRows.Select(r => new ItemListRowExecution
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

        public async Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> PrepareForExecutionAsync(int id, int areaId, int? bayId)
        {
            IEnumerable<ItemListRowSchedulerRequest> requests = null;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var list = await this.GetByIdAsync(id);
                var listStatus = list.GetStatus();
                if (listStatus != ItemListStatus.New)
                {
                    if (listStatus == ItemListStatus.Waiting && bayId.HasValue == false)
                    {
                        return new BadRequestOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(
                            null,
                            "Cannot execute the list because no bay was specified.");
                    }
                    else if (listStatus != ItemListStatus.Waiting)
                    {
                        return new BadRequestOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(
                            null, $"Cannot execute the list bacause its current state is {listStatus}.");
                    }
                }

                requests = await this.BuildRequestsForRowsAsync(list, areaId, bayId);
                await this.schedulerRequestExecutionProvider.CreateRangeAsync(requests);
                if (bayId.HasValue)
                {
                    var extraPriorityForRowsWithoutPriority = list.Rows.Any(r => r.Priority.HasValue == false) ? 1 : 0;

                    await this.bayProvider.UpdatePriorityAsync(
                        bayId.Value,
                        list.Rows.Max(r => r.Priority) + extraPriorityForRowsWithoutPriority);
                }

                scope.Complete();

                return new SuccessOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(requests);
            }
        }

        public async Task<IOperationResult<ItemListExecution>> SuspendAsync(int id)
        {
            await this.GetByIdAsync(id);
            throw new NotImplementedException();
        }

        public async Task<IOperationResult<ItemListExecution>> UpdateAsync(ItemListExecution model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingList = this.databaseContext.ItemLists.Find(model.Id);
            this.databaseContext.Entry(existingList).CurrentValues.SetValues(model);

            await this.databaseContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemListExecution>(model);
        }

        private async Task<IEnumerable<ItemListRowSchedulerRequest>> BuildRequestsForRowsAsync(ItemListExecution list, int areaId, int? bayId)
        {
            var requests = new List<ItemListRowSchedulerRequest>(list.Rows.Count());
            ItemListRowExecution previousRow = null;
            foreach (var row in list.Rows.OrderBy(r => r.Priority.HasValue ? r.Priority : int.MaxValue))
            {
                int? basePriority = null;
                if (row.Priority.HasValue == false)
                {
                    var previousRowPriority = previousRow?.Priority;

                    System.Diagnostics.Debug.Assert(
                        requests.LastOrDefault() == null || requests.LastOrDefault()?.Priority.HasValue == true,
                        "Scheduler requests for list rows must always have a priority.");

                    basePriority = requests.LastOrDefault()?.Priority.Value;

                    if (previousRowPriority.HasValue)
                    {
                        basePriority++;
                    }
                }

                var result = await this.rowExecutionProvider.PrepareForExecutionInListAsync(row, areaId, bayId, basePriority);

                if (result.Success)
                {
                    requests.Add(result.Entity);
                }

                previousRow = row;
            }

            return requests;
        }

        #endregion
    }
}
