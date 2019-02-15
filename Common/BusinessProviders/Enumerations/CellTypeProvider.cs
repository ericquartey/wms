using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class CellTypeProvider : ICellTypeProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.ICellTypesDataService cellTypesDataService;

        #endregion

        #region Constructors

        public CellTypeProvider(WMS.Data.WebAPI.Contracts.ICellTypesDataService cellTypesDataService)
        {
            this.cellTypesDataService = cellTypesDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Enumeration>> GetAllAsync()
        {
            return (await this.cellTypesDataService.GetAllAsync())
                .Select(c => new Enumeration(c.Id, c.Description));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.cellTypesDataService.GetAllCountAsync();
        }

        #endregion
    }
}
