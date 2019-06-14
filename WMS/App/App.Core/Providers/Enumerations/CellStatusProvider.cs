using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Interfaces;
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
            try
            {
                return (await this.cellStatusesDataService.GetAllAsync())
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
                return await this.cellStatusesDataService.GetAllCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }
}
