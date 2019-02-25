using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemListRowProvider : IItemListRowProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public ItemListRowProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemListRowDetails>> CreateAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = await this.dataContext.ItemListRows.AddAsync(new Common.DataModels.ItemListRow
            {
                Code = model.Code,
                DispatchedQuantity = model.DispatchedQuantity,
                ItemId = model.ItemId,
                ItemListId = model.ItemListId,
                Lot = model.Lot,
                MaterialStatusId = model.MaterialStatusId,
                PackageTypeId = model.PackageTypeId,
                Priority = model.RowPriority,
                RegistrationNumber = model.RegistrationNumber,
                RequiredQuantity = model.RequiredQuantity,
                Status = (Common.DataModels.ItemListRowStatus)model.ItemListRowStatus,
                Sub1 = model.Sub1,
                Sub2 = model.Sub2
            });

            var changedEntitiesCount = await this.dataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                model.Id = entry.Entity.Id;
                model.CompletionDate = entry.Entity.CompletionDate;
                model.CreationDate = entry.Entity.CreationDate;
                model.LastExecutionDate = entry.Entity.LastExecutionDate;
                model.LastModificationDate = entry.Entity.LastModificationDate;
            }

            return new SuccessOperationResult<ItemListRowDetails>(model);
        }

        public async Task<IEnumerable<ItemListRow>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBy = null,
            IExpression whereExpression = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .ToArrayAsync(
                    skip,
                    take,
                    orderBy,
                    whereExpression,
                    BuildSearchExpression(searchString));
        }

        public async Task<int> GetAllCountAsync(
            IExpression whereExpression = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync(
                    whereExpression,
                    BuildSearchExpression(searchString));
        }

        public async Task<ItemListRowDetails> GetByIdAsync(int id)
        {
            return await this.GetAllDetailsBase()
                       .SingleOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<ItemListRow>> GetByItemListIdAsync(int id)
        {
            return await this.GetAllBase()
                       .Where(l => l.ItemListId == id)
                       .ToArrayAsync();
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.dataContext.ItemListRows,
                       this.GetAllBase());
        }

        public async Task<IOperationResult<ItemListRowDetails>> UpdateAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.ItemListRows.Find(model.Id);

            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemListRowDetails>();
            }

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemListRowDetails>(model);
        }

        private static Expression<Func<ItemListRow, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (i) =>
                i.Code.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemUnitMeasure.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.MaterialStatusDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.DispatchedQuantity.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.RequiredQuantity.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.RowPriority.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        private IQueryable<ItemListRow> GetAllBase()
        {
            return this.dataContext.ItemListRows
                .Include(l => l.MaterialStatus)
                .Include(l => l.Item)
                .ThenInclude(i => i.MeasureUnit)
                .Select(l => new ItemListRow
                {
                    Id = l.Id,
                    Code = l.Code,
                    RowPriority = l.Priority,
                    ItemDescription = l.Item.Description,
                    RequiredQuantity = l.RequiredQuantity,
                    DispatchedQuantity = l.DispatchedQuantity,
                    ItemListId = l.ItemListId,
                    ItemListRowStatus = (ItemListRowStatus)l.Status,
                    MaterialStatusDescription = l.MaterialStatus.Description,
                    CreationDate = l.CreationDate,
                    ItemUnitMeasure = l.Item.MeasureUnit.Description
                });
        }

        private IQueryable<ItemListRowDetails> GetAllDetailsBase()
        {
            return this.dataContext.ItemListRows
                .Include(lr => lr.ItemList)
                .Include(lr => lr.Item)
                .ThenInclude(i => i.MeasureUnit)
                .Select(l => new ItemListRowDetails
                {
                    Id = l.Id,
                    Code = l.Code,
                    RowPriority = l.Priority,
                    ItemId = l.Item.Id,
                    RequiredQuantity = l.RequiredQuantity,
                    DispatchedQuantity = l.DispatchedQuantity,
                    ItemListRowStatus = (ItemListRowStatus)l.Status,
                    ItemDescription = l.Item.Description,
                    CreationDate = l.CreationDate,
                    ItemListCode = l.ItemList.Code,
                    ItemListDescription = l.ItemList.Description,
                    ItemListId = l.ItemListId,
                    ItemListType = (ItemListType)l.ItemList.ItemListType,
                    ItemListStatus = (ItemListStatus)l.ItemList.Status,
                    CompletionDate = l.CompletionDate,
                    LastExecutionDate = l.LastExecutionDate,
                    LastModificationDate = l.LastModificationDate,
                    Lot = l.Lot,
                    RegistrationNumber = l.RegistrationNumber,
                    Sub1 = l.Sub1,
                    Sub2 = l.Sub2,
                    PackageTypeId = l.PackageTypeId,
                    MaterialStatusId = l.MaterialStatusId,
                    ItemUnitMeasure = l.Item.MeasureUnit.Description
                });
        }

        #endregion
    }
}
