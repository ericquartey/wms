﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class AbcClassProvider : IAbcClassProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IAbcClassesDataService abcClassesDataService;

        #endregion

        #region Constructors

        public AbcClassProvider(WMS.Data.WebAPI.Contracts.IAbcClassesDataService abcClassesDataService)
        {
            this.abcClassesDataService = abcClassesDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<EnumerationString>> GetAllAsync()
        {
            return (await this.abcClassesDataService.GetAllAsync())
                .Select(c => new EnumerationString(c.Id, c.Description));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.abcClassesDataService.GetAllCountAsync();
        }

        #endregion
    }
}
