using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class AisleProvider : IAisleProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IAislesDataService aislesDataService;

        private readonly WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService;

        #endregion

        #region Constructors

        public AisleProvider(
            WMS.Data.WebAPI.Contracts.IAislesDataService aislesDataService,
            WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService)
        {
            this.aislesDataService = aislesDataService;
            this.areasDataService = areasDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Aisle>> GetAislesByAreaIdAsync(int areaId)
        {
            return (await this.areasDataService.GetAislesAsync(areaId))
                .Select(a => new Aisle
                {
                    Id = a.Id,
                    AreaId = a.AreaId,
                    AreaName = a.AreaName,
                    Name = a.Name
                });
        }

        public async Task<IEnumerable<Aisle>> GetAllAsync()
        {
            return (await this.aislesDataService.GetAllAsync())
                .Select(a => new Aisle
                {
                    Id = a.Id,
                    AreaId = a.AreaId,
                    AreaName = a.AreaName,
                    Name = a.Name
                });
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.aislesDataService.GetAllCountAsync();
        }

        public async Task<Aisle> GetByIdAsync(int id)
        {
            var aisle = await this.aislesDataService.GetByIdAsync(id);
            return new Aisle
            {
                AreaId = aisle.AreaId,
                AreaName = aisle.AreaName,
                Id = aisle.Id,
                Name = aisle.Name
            };
        }

        #endregion
    }
}
