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
                        RequestedQuantity = r.RequestedQuantity,
                        Status = (ItemListRowStatus)r.Status,
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

        public async Task<IEnumerable<SchedulerRequest>> PrepareForExecutionAsync(int id, int areaId, int? bayId)
        {
            IEnumerable<SchedulerRequest> requests = null;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var list = await this.GetByIdAsync(id);

                requests = await this.BuildRequestsAsync(list, areaId, bayId);

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

        private async Task<IEnumerable<SchedulerRequest>> BuildRequestsAsync(ItemList list, int areaId, int? bayId)
        {
            var requests = new List<SchedulerRequest>(list.Rows.Count());

            foreach (var row in list.Rows)
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
                    .FullyQualifyWithdrawalRequestAsync(row.ItemId, options);

                if (qualifiedRequest != null)
                {
                    qualifiedRequest.ListId = list.Id;
                    qualifiedRequest.ListRowId = row.Id;

                    requests.Add(qualifiedRequest);

                    row.Status = bayId.HasValue
                        ? ItemListRowStatus.Executing
                        : ItemListRowStatus.Waiting;
                }
                else
                {
                    row.Status = ItemListRowStatus.Suspended;
                }

                await this.itemListRowSchedulerProvider.UpdateAsync(row);
            }

            return requests;
        }

        #endregion
    }
}
