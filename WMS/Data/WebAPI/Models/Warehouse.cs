using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Ferretto.WMS.Data.WebAPI.Models
{
    public class Warehouse : IWarehouse
    {
        #region Fields

        private readonly IMemoryCache cache;

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public Warehouse(
            IMemoryCache cache,
            DatabaseContext context)
        {
            this.cache = cache;
            this.dataContext = context;
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<AbcClass> AbcClasses => this.GetValue(this.RetrieveAbcClasses);

        public IEnumerable<ItemCategory> ItemCategories => this.GetValue(this.RetrieveItemCategories);

        public IEnumerable<Item> Items => this.GetValue(this.RetrieveItems);

        public IEnumerable<ItemList> Lists => this.GetValue(this.RetrieveLists);

        public IEnumerable<Mission> Missions => this.GetValue(this.RetrieveMissions);

        #endregion Properties

        #region Methods

        public T GetValue<T>(Func<T> retrieveData, [CallerMemberName] string key = null)
        {
            // Look for cache key.
            if (!this.cache.TryGetValue<T>(key, out var cacheEntry))
            {
                // Key not in cache, so get data.
                cacheEntry = retrieveData();

                // Keep in cache for this time, reset time if accessed.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                // Save data in cache.
                this.cache.Set(key, cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }

        public async Task<Item> UpdateAsync(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var existingModel = this.dataContext.Items.Find(item.Id);

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(item);

            await this.dataContext.SaveChangesAsync();

            return item;
        }

        private IEnumerable<AbcClass> RetrieveAbcClasses()
        {
            return this.dataContext.AbcClasses
            .Select(c =>
                new AbcClass
                {
                    Id = c.Id,
                    Description = c.Description
                })
            .ToArray();
        }

        private IEnumerable<ItemCategory> RetrieveItemCategories()
        {
            return this.dataContext.ItemCategories
            .Select(c =>
                new ItemCategory
                {
                    Id = c.Id,
                    Description = c.Description
                })
            .ToArray();
        }

        private IEnumerable<Item> RetrieveItems()
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
                        (a, b) => new Item
                        {
                            ItemCategories = this.ItemCategories,
                            AbcClasses = this.AbcClasses,
                            Id = a.Item.Id,
                            AbcClassId = a.Item.AbcClassId,
                            AverageWeight = a.Item.AverageWeight,
                            CreationDate = a.Item.CreationDate,
                            FifoTimePick = a.Item.FifoTimePick,
                            FifoTimeStore = a.Item.FifoTimeStore,
                            Height = a.Item.Height,
                            InventoryDate = a.Item.InventoryDate,
                            InventoryTolerance = a.Item.InventoryTolerance,
                            ManagementType = (ItemManagementType)a.Item.ManagementType,
                            LastModificationDate = a.Item.LastModificationDate,
                            LastPickDate = a.Item.LastPickDate,
                            LastStoreDate = a.Item.LastStoreDate,
                            Length = a.Item.Length,
                            MeasureUnitDescription = a.Item.MeasureUnit.Description,
                            PickTolerance = a.Item.PickTolerance,
                            ReorderPoint = a.Item.ReorderPoint,
                            ReorderQuantity = a.Item.ReorderQuantity,
                            StoreTolerance = a.Item.StoreTolerance,
                            Width = a.Item.Width,
                            Code = a.Item.Code,
                            Description = a.Item.Description,
                            TotalReservedForPick = b != null ? b.TotalReservedForPick : 0,
                            TotalReservedToStore = b != null ? b.TotalReservedToStore : 0,
                            TotalStock = b != null ? b.TotalStock : 0
                        }).ToArray();
        }

        private IEnumerable<ItemList> RetrieveLists()
        {
            return this.dataContext.ItemLists
                .AsNoTracking()
                .Include(l => l.ItemListRows)
                .Select(l => new ItemList
                {
                    Id = l.Id,
                    Code = l.Code,
                    Description = l.Description,
                    Priority = l.Priority,
                    ItemListStatus = (ItemListStatus)l.Status,
                    ItemListType = (ItemListType)l.ItemListType,
                    ItemListRowsCount = l.ItemListRows.Count(),
                    ItemListItemsCount = l.ItemListRows.Sum(row => row.RequiredQuantity),
                    CreationDate = l.CreationDate
                })

             .ToArray();
        }

        private IEnumerable<Mission> RetrieveMissions()
        {
            return this.dataContext.Missions
               .Select(m =>
                   new Mission
                   {
                       Id = m.Id,
                       BayId = m.BayId,
                       CellId = m.CellId,
                       CompartmentId = m.CompartmentId,
                       Lot = m.Lot,
                       ItemId = m.ItemId,
                       ItemListId = m.ItemListId,
                       ItemListRowId = m.ItemListRowId,
                       LoadingUnitId = m.LoadingUnitId,
                       MaterialStatusId = m.MaterialStatusId,
                       PackageTypeId = m.PackageTypeId,
                       Quantity = m.RequiredQuantity,
                       RegistrationNumber = m.RegistrationNumber,
                       Status = (MissionStatus)m.Status,
                       Sub1 = m.Sub1,
                       Sub2 = m.Sub2,
                       Type = (MissionType)m.Type
                   })
                .ToArray();
        }

        #endregion Methods
    }
}
