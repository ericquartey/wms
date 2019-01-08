using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Ferretto.Common.EF;
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

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                // Save data in cache.
                this.cache.Set(key, cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }

        private IEnumerable<Item> RetrieveItems()
        {
            return this.dataContext.Items
                .Select(i =>
                    new Item
                    {
                        Id = i.Id,
                        Code = i.Code
                    })
                .ToArray();
        }

        private IEnumerable<ItemList> RetrieveLists()
        {
            return this.dataContext.ItemLists
            .Select(l =>
                new ItemList
                {
                    Id = l.Id
                })
            .ToArray();
        }

        private IEnumerable<Mission> RetrieveMissions()
        {
            return this.dataContext.Missions
               .Select(m =>
                   new Mission
                   {
                       Id = m.Id
                   })
                .ToArray();
        }

        #endregion Methods
    }
}
