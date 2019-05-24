using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class AreaProvider : IAreaProvider
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

        public async Task<IOperationResult<ItemArea>> CreateAllowedByItemIdAsync(int id, int itemId)
        {
            try
            {
                var entry = await this.dataContext.ItemsAreas.AddAsync(new Common.DataModels.ItemArea
                {
                    AreaId = id,
                    ItemId = itemId
                });
                await this.dataContext.SaveChangesAsync();
                var model = new ItemArea { AreaId = entry.Entity.AreaId, ItemId = entry.Entity.ItemId };
                return new SuccessOperationResult<ItemArea>(model);
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityOperationResult<ItemArea>(ex);
            }
        }

        public async Task<IOperationResult<ItemArea>> DeleteAllowedByItemIdAsync(int id, int itemId)
        {
            var allowedAreas = await this.GetAllowedByItemIdAsync(itemId);
            var existingModel = allowedAreas.SingleOrDefault(m => m.Id == id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemArea>();
            }

            if (!existingModel.CanExecuteOperation(nameof(AreaPolicy.DeleteItemArea)))
            {
                return new UnprocessableEntityOperationResult<ItemArea>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            this.dataContext.ItemsAreas.Remove(new Common.DataModels.ItemArea { AreaId = id, ItemId = itemId });
            await this.dataContext.SaveChangesAsync();
            var model = new ItemArea
            {
                AreaId = id,
                ItemId = itemId
            };

            return new SuccessOperationResult<ItemArea>(model);
        }

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

        public async Task<IEnumerable<AllowedItemArea>> GetAllowedByItemIdAsync(int id)
        {
            var models = await this.dataContext.ItemsAreas
                                .Where(x => x.ItemId == id)
                                .GroupJoin(
                                    this.dataContext.Compartments,
                                    ia => new { ia.ItemId, ia.AreaId },
                                    c => new { ItemId = c.ItemId.Value, c.LoadingUnit.Cell.Aisle.AreaId },
                                    (ia, c) => new
                                    {
                                        a = ia.Area,
                                        c,
                                    })
                                .SelectMany(
                                    j => j.c.DefaultIfEmpty(),
                                    (j, c) => new
                                    {
                                        a = j.a,
                                        stock = c != null ? c.Stock : 0,
                                    })
                                .GroupBy(
                                    x => x.a,
                                    (key, group) => new AllowedItemArea
                                    {
                                        Id = key.Id,
                                        Name = key.Name,
                                        TotalStock = group.Sum(g => g.stock)
                                    })
                               .ToArrayAsync();

            foreach (var model in models)
            {
                this.SetPolicies(model);
            }

            return models;
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

        public async Task<IEnumerable<Area>> GetByItemIdAsync(int id)
        {
            return await this.dataContext.ItemsAreas
                             .Where(x => x.ItemId == id)
                             .Join(
                                    this.dataContext.Areas,
                                    ia => ia.AreaId,
                                    a => a.Id,
                                    (ia, a) => a)
                                    .Select(a => new Area
                                    {
                                        Id = a.Id,
                                        Name = a.Name
                                    })
                                   .ToArrayAsync();
        }

        public async Task<IEnumerable<Area>> GetByItemIdAvailabilityAsync(int id)
        {
            return await this.dataContext.Compartments
                .Where(c => c.ItemId == id)
                .Where(c => (c.Stock - c.ReservedForPick + c.ReservedToPut) > 0)
                .Where(c => c.LoadingUnit.Cell != null)
                .Select(c => new
                {
                    Id = c.LoadingUnit.Cell.Aisle.AreaId,
                    Name = c.LoadingUnit.Cell.Aisle.Area.Name,
                })
                .Distinct()
                .Select(a => new Area
                {
                    Id = a.Id,
                    Name = a.Name,
                })
                .ToArrayAsync();
        }

        #endregion
    }
}
