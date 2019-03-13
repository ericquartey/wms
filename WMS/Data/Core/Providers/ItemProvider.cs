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
    internal class ItemProvider : IItemProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public ItemProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemDetails>> CreateAsync(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = await this.dataContext.Items.AddAsync(new Common.DataModels.Item
            {
                AbcClassId = model.AbcClassId,
                AverageWeight = model.AverageWeight,
                Code = model.Code,
                Description = model.Description,
                FifoTimePick = model.FifoTimePick,
                FifoTimeStore = model.FifoTimeStore,
                Height = model.Height,
                Image = model.Image,
                InventoryDate = model.InventoryDate,
                InventoryTolerance = model.InventoryTolerance,
                ItemCategoryId = model.ItemCategoryId,
                LastPickDate = model.LastPickDate,
                LastStoreDate = model.LastStoreDate,
                Length = model.Length,
                ManagementType = (Common.DataModels.ItemManagementType)model.ManagementType,
                MeasureUnitId = model.MeasureUnitId,
                Note = model.Note,
                PickTolerance = model.PickTolerance,
                ReorderPoint = model.ReorderPoint,
                ReorderQuantity = model.ReorderQuantity,
                StoreTolerance = model.StoreTolerance,
                Width = model.Width
            });

            var changedEntitiesCount = await this.dataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                model.Id = entry.Entity.Id;
                model.CreationDate = entry.Entity.CreationDate;
                model.LastModificationDate = entry.Entity.LastModificationDate;
            }

            return new SuccessOperationResult<ItemDetails>(model);
        }

        public async Task<IEnumerable<Item>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .ToArrayAsync<Item, Common.DataModels.Item>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<Item, Common.DataModels.Item>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<ItemDetails> GetByIdAsync(int id)
        {
            var compartmentsCount =
                await this.dataContext.Compartments
                    .CountAsync(c => c.ItemId == id);

            var result = await this.GetAllDetailsBase()
                             .SingleOrDefaultAsync(i => i.Id == id);
            result.CompartmentsCount = compartmentsCount;
            return result;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.dataContext.Items,
                       this.GetAllBase());
        }

        public async Task<IOperationResult<ItemDetails>> UpdateAsync(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.Items.Find(model.Id);

            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemDetails>();
            }

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemDetails>(model);
        }

        private static Expression<Func<Item, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (i) =>
                i.AbcClassDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Code.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Description.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemCategoryDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.TotalAvailable.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.MeasureUnitDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ;
        }

        private IQueryable<Item> GetAllBase(
            Expression<Func<Common.DataModels.Item, bool>> whereExpression = null,
            Expression<Func<Common.DataModels.Item, bool>> searchExpression = null)
        {
            var actualWhereFunc = whereExpression ?? ((i) => true);
            var actualSearchFunc = searchExpression ?? ((i) => true);

            return this.dataContext.Items
                .Where(actualWhereFunc)
                .Where(actualSearchFunc)
                .GroupJoin(
                    this.dataContext.Compartments
                        .Where(c => c.ItemId != null)
                        .GroupBy(c => c.ItemId)
                        .Select(j => new
                        {
                            ItemId = j.Key,
                            TotalStock = j.Sum(x => x.Stock),
                            TotalReservedForPick = j.Sum(x => x.ReservedForPick),
                            TotalReservedToStore = j.Sum(x => x.ReservedToStore)
                        }),
                    i => i.Id,
                    c => c.ItemId,
                    (i, c) => new
                    {
                        Item = i,
                        CompartmentsAggregation = c
                    })
                .SelectMany(
                    temp => temp.CompartmentsAggregation.DefaultIfEmpty(),
                    (i, c) => new Item
                    {
                        Id = i.Item.Id,
                        AbcClassId = i.Item.AbcClassId,
                        AverageWeight = i.Item.AverageWeight,
                        CreationDate = i.Item.CreationDate,
                        FifoTimePick = i.Item.FifoTimePick,
                        FifoTimeStore = i.Item.FifoTimeStore,
                        Height = i.Item.Height,
                        Image = i.Item.Image,
                        InventoryDate = i.Item.InventoryDate,
                        InventoryTolerance = i.Item.InventoryTolerance,
                        ManagementType = (ItemManagementType)i.Item.ManagementType,
                        LastModificationDate = i.Item.LastModificationDate,
                        LastPickDate = i.Item.LastPickDate,
                        LastStoreDate = i.Item.LastStoreDate,
                        Length = i.Item.Length,
                        MeasureUnitDescription = i.Item.MeasureUnit.Description,
                        PickTolerance = i.Item.PickTolerance,
                        ReorderPoint = i.Item.ReorderPoint,
                        ReorderQuantity = i.Item.ReorderQuantity,
                        StoreTolerance = i.Item.StoreTolerance,
                        Width = i.Item.Width,
                        Code = i.Item.Code,
                        Description = i.Item.Description,
                        TotalReservedForPick = c != null ? c.TotalReservedForPick : 0,
                        TotalReservedToStore = c != null ? c.TotalReservedToStore : 0,
                        TotalStock = c != null ? c.TotalStock : 0,
                        ItemCategoryId = i.Item.ItemCategoryId,
                        ItemCategoryDescription = i.Item.ItemCategory.Description,
                        AbcClassDescription = i.Item.AbcClass.Description,
                    });
        }

        private IQueryable<ItemDetails> GetAllDetailsBase(
            Expression<Func<Common.DataModels.Item, bool>> whereExpression = null,
            Expression<Func<Common.DataModels.Item, bool>> searchExpression = null)
        {
            var actualWhereFunc = whereExpression ?? ((i) => true);
            var actualSearchFunc = searchExpression ?? ((i) => true);

            return this.dataContext.Items
                .Where(actualWhereFunc)
                .Where(actualSearchFunc)
                .GroupJoin(
                    this.dataContext.Compartments
                        .Where(c => c.ItemId != null)
                        .GroupBy(c => c.ItemId)
                        .Select(j => new
                        {
                            ItemId = j.Key,
                            TotalStock = j.Sum(x => x.Stock),
                            TotalReservedForPick = j.Sum(x => x.ReservedForPick),
                            TotalReservedToStore = j.Sum(x => x.ReservedToStore)
                        }),
                    i => i.Id,
                    c => c.ItemId,
                    (i, c) => new
                    {
                        Item = i,
                        CompartmentsAggregation = c
                    })
                .SelectMany(
                    temp => temp.CompartmentsAggregation.DefaultIfEmpty(),
                    (a, b) => new ItemDetails
                    {
                        Id = a.Item.Id,
                        Code = a.Item.Code,
                        Description = a.Item.Description,
                        ItemCategoryId = a.Item.ItemCategoryId,
                        Note = a.Item.Note,

                        AbcClassId = a.Item.AbcClassId,
                        MeasureUnitId = a.Item.MeasureUnitId,
                        MeasureUnitDescription = a.Item.MeasureUnit.Description,
                        ManagementType = (ItemManagementType)a.Item.ManagementType,
                        FifoTimePick = a.Item.FifoTimePick,
                        FifoTimeStore = a.Item.FifoTimeStore,
                        ReorderPoint = a.Item.ReorderPoint,
                        ReorderQuantity = a.Item.ReorderQuantity,

                        Height = a.Item.Height,
                        Length = a.Item.Length,
                        Width = a.Item.Width,
                        PickTolerance = a.Item.PickTolerance,
                        StoreTolerance = a.Item.StoreTolerance,
                        InventoryTolerance = a.Item.InventoryTolerance,
                        AverageWeight = a.Item.AverageWeight,

                        Image = a.Item.Image,

                        CreationDate = a.Item.CreationDate,
                        InventoryDate = a.Item.InventoryDate,
                        LastModificationDate = a.Item.LastModificationDate,
                        LastPickDate = a.Item.LastPickDate,
                        LastStoreDate = a.Item.LastStoreDate,

                        TotalAvailable =
                            b != null
                                ? b.TotalStock + b.TotalReservedToStore - b.TotalReservedForPick
                                : 0,
                    });
        }

        #endregion
    }
}
