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
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemListExecutionProvider : BaseProvider, IItemListExecutionProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly ILogger<ItemListExecutionProvider> logger;

        private readonly IItemListRowExecutionProvider rowExecutionProvider;

        #endregion

        #region Constructors

        public ItemListExecutionProvider(
            DatabaseContext dataContext,
            ILogger<ItemListExecutionProvider> logger,
            IItemListRowExecutionProvider rowExecutionProvider,
            IBayProvider bayProvider,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.rowExecutionProvider = rowExecutionProvider;
            this.bayProvider = bayProvider;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public static void SetPolicies(BasePolicyModel model)
        {
            model.AddPolicy((model as IItemListPolicy).ComputeExecutePolicy());
        }

        public async Task<ItemListOperation> GetByIdAsync(int id)
        {
            var result = await this.DataContext.ItemLists
                .Include(l => l.ItemListRows)
                .Select(i => new ItemListOperation
                {
                    Id = i.Id,
                    Code = i.Code,
                    OperationType = (ItemListType)i.ItemListType,
                    CompletedRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Completed),
                    ErrorRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Error),
                    ExecutingRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Executing),
                    IncompleteRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Incomplete),
                    NewRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.New),
                    SuspendedRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Suspended),
                    TotalRowsCount = i.ItemListRows.Count(),
                    WaitingRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Waiting),
                    ReadyRowsCount = i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Ready),
                    Rows = i.ItemListRows.Select(r => new ItemListRowOperation
                    {
                        Id = r.Id,
                        ItemId = r.ItemId,
                        ListId = r.ItemListId,
                        Lot = r.Lot,
                        Priority = r.Priority,
                        OperationType = (ItemListType)i.ItemListType,
                        MaterialStatusId = r.MaterialStatusId,
                        PackageTypeId = r.PackageTypeId,
                        RegistrationNumber = r.RegistrationNumber,
                        RequestedQuantity = r.RequestedQuantity,
                        Status = (ItemListRowStatus)r.Status,
                        Sub1 = r.Sub1,
                        Sub2 = r.Sub2,
                    }),
                })
                .SingleOrDefaultAsync(l => l.Id == id);

            if (result != null)
            {
                SetPolicies(result);
            }

            return result;
        }

        public async Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> PrepareForExecutionAsync(int id, int areaId, int? bayId)
        {
            IEnumerable<ItemListRowSchedulerRequest> requests = null;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var list = await this.GetByIdAsync(id);
                if (list.Status == ItemListStatus.Waiting && !bayId.HasValue)
                {
                    return new BadRequestOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(
                        Resources.ItemList.CannotExecuteBecauseNoBayWasSpecified);
                }

                if (!list.CanExecuteOperation(nameof(ItemListPolicy.Execute)))
                {
                    return new BadRequestOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(
                        list.GetCanExecuteOperationReason(nameof(ItemListPolicy.Execute)));
                }

                if (list.OperationType != ItemListType.Pick
                    && list.OperationType != ItemListType.Put)
                {
                    return new BadRequestOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(
                        string.Format(
                            Resources.Errors.TheListTypeIsNotSupported,
                            list.OperationType));
                }

                requests = await this.BuildRequestsForRowsAsync(list, areaId, bayId);
                if (!requests.Any())
                {
                    return new UnprocessableEntityOperationResult<IEnumerable<ItemListRowSchedulerRequest>>(
                        Resources.ItemList.NoneOfTheListRowsCouldBeProcessed);
                }

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
            var result = await this.UpdateAsync(
                model,
                this.DataContext.ItemLists,
                this.DataContext);

            this.NotificationService.PushUpdate(model);
            this.NotificationService.PushUpdate(typeof(ItemListRow), model);

            return result;
        }

        private async Task<IEnumerable<ItemListRowSchedulerRequest>> BuildRequestsForRowsAsync(ItemListOperation list, int areaId, int? bayId)
        {
            var requests = new List<ItemListRowSchedulerRequest>(list.Rows.Count());
            ItemListRowOperation previousRow = null;
            foreach (var row in list.Rows.OrderBy(r => r.Priority ?? int.MaxValue))
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
                    requests.AddRange(result.Entity);
                }
                else
                {
                    this.logger.LogWarning($"Creation of request for row (id={row.Id}) failed.");
                }

                previousRow = row;
            }

            return requests;
        }

        #endregion
    }
}
