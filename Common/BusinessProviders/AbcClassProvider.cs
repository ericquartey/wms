using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class AbcClassProvider : IAbcClassProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IAbcClassesDataService abcClassesDataService;

        #endregion

        #region Constructors

        public AbcClassesProvider(WMS.Data.WebAPI.Contracts.IAbcClassesDataService abcClassesDataService)
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
