using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class LoadingUnitTypeProvider : ILoadingUnitTypeProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.ILoadingUnitTypesDataService loadingUnitTypesDataService;

        #endregion

        #region Constructors

        public LoadingUnitTypeProvider(WMS.Data.WebAPI.Contracts.ILoadingUnitTypesDataService loadingUnitTypesDataService)
        {
            this.loadingUnitTypesDataService = loadingUnitTypesDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Enumeration>> GetAllAsync()
        {
            try
            {
                return (await this.loadingUnitTypesDataService.GetAllAsync())
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
                return await this.loadingUnitTypesDataService.GetAllCountAsync();
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
                var type = await this.loadingUnitTypesDataService.GetByIdAsync(id);
                return new Enumeration(type.Id, type.Description);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
