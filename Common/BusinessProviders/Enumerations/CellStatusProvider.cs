using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class CellStatusProvider : ICellStatusProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.ICellStatusesDataService cellStatusesDataService;

        #endregion

        #region Constructors

        public CellStatusProvider(WMS.Data.WebAPI.Contracts.ICellStatusesDataService cellStatusesDataService)
        {
            this.cellStatusesDataService = cellStatusesDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Enumeration>> GetAllAsync()
        {
            return (await this.cellStatusesDataService.GetAllAsync())
                            .Select(c => new Enumeration(c.Id, c.Description));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.cellStatusesDataService.GetAllCountAsync();
        }

        #endregion
    }
}
