using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
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
