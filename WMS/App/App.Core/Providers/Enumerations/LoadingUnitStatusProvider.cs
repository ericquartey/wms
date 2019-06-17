using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class LoadingUnitStatusProvider : ILoadingUnitStatusProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.ILoadingUnitStatusesDataService loadingUnitStatusesDataService;

        #endregion

        #region Constructors

        public LoadingUnitStatusProvider(WMS.Data.WebAPI.Contracts.ILoadingUnitStatusesDataService loadingUnitStatusesDataService)
        {
            this.loadingUnitStatusesDataService = loadingUnitStatusesDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<EnumerationString>> GetAllAsync()
        {
            try
            {
                return (await this.loadingUnitStatusesDataService.GetAllAsync())
                    .Select(c => new EnumerationString(c.Id, c.Description));
            }
            catch
            {
                return new List<EnumerationString>();
            }
        }

        public async Task<int> GetAllCountAsync()
        {
            try
            {
                return await this.loadingUnitStatusesDataService.GetAllCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<EnumerationString> GetByIdAsync(string id)
        {
            try
            {
                var status = await this.loadingUnitStatusesDataService.GetByIdAsync(id);
                return new EnumerationString(status.Id, status.Description);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
