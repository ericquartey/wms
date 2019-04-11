using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class ItemListProvider : IItemListProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public ItemListProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemListDetails>> CreateAsync(ItemListDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = await this.dataContext.ItemLists.AddAsync(new Common.DataModels.ItemList
            {
                Code = model.Code,
                CustomerOrderCode = model.CustomerOrderCode,
                CustomerOrderDescription = model.CustomerOrderDescription,
                Description = model.Description,
                ItemListType = (Common.DataModels.ItemListType)model.ItemListType,
                Job = model.Job,
                Priority = model.Priority,
                ShipmentUnitAssociated = model.ShipmentUnitAssociated,
                ShipmentUnitCode = model.ShipmentUnitCode,
                ShipmentUnitDescription = model.ShipmentUnitDescription
            });

            var changedEntitiesCount = await this.dataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                model.Id = entry.Entity.Id;
                model.CreationDate = entry.Entity.CreationDate;
                model.LastModificationDate = entry.Entity.LastModificationDate;
                model.ExecutionEndDate = entry.Entity.ExecutionEndDate;
                model.FirstExecutionDate = entry.Entity.FirstExecutionDate;
            }

            return new SuccessOperationResult<ItemListDetails>(model);
        }

        public async Task<IOperationResult<ItemListDetails>> DeleteAsync(int id)
        {
            var itemList = await this.GetByIdAsync(id);
            if (itemList == null)
            {
                return new NotFoundOperationResult<ItemListDetails>();
            }

            if (!itemList.CanDelete())
            {
                return new UnprocessableEntityOperationResult<ItemListDetails>
                {
                    Description = itemList.GetCanDeleteReason(),
                };
            }

            var rows = await this.dataContext.ItemListRows
                .Where(r => r.ItemListId == id)
                .ToArrayAsync();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                this.dataContext.ItemListRows.RemoveRange(rows);
                this.dataContext.ItemLists.Remove(new Common.DataModels.ItemList { Id = id });
                await this.dataContext.SaveChangesAsync();

                scope.Complete();
            }

            return new SuccessOperationResult<ItemListDetails>();
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
                this.SetPolicies(model);
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

        public async Task<ItemListDetails> GetByIdAsync(int id)
        {
            var model = await this.GetAllDetailsBase()
                .SingleOrDefaultAsync(i => i.Id == id);

            if (model != null)
            {
                this.SetPolicies(model);
            }

            return model;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                propertyName,
                this.dataContext.ItemLists,
                this.GetAllBase());
        }

        public async Task<IOperationResult<ItemListDetails>> UpdateAsync(ItemListDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = await this.GetByIdAsync(model.Id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemListDetails>();
            }

            if (!existingModel.CanUpdate())
            {
                return new UnprocessableEntityOperationResult<ItemListDetails>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            var existingDataModel = this.dataContext.ItemLists.Find(model.Id);
            this.dataContext.Entry(existingDataModel).CurrentValues.SetValues(model);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemListDetails>(model);
        }

        private static Expression<Func<ItemList, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return i =>
                i.Code.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Description.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemListRowsCount.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        private IQueryable<ItemList> GetAllBase()
        {
            return this.dataContext.ItemLists
                .Select(i => new ItemList
                {
                    Id = i.Id,
                    Code = i.Code,
                    Description = i.Description,
                    Priority = i.Priority,
                    CompletedRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Completed),
                    ExecutingRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Executing),
                    NewRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.New),
                    WaitingRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Waiting),
                    IncompleteRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Incomplete),
                    SuspendedRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Suspended),
                    HasActiveRows = i.ItemListRows.Any(r =>
                        r.Status != Common.DataModels.ItemListRowStatus.Completed &&
                        r.Status != Common.DataModels.ItemListRowStatus.New),
                    ItemListType = (ItemListType)i.ItemListType,
                    ItemListRowsCount = i.ItemListRows.Count(),
                    CreationDate = i.CreationDate
                });
        }

        private IQueryable<ItemListDetails> GetAllDetailsBase()
        {
            return this.dataContext.ItemLists
                .Select(i => new ItemListDetails
                {
                    Id = i.Id,
                    Code = i.Code,
                    Description = i.Description,
                    Priority = i.Priority,
                    ItemListType = (ItemListType)i.ItemListType,
                    RowsCount = i.ItemListRows.Count(),
                    CompletedRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Completed),
                    ExecutingRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Executing),
                    WaitingRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Waiting),
                    IncompleteRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Incomplete),
                    SuspendedRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.Suspended),
                    NewRowsCount =
                        i.ItemListRows.Count(r => r.Status == Common.DataModels.ItemListRowStatus.New),
                    HasActiveRows = i.ItemListRows.Any(r =>
                        r.Status != Common.DataModels.ItemListRowStatus.Completed &&
                        r.Status != Common.DataModels.ItemListRowStatus.New),
                    CreationDate = i.CreationDate,
                    Job = i.Job,
                    CustomerOrderCode = i.CustomerOrderCode,
                    CustomerOrderDescription = i.CustomerOrderDescription,
                    ShipmentUnitAssociated = i.ShipmentUnitAssociated,
                    ShipmentUnitCode = i.ShipmentUnitCode,
                    ShipmentUnitDescription = i.ShipmentUnitDescription,
                    LastModificationDate = i.LastModificationDate,
                    FirstExecutionDate = i.FirstExecutionDate,
                    ExecutionEndDate = i.ExecutionEndDate
                });
        }

        #endregion
    }
}
