using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class AreaProvider : IAreaProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public AreaProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Aisle>> GetAislesAsync(int id)
        {
            return await this.dataContext.Aisles
                       .Where(a => a.AreaId == id)
                       .OrderBy(a => a.Area.Name)
                       .ThenBy(a => a.Name)
                       .Select(a => new Aisle
                       {
                           Id = a.Id,
                           Name = a.Name,
                           AreaId = a.AreaId,
                           AreaName = a.Area.Name,
                       })
                       .ToArrayAsync();
        }

        public async Task<IEnumerable<Area>> GetAllAsync()
        {
            return await this.dataContext.Areas
                       .Select(a => new Area
                       {
                           Id = a.Id,
                           Name = a.Name
                       })
                       .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.dataContext.Areas.CountAsync();
        }

        public async Task<Area> GetByIdAsync(int id)
        {
            return await this.dataContext.Areas
                       .Select(a => new Area
                       {
                           Id = a.Id,
                           Name = a.Name
                       })
                       .SingleOrDefaultAsync(a => a.Id == id);
        }

        public async Task<AreaAvailable> GetByIdForExecutionAsync(int id)
        {
            return await this.dataContext.Areas
               .Include(a => a.Bays)
               .ThenInclude(b => b.Missions)
               .Select(a => new AreaAvailable
               {
                   Id = a.Id,
                   Bays = a.Bays.Select(b => new BayAvailable
                   {
                       Id = b.Id,
                       LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                       LoadingUnitsBufferUsage = b.Missions.Count(
                           m => m.Status != Common.DataModels.MissionStatus.Completed
                           &&
                           m.Status != Common.DataModels.MissionStatus.Incomplete)
                   })
               })
               .SingleAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Area>> GetByItemIdAvailabilityAsync(int id)
        {
            return await this.dataContext.Compartments
                       .Where(c => c.ItemId == id)
                       .Where(c => (c.Stock - c.ReservedForPick + c.ReservedToPut) > 0)
                       .Select(c => new
                       {
                           Id = c.LoadingUnit.Cell.Aisle.AreaId,
                           Name = c.LoadingUnit.Cell.Aisle.Area.Name,
                       })
                       .Distinct()
                       .Select(c => new Area
                       {
                           Id = c.Id,
                           Name = c.Name,
                       })
                       .ToArrayAsync();
        }

        #endregion
    }
}
