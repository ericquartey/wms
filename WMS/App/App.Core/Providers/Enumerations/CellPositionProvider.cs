using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
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
            try
            {
                return (await this.cellPositionsDataService.GetAllAsync())
                    .Select(c => new Enumeration(c.Id, c.Description));
            }
            catch
            {
                return new List<Enumeration>();
            }
        }

        public async Task<int> GetAllCountAsync()
        {
            try
            {
                return await this.cellPositionsDataService.GetAllCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }
}
