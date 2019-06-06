using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class ItemCategoryProvider : IItemCategoryProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IItemCategoriesDataService itemCategoriesDataService;

        #endregion

        #region Constructors

        public ItemCategoryProvider(WMS.Data.WebAPI.Contracts.IItemCategoriesDataService itemCategoriesDataService)
        {
            this.itemCategoriesDataService = itemCategoriesDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Enumeration>> GetAllAsync()
        {
            try
            {
                return (await this.itemCategoriesDataService.GetAllAsync())
                    .Select(c => new Enumeration(c.Id, c.Description));
            }
            catch
            {
                return new List<Enumeration>();
            }
        }

        public async Task<int> GetAllCountAsync()
        {
            try
            {
                return await this.itemCategoriesDataService.GetAllCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<Enumeration> GetByIdAsync(int id)
        {
            try
            {
                var category = await this.itemCategoriesDataService.GetByIdAsync(id);
                return new Enumeration(category.Id, category.Description);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
