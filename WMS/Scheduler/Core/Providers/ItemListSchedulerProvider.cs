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

        private readonly DatabaseContext databaseContext;

        private readonly IItemListRowSchedulerProvider itemListRowSchedulerProvider;

        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion

        #region Constructors

        public ItemListSchedulerProvider()
        {
        }

        public ItemListSchedulerProvider(
            DatabaseContext databaseContext,
            IItemListRowSchedulerProvider itemListRowSchedulerProvider,
            ISchedulerRequestProvider schedulerRequestProvider)
        {
            this.databaseContext = databaseContext;
            this.itemListRowSchedulerProvider = itemListRowSchedulerProvider;
            this.schedulerRequestProvider = schedulerRequestProvider;
        }

        #endregion

        #region Methods

        public async Task<ItemList> GetByIdAsync(int id)
        {
            var list = await this.databaseContext.ItemLists
                .Include(l => l.ItemListRows)
                .Select(i => new ItemList
                {
                    Id = i.Id,
                    Code = i.Code,
                    Rows = i.ItemListRows.Select(r => new ItemListRow
                    {
                        Code = r.Code,
                        Id = r.Id,
                        ItemId = r.ItemId,
                        Lot = r.Lot,
                        MaterialStatusId = r.MaterialStatusId,
                        PackageTypeId = r.PackageTypeId,
                        RegistrationNumber = r.RegistrationNumber,
                        RequestedQuantity = r.RequiredQuantity,
                        Status = (ListRowStatus)r.Status,
                        Sub1 = r.Sub1,
                        Sub2 = r.Sub2,
                    })
                })
                .SingleOrDefaultAsync(l => l.Id == id);

            if (list == null)
            {
                throw new ArgumentException($"No list with id={id} exists.");
            }

            return list;
        }

        public async Task<IEnumerable<SchedulerRequest>> PrepareForExecutionAsync(ListExecutionRequest request)
        {
            IEnumerable<SchedulerRequest> requests = null;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var list = await this.GetByIdAsync(request.ListId);

                requests = await this.BuildRequestsAsync(list, request);

                await this.UpdateAsync(list);
                await this.schedulerRequestProvider.CreateRangeAsync(requests);

                scope.Complete();
            }

            return requests;
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

        private async Task<IEnumerable<SchedulerRequest>> BuildRequestsAsync(ItemList list, ListExecutionRequest executionRequest)
        {
            var requests = new List<SchedulerRequest>(list.Rows.Count());

            foreach (var row in list.Rows)
            {
                row.Status = executionRequest.BayId.HasValue ? ListRowStatus.Executing : ListRowStatus.Waiting;

                var schedulerRequest = new SchedulerRequest
                {
                    IsInstant = false,
                    Type = OperationType.Withdrawal,
                    BayId = executionRequest.BayId,
                    AreaId = executionRequest.AreaId,
                    RequestedQuantity = row.RequestedQuantity,
                    ItemId = row.ItemId,
                    ListId = executionRequest.ListId,
                    ListRowId = row.Id,
                    Lot = row.Lot,
                    MaterialStatusId = row.MaterialStatusId,
                    PackageTypeId = row.PackageTypeId,
                    RegistrationNumber = row.RegistrationNumber,
                    Sub1 = row.Sub1,
                    Sub2 = row.Sub2,
                };

                var qualifiedRequest = await this.schedulerRequestProvider.FullyQualifyWithdrawalRequestAsync(schedulerRequest);
                if (qualifiedRequest != null)
                {
                    requests.Add(qualifiedRequest);

                    if (executionRequest.BayId.HasValue)
                    {
                        row.Status = ListRowStatus.Executing;
                    }
                    else
                    {
                        row.Status = ListRowStatus.Waiting;
                    }
                }
                else
                {
                    row.Status = ListRowStatus.Suspended;
                }

                await this.itemListRowSchedulerProvider.UpdateAsync(row);
            }

            return requests;
        }

        #endregion
    }
}
