using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            return (await this.loadingUnitTypesDataService.GetAllAsync())
                .Select(c => new Enumeration(c.Id, c.Description));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.loadingUnitTypesDataService.GetAllCountAsync();
        }

        public async Task<Enumeration> GetByIdAsync(int id)
        {
            var type = await this.loadingUnitTypesDataService.GetByIdAsync(id);
            return new Enumeration(type.Id, type.Description);
        }

        #endregion
    }
}
