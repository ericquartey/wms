using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class ItemProvider : BaseProvider, IItemProvider
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

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            return await this.GetAllBase().ToArrayAsync();
        }

        public async Task<IEnumerable<Item>> GetAllAsync(
            int skip,
            int take,
            string orderBy = null,
            Expression<Func<Item, bool>> whereExpression = null,
            Expression<Func<Item, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                             .ApplyTransform(
                                 skip,
                                 take,
                                 orderBy,
                                 whereExpression,
                                 searchExpression)
                             .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync(Expression<Func<Item, bool>> whereExpression = null, Expression<Func<Item, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                             .ApplyTransform(whereExpression, searchExpression)
                             .CountAsync();
        }

        public async Task<Item> GetByIdAsync(int id)
        {
            return await this.GetAllBase()
                .SingleOrDefaultAsync(i => i.Id == id);
        }

        public async Task<object[]> GetUniqueValuesAsync(string propertyName)
        {
            return await GetUniqueValuesAsync(propertyName, this.dataContext.Items);
        }

        public async Task<Item> UpdateAsync(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var existingModel = this.dataContext.Items.Find(item.Id);

            if (existingModel == null)
            {
                throw new ArgumentException("Specified object not found", nameof(item));
            }

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(item);
            await this.dataContext.SaveChangesAsync();

            return item;
        }

        private IQueryable<Item> GetAllBase()
        {
            return this.dataContext.Items
                .AsNoTracking()
                    .Include(i => i.AbcClass)
                    .Include(i => i.ItemCategory)
                    .GroupJoin(
                        this.dataContext.Compartments
                            .AsNoTracking()
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

        #endregion
    }
}
