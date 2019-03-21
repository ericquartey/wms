using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class CellPositionProvider : ICellPositionProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.ICellPositionsDataService cellPositionsDataService;

        #endregion

        #region Constructors

        public CellPositionProvider(WMS.Data.WebAPI.Contracts.ICellPositionsDataService cellPositionsDataService)
        {
            this.cellPositionsDataService = cellPositionsDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Enumeration>> GetAllAsync()
        {
            return (await this.cellPositionsDataService.GetAllAsync())
                .Select(c => new Enumeration(c.Id, c.Description));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.cellPositionsDataService.GetAllCountAsync();
        }

        #endregion
    }
}
