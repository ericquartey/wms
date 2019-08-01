using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemCategoryProvider : BaseProvider, IItemCategoryProvider
    {
        #region Constructors

        public ItemCategoryProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<ItemCategory>> GetAllAsync()
        {
            return await this.DataContext.ItemCategories
               .Select(c => new ItemCategory
               {
                   Id = c.Id,
                   Description = c.Description,
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.DataContext.ItemCategories.CountAsync();
        }

        public async Task<ItemCategory> GetByIdAsync(int id)
        {
            return await this.DataContext.ItemCategories
                 .Select(c => new ItemCategory
                 {
                     Id = c.Id,
                     Description = c.Description,
                 })
                 .SingleOrDefaultAsync(i => i.Id == id);
        }

        #endregion
    }
}
