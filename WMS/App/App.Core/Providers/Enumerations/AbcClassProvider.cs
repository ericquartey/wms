using System.Collections.Generic;
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
            try
            {
                return (await this.abcClassesDataService.GetAllAsync())
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
                return await this.abcClassesDataService.GetAllCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }
}
