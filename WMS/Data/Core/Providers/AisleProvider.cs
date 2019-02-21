using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class AisleProvider : IAisleProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public AisleProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Aisle>> GetAllAsync()
        {
            return await this.dataContext.Aisles
                       .Include(a => a.Area)
                       .Select(a => new Aisle
                       {
                           Id = a.Id,
                           Name = a.Name,
                           AreaId = a.AreaId,
                           AreaName = a.Area.Name,
                       })
                       .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.dataContext.Aisles.CountAsync();
        }

        public async Task<Aisle> GetByIdAsync(int id)
        {
            return await this.dataContext.Aisles
                       .Include(a => a.Area)
                       .Select(a => new Aisle
                       {
                           Id = a.Id,
                           Name = a.Name,
                           AreaId = a.AreaId,
                           AreaName = a.Area.Name,
                       })
                       .SingleOrDefaultAsync(a => a.Id == id);
        }

        #endregion
    }
}
