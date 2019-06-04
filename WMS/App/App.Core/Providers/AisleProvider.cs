using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class AisleProvider : IAisleProvider
    {
        #region Fields

        private readonly Data.WebAPI.Contracts.IAislesDataService aislesDataService;

        private readonly Data.WebAPI.Contracts.IAreasDataService areasDataService;

        #endregion

        #region Constructors

        public AisleProvider(
            Data.WebAPI.Contracts.IAislesDataService aislesDataService,
            Data.WebAPI.Contracts.IAreasDataService areasDataService)
        {
            this.aislesDataService = aislesDataService;
            this.areasDataService = areasDataService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<IEnumerable<Aisle>>> GetAislesByAreaIdAsync(int areaId)
        {
            try
            {
                var result = (await this.areasDataService.GetAislesAsync(areaId))
                    .Select(a => new Aisle
                    {
                        Id = a.Id,
                        AreaId = a.AreaId,
                        AreaName = a.AreaName,
                        Name = a.Name
                    });

                return new OperationResult<IEnumerable<Aisle>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<Aisle>>(e);
            }
        }

        public async Task<IEnumerable<Aisle>> GetAllAsync()
        {
            try
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
            catch
            {
                return new List<Aisle>();
            }
        }

        public async Task<int> GetAllCountAsync()
        {
            try
            {
                return await this.aislesDataService.GetAllCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<Aisle> GetByIdAsync(int id)
        {
            try
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
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
