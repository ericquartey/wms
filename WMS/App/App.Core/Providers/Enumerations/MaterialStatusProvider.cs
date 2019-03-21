using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class MaterialStatusProvider : IMaterialStatusProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IMaterialStatusesDataService materialStatusesDataService;

        #endregion

        #region Constructors

        public MaterialStatusProvider(WMS.Data.WebAPI.Contracts.IMaterialStatusesDataService materialStatusesDataService)
        {
            this.materialStatusesDataService = materialStatusesDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Enumeration>> GetAllAsync()
        {
            return (await this.materialStatusesDataService.GetAllAsync())
                .Select(c => new Enumeration(c.Id, c.Description));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.materialStatusesDataService.GetAllCountAsync();
        }

        public async Task<Enumeration> GetByIdAsync(int id)
        {
            var materialStatus = await this.materialStatusesDataService.GetByIdAsync(id);
            return new Enumeration(materialStatus.Id, materialStatus.Description);
        }

        #endregion
    }
}
