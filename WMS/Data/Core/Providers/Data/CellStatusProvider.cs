using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class CellStatusProvider : ICellStatusProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public CellStatusProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<CellStatus>> GetAllAsync()
        {
            return await this.dataContext.CellStatuses
               .Select(c => new CellStatus
               {
                   Id = c.Id,
                   Description = c.Description
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.dataContext.CellStatuses.CountAsync();
        }

        public async Task<CellStatus> GetByIdAsync(int id)
        {
            return await this.dataContext.CellStatuses
                 .Select(c => new CellStatus
                 {
                     Id = c.Id,
                     Description = c.Description
                 })
                 .SingleOrDefaultAsync(a => a.Id == id);
        }

        #endregion
    }
}
