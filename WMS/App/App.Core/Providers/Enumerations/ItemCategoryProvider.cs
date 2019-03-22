using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            return (await this.itemCategoriesDataService.GetAllAsync())
                .Select(c => new Enumeration(c.Id, c.Description));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.itemCategoriesDataService.GetAllCountAsync();
        }

        public async Task<Enumeration> GetByIdAsync(int id)
        {
            var category = await this.itemCategoriesDataService.GetByIdAsync(id);
            return new Enumeration(category.Id, category.Description);
        }

        #endregion
    }
}
