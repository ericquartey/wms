﻿using System.Collections.Generic;
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
            return (await this.loadingUnitStatusesDataService.GetAllAsync())
                            .Select(c => new EnumerationString(c.Id, c.Description));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.loadingUnitStatusesDataService.GetAllCountAsync();
        }

        public async Task<EnumerationString> GetByIdAsync(string id)
        {
            var status = await this.loadingUnitStatusesDataService.GetByIdAsync(id);
            return new EnumerationString(status.Id, status.Description);
        }

        #endregion
    }
}
