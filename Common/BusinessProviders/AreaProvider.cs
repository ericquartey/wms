using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class AreaProvider : IAreaProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService;

        private readonly WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService;

        #endregion

        #region Constructors

        public AreaProvider(
            WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService,
            WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService)
        {
            this.areasDataService = areasDataService;
            this.itemsDataService = itemsDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Area>> GetAllAsync()
        {
            return (await this.areasDataService.GetAllAsync())
                .Select(a => new Area
                {
                    Id = a.Id,
                    Name = a.Name,
                });
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.areasDataService.GetAllCountAsync();
        }

        public async Task<IEnumerable<Area>> GetAreasWithAvailabilityAsync(int id)
        {
            return (await this.itemsDataService.GetAreasWithAvailabilityAsync(id))
                .Select(a => new Area
                {
                    Id = a.Id,
                    Name = a.Name
                });
        }

        public async Task<Area> GetByIdAsync(int id)
        {
            var area = await this.areasDataService.GetByIdAsync(id);
            return new Area
            {
                Id = area.Id,
                Name = area.Name,
            };
        }

        #endregion
    }
}
