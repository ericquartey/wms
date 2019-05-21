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
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemListExecutionProvider : IItemListExecutionProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly DatabaseContext dataContext;

        private readonly IItemListRowExecutionProvider rowExecutionProvider;

        private readonly ISchedulerRequestExecutionProvider schedulerRequestExecutionProvider;

        #endregion

        #region Constructors

        public ItemListExecutionProvider(
            DatabaseContext dataContext,
            IItemListRowExecutionProvider rowExecutionProvider,
            IBayProvider bayProvider,
            ISchedulerRequestExecutionProvider schedulerRequestExecutionProvider)
        {
            this.dataContext = dataContext;
            this.rowExecutionProvider = rowExecutionProvider;
            this.schedulerRequestExecutionProvider = schedulerRequestExecutionProvider;
            this.bayProvider = bayProvider;
        }

        #endregion

        #region Methods

        public async Task<ItemListOperation> GetByIdAsync(int id)
        {
            return await this.dataContext.ItemLists
                .Include(l => l.ItemListRows)
                .Select(i => new ItemListOperation
                {
                    Id = i.Id,
                    Code = i.Code,
                    CompletedRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Completed),
                    ErrorRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Error),
                    ExecutingRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Executing),
                    IncompleteRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Incomplete),
                    NewRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.New),
                    SuspendedRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Suspended),
                    TotalRowsCount = i.ItemListRows.Count(),
                    WaitingRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Waiting),
                    Rows = i.ItemListRows.Select(r => new ItemListRowOperation
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
                if (list.Status != ItemListStatus.New &&
                    list.Status == ItemListStatus.Waiting &&
                    !bayId.HasValue)
                {
                    return new BadRequestOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(
                        null,
                        "Cannot execute the list because no bay was specified.");
                }

                requests = await this.BuildRequestsForRowsAsync(list, areaId, bayId);
                await this.schedulerRequestExecutionProvider.CreateRangeAsync(requests);
                if (bayId.HasValue)
                {
                    var extraPriorityForRowsWithoutPriority = list.Rows.Any(r => !r.Priority.HasValue) ? 1 : 0;

                    await this.bayProvider.UpdatePriorityAsync(
                        bayId.Value,
                        list.Rows.Max(r => r.Priority) + extraPriorityForRowsWithoutPriority);
                }

                scope.Complete();

                return new SuccessOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(requests);
            }
        }

        public async Task<IOperationResult<ItemListOperation>> SuspendAsync(int id)
        {
            await this.GetByIdAsync(id);
            throw new NotImplementedException();
        }

        public async Task<IOperationResult<ItemListOperation>> UpdateAsync(ItemListOperation model)
        {
            return await this.UpdateAsync(
                model,
                this.dataContext.ItemLists,
                this.dataContext);
        }

        private async Task<IEnumerable<ItemListRowSchedulerRequest>> BuildRequestsForRowsAsync(ItemListOperation list, int areaId, int? bayId)
        {
            var requests = new List<ItemListRowSchedulerRequest>(list.Rows.Count());
            ItemListRowOperation previousRow = null;
            foreach (var row in list.Rows.OrderBy(r => r.Priority.HasValue ? r.Priority : int.MaxValue))
            {
                int? basePriority = null;
                if (!row.Priority.HasValue)
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
