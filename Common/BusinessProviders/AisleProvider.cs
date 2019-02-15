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

        #endregion

        #region Constructors

        public AisleProvider(WMS.Data.WebAPI.Contracts.IAislesDataService aislesDataService)
        {
            this.aislesDataService = aislesDataService;
        }

        #endregion

        public int AreaId { get; set; }

        public string AreaName { get; set; }

        public string Name { get; set; }

        public async Task<IEnumerable<Aisle>> GetAllAsync()
        {
            return (await this.aislesDataService.GetAllAsync())
                .Select(c => new Aisle { });
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
    }
}
