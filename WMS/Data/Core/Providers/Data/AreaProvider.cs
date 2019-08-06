using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class AreaProvider : BaseProvider, IAreaProvider
    {
        #region Constructors

        public AreaProvider(
            DatabaseContext dataContext,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Aisle>> GetAislesAsync(int id)
        {
            return await this.DataContext.Aisles
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
            return await this.DataContext.Areas
                .Select(a => new Area
                {
                    Id = a.Id,
                    Name = a.Name,
                })
                .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.DataContext.Areas.CountAsync();
        }

        public async Task<Area> GetByIdAsync(int id)
        {
            return await this.DataContext.Areas
                .Select(a => new Area
                {
                    Id = a.Id,
                    Name = a.Name,
                })
                .SingleOrDefaultAsync(a => a.Id == id);
        }

        public async Task<AreaAvailable> GetByIdForExecutionAsync(int id)
        {
            return await this.DataContext.Areas
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
                            m => m.Operations.Any(o =>
                                o.Status != Enums.MissionOperationStatus.Completed
                                &&
                                o.Status != Enums.MissionOperationStatus.Incomplete)),
                    }),
                })
                .SingleAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Area>> GetByItemIdAsync(int id)
        {
            return await this.DataContext.ItemsAreas
                .Where(x => x.ItemId == id)
                .Join(
                    this.DataContext.Areas,
                    ia => ia.AreaId,
                    a => a.Id,
                    (ia, a) => a)
                .Select(a => new Area
                {
                    Id = a.Id,
                    Name = a.Name,
                })
                .ToArrayAsync();
        }

        public async Task<IEnumerable<Area>> GetByItemIdAvailabilityAsync(int id)
        {
            return await this.DataContext.Compartments
                .Where(c => c.ItemId == id)
                .Where(c => (c.Stock - c.ReservedForPick + c.ReservedToPut) > 0)
                .Where(c => c.LoadingUnit.Cell != null)
                .Select(c => new
                {
                    Id = c.LoadingUnit.Cell.Aisle.AreaId,
                    Name = c.LoadingUnit.Cell.Aisle.Area.Name,
                })
                .Distinct()
                .GroupJoin(
                    this.DataContext.Bays,
                    area => area.Id,
                    bay => bay.AreaId,
                    (area, bays) => new { Area = area, Bays = bays })
                .Select(join => new Area
                {
                    Id = join.Area.Id,
                    Name = join.Area.Name,
                    Bays = join.Bays.Select(b => new Bay
                    {
                        Id = b.Id,
                        Description = b.Description,
                    }),
                })
                .ToArrayAsync();
        }

        #endregion
    }
}
