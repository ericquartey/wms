using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class CellProvider : ICellProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public CellProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Cell>> GetAllAsync(
            int skip,
            int take,
            string orderBy = null,
            Expression<Func<Cell, bool>> whereExpression = null,
            Expression<Func<Cell, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                             .ApplyTransform(
                                 skip,
                                 take,
                                 orderBy,
                                 whereExpression,
                                 searchExpression)
                             .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync(
            Expression<Func<Cell, bool>> whereExpression = null,
            Expression<Func<Cell, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                             .ApplyTransform(whereExpression, searchExpression)
                             .CountAsync();
        }

        public async Task<IEnumerable<Cell>> GetByAisleIdAsync(int aisleId)
        {
            return await this.dataContext.Cells
                             .AsNoTracking()
                             .Include(c => c.Aisle)
                             .ThenInclude(a => a.Area)
                             .Where(c => c.AisleId == aisleId)
                             .OrderBy(c => c.CellNumber)
                             .Select(c => new Cell
                             {
                                 Id = c.Id,
                                 AbcClassDescription = c.AbcClass.Description,
                                 AisleName = c.Aisle.Name,
                                 AreaName = c.Aisle.Area.Name,
                                 Column = c.Column,
                                 Floor = c.Floor,
                                 Number = c.CellNumber,
                                 Priority = c.Priority,
                                 Side = (Side)c.Side,
                                 Status = c.CellStatus.Description,
                                 Type = c.CellType.Description,
                                 XCoordinate = c.XCoordinate,
                                 YCoordinate = c.YCoordinate,
                                 ZCoordinate = c.ZCoordinate,
                             })
                             .ToArrayAsync();
        }

        public async Task<IEnumerable<Cell>> GetByAreaIdAsync(int areaId)
        {
            return await this.dataContext.Cells
                       .AsNoTracking()
                       .Include(c => c.Aisle)
                       .ThenInclude(a => a.Area)
                       .Where(c => c.Aisle.AreaId == areaId)
                       .OrderBy(c => c.Aisle.Name)
                       .ThenBy(c => c.CellNumber)
                       .Select(c => new Cell
                       {
                           Id = c.Id,
                           AbcClassDescription = c.AbcClass.Description,
                           AisleName = c.Aisle.Name,
                           AreaName = c.Aisle.Area.Name,
                           Column = c.Column,
                           Floor = c.Floor,
                           Number = c.CellNumber,
                           Priority = c.Priority,
                           Side = (Side)c.Side,
                           Status = c.CellStatus.Description,
                           Type = c.CellType.Description,
                           XCoordinate = c.XCoordinate,
                           YCoordinate = c.YCoordinate,
                           ZCoordinate = c.ZCoordinate,
                       })
                       .ToArrayAsync();
        }

        public async Task<CellDetails> GetByIdAsync(int id)
        {
            var loadingUnitsCount = await this.dataContext.LoadingUnits
                                              .AsNoTracking()
                                              .CountAsync(l => l.CellId == id);

            return await this.dataContext.Cells
                             .Include(c => c.Aisle)
                             .ThenInclude(a => a.Area)
                             .Select(c => new CellDetails
                             {
                                 Id = c.Id,
                                 AbcClassId = c.AbcClassId,
                                 AisleId = c.AisleId,
                                 AreaId = c.Aisle.AreaId,
                                 CellStatusId = c.CellStatusId,
                                 CellTypeId = c.CellTypeId,
                                 Column = c.Column,
                                 Floor = c.Floor,
                                 LoadingUnitsCount = loadingUnitsCount,
                                 Number = c.CellNumber,
                                 Priority = c.Priority,
                                 Side = (Side)c.Side,
                                 XCoordinate = c.XCoordinate,
                                 YCoordinate = c.YCoordinate,
                                 ZCoordinate = c.ZCoordinate,
                             })
                             .SingleOrDefaultAsync(c => c.Id == id);
        }

        private IQueryable<Cell> GetAllBase()
        {
            return this.dataContext.Cells
                       .AsNoTracking()
                       .Include(c => c.AbcClass)
                       .Include(c => c.Aisle)
                       .ThenInclude(a => a.Area)
                       .Include(c => c.CellStatus)
                       .Include(c => c.CellType)
                       .GroupJoin(
                           this.dataContext.LoadingUnits
                               .AsNoTracking()
                               .GroupBy(l => l.CellId)
                               .Select(j => new
                               {
                                   CellId = j.Key,
                                   LoadingUnitsCount = j.Count(),
                                   LoadingUnitsDescription = string.Join(",", j.Select(x => x.Code)),
                               }),
                           c => c.Id,
                           l => l.CellId,
                           (c, l) => new
                           {
                               Cell = c,
                               LoadingUnitsAggregation = l.DefaultIfEmpty(),
                           })
                       .SelectMany(
                           x => x.LoadingUnitsAggregation.DefaultIfEmpty(),
                           (c, l) => new Cell
                           {
                               Id = c.Cell.Id,
                               AbcClassDescription = c.Cell.AbcClass.Description,
                               AisleName = c.Cell.Aisle.Name,
                               AreaName = c.Cell.Aisle.Area.Name,
                               Column = c.Cell.Column,
                               Floor = c.Cell.Floor,
                               Number = c.Cell.CellNumber,
                               Priority = c.Cell.Priority,
                               Side = (Side)c.Cell.Side,
                               Status = c.Cell.CellStatus.Description,
                               Type = c.Cell.CellType.Description,
                               XCoordinate = c.Cell.XCoordinate,
                               YCoordinate = c.Cell.YCoordinate,
                               ZCoordinate = c.Cell.ZCoordinate,
                               LoadingUnitsCount = l != null ? l.LoadingUnitsCount : 0,
                               LoadingUnitsDescription = l != null ? l.LoadingUnitsDescription : string.Empty,
                           });
        }

        #endregion
    }
}
