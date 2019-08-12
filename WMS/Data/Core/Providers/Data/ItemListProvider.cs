using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemListProvider : BaseProvider, IItemListProvider
    {
        #region Fields

        private readonly IMapper mapper;

        #endregion

        #region Constructors

        public ItemListProvider(
            DatabaseContext dataContext,
            IMapper mapper,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.mapper = mapper;
        }

        #endregion

        #region Methods

        public static void SetPolicies(BasePolicyModel model)
        {
            model.AddPolicy((model as IItemListPolicy).ComputeUpdatePolicy());
            model.AddPolicy((model as IItemListDeletePolicy).ComputeDeletePolicy());
            model.AddPolicy((model as IItemListPolicy).ComputeExecutePolicy());
            model.AddPolicy((model as IItemListPolicy).ComputeAddRowPolicy());
        }

        public async Task<IOperationResult<ItemListDetails>> CreateAsync(ItemListDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var validationError = model.ValidateBusinessModel(this.DataContext.ItemLists);
            if (!string.IsNullOrEmpty(validationError))
            {
                return new BadRequestOperationResult<ItemListDetails>(
                    validationError,
                    model);
            }

            var entry = await this.DataContext.ItemLists.AddAsync(
                this.mapper.Map<Common.DataModels.ItemList>(model));

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount <= 0)
            {
                return new CreationErrorOperationResult<ItemListDetails>();
            }

            var createdModel = await this.GetByIdAsync(entry.Entity.Id);

            this.NotificationService.PushCreate(createdModel);

            return new SuccessOperationResult<ItemListDetails>(createdModel);
        }

        public async Task<IOperationResult<ItemListDetails>> DeleteAsync(int id)
        {
            var existingModel = await this.GetByIdAsync(id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemListDetails>();
            }

            if (!existingModel.CanDelete())
            {
                return new UnprocessableEntityOperationResult<ItemListDetails>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            var rows = await this.DataContext.ItemListRows
                .Where(r => r.ItemListId == id)
                .ToArrayAsync();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                this.DataContext.ItemListRows.RemoveRange(rows);
                this.DataContext.ItemLists.Remove(new Common.DataModels.ItemList { Id = id });

                var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
                if (changedEntitiesCount > 0)
                {
                    this.NotificationService.PushDelete(existingModel);
                }

                scope.Complete();
            }

            return new SuccessOperationResult<ItemListDetails>(existingModel);
        }

        public async Task<IEnumerable<ItemList>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var models = await this.GetAllBase()
                .ToArrayAsync<ItemList, Common.DataModels.ItemList>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));

            foreach (var model in models)
            {
                SetPolicies(model);
            }

            return models;
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<ItemList, Common.DataModels.ItemList>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<IEnumerable<ItemList>> GetByAreaIdAsync(int id)
        {
            return await this.GetByAreaId(id).ToListAsync();
        }

        public async Task<ItemListDetails> GetByIdAsync(int id)
        {
            var model = await this.GetAllDetailsBase()
                .SingleOrDefaultAsync(i => i.Id == id);

            if (model != null)
            {
                SetPolicies(model);
            }

            return model;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                propertyName,
                this.DataContext.ItemLists,
                this.GetAllBase());
        }

        public async Task<IOperationResult<ItemListDetails>> UpdateAsync(ItemListDetails model)
        {
            var result = await this.UpdateAsync(
                model,
                this.DataContext.ItemLists,
                this.DataContext);

            this.NotificationService.PushUpdate(model);

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4058:Overloads with a \"StringComparison\" parameter should be used",
            Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<ItemList, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsInt = int.TryParse(search, out var searchAsInt);

            return i =>
                i.Code.Contains(search)
                || (i.Description != null && i.Description.Contains(search))
                || (successConversionAsInt
                    && (Equals(i.ItemListRowsCount, searchAsInt)
                        || Equals(i.Priority, searchAsInt)));
        }

        private IQueryable<ItemList> GetAllBase()
        {
            return this.DataContext.ItemLists
                .Select(i => new ItemList
                {
                    Id = i.Id,
                    Code = i.Code,
                    Description = i.Description,
                    Priority = i.Priority,
                    ShipmentUnitCode = i.ShipmentUnitCode,
                    ShipmentUnitDescription = i.ShipmentUnitDescription,
                    CompletedRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Completed),
                    ErrorRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Error),
                    ExecutingRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Executing),
                    NewRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.New),
                    WaitingRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Waiting),
                    IncompleteRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Incomplete),
                    SuspendedRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Suspended),
                    ReadyRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Ready),
                    HasActiveRows = i.ItemListRows.Any(r =>
                        r.Status != Enums.ItemListRowStatus.Completed &&
                        r.Status != Enums.ItemListRowStatus.New),
                    ItemListType = i.ItemListType,
                    ItemListRowsCount = i.ItemListRows.Count(),
                    CreationDate = i.CreationDate,
                });
        }

        private IQueryable<ItemListDetails> GetAllDetailsBase()
        {
            return this.DataContext.ItemLists
                .Select(i => new ItemListDetails
                {
                    Id = i.Id,
                    Code = i.Code,
                    Description = i.Description,
                    Priority = i.Priority,
                    ItemListType = i.ItemListType,
                    RowsCount = i.ItemListRows.Count(),
                    CompletedRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Completed),
                    ErrorRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Error),
                    ExecutingRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Executing),
                    WaitingRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Waiting),
                    IncompleteRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Incomplete),
                    SuspendedRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Suspended),
                    NewRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.New),
                    ReadyRowsCount =
                        i.ItemListRows.Count(r => r.Status == Enums.ItemListRowStatus.Ready),
                    HasActiveRows = i.ItemListRows.Any(r =>
                        r.Status != Enums.ItemListRowStatus.Completed &&
                        r.Status != Enums.ItemListRowStatus.New),
                    CreationDate = i.CreationDate,
                    Job = i.Job,
                    CustomerOrderCode = i.CustomerOrderCode,
                    CustomerOrderDescription = i.CustomerOrderDescription,
                    ShipmentUnitAssociated = i.ShipmentUnitAssociated,
                    ShipmentUnitCode = i.ShipmentUnitCode,
                    ShipmentUnitDescription = i.ShipmentUnitDescription,
                    LastModificationDate = i.LastModificationDate,
                    FirstExecutionDate = i.FirstExecutionDate,
                    ExecutionEndDate = i.ExecutionEndDate,
                });
        }

        private IQueryable<ItemList> GetByAreaId(int areaId)
        {
            return this.DataContext.ItemLists.Join(
                    this.DataContext.ItemListRows,
                    il => il.Id,
                    ilr => ilr.ItemListId,
                    (il, ilr) => new
                    {
                        ItemList = il,
                        ItemListRow = ilr,
                    })
                .Join(
                    this.DataContext.Compartments,
                    j => j.ItemListRow.ItemId,
                    c => c.ItemId,
                    (j, c) => new
                    {
                        ItemList = j.ItemList,
                        ItemListRow = j.ItemListRow,
                        Compartment = c,
                    })
                .Join(
                    this.DataContext.Machines,
                    j => j.Compartment.LoadingUnit.Cell.AisleId,
                    m => m.AisleId,
                    (j, m) => new
                    {
                        ItemList = j.ItemList,
                        ItemListRow = j.ItemListRow,
                        Compartment = j.Compartment,
                        Machine = m,
                    })
                .Where(j => j.Compartment.LoadingUnit.Cell.Aisle.AreaId == areaId)
                .GroupBy(x => x.ItemList)
                .Select(g => new ItemList
                {
                    Id = g.Key.Id,
                    Description = g.Key.Description,
                    Machines = g.Select(x => x.Machine)
                        .Select(x => new
                        {
                            Id = x.Id,
                            ErrorTime = x.ErrorTime,
                            Image = x.Image,
                            Model = x.Model,
                            Nickname = x.Nickname,
                        }).Distinct()
                        .Select(m1 => new Machine
                        {
                            Id = m1.Id,
                            ErrorTime = m1.ErrorTime,
                            Image = m1.Image,
                            Model = m1.Model,
                            Nickname = m1.Nickname,
                        }),
                });
        }

        #endregion
    }
}
