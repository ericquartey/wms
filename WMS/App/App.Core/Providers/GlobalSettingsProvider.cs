using System.Threading.Tasks;
using AutoMapper;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class GlobalSettingsProvider : IGlobalSettingsProvider
    {
        #region Fields

        private readonly Data.WebAPI.Contracts.IGlobalSettingsDataService globalSettingsDataService;

        private readonly IMapper mapper;

        #endregion

        #region Constructors

        public GlobalSettingsProvider(
            Data.WebAPI.Contracts.IGlobalSettingsDataService globalSettingsDataService,
            IMapper mapper)
        {
            this.globalSettingsDataService = globalSettingsDataService;
            this.mapper = mapper;
        }

        #endregion

        #region Methods

        public async Task<GlobalSettings> GetAllAsync()
        {
            var gS = await this.globalSettingsDataService.GetAllAsync();
            return this.mapper.Map<GlobalSettings>(gS);
        }

        #endregion
    }
}
