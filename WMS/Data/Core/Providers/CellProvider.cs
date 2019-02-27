using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
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

        public async Task<IOperationResult<CellDetails>> CreateAsync(CellDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = await this.dataContext.Cells.AddAsync(new Common.DataModels.Cell
            {
                AbcClassId = model.AbcClassId,
                AisleId = model.AisleId,
                CellNumber = model.Number,
                CellStatusId = model.CellStatusId,
                CellTypeId = model.CellTypeId,
                Column = model.Column,
                Floor = model.Floor,
                Priority = model.Priority,
                Side = (Common.DataModels.Side)model.Side,
                XCoordinate = model.XCoordinate,
                YCoordinate = model.YCoordinate,
                ZCoordinate = model.ZCoordinate,
            });

            var changedEntitiesCount = await this.dataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                model.Id = entry.Entity.Id;
            }

            return new SuccessOperationResult<CellDetails>(model);
        }

        public async Task<IEnumerable<Cell>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBy = null,
            IExpression whereExpression = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .ToArrayAsync(
                    skip,
                    take,
                    orderBy,
                    whereExpression,
                    BuildSearchExpression(searchString));
        }

        public async Task<int> GetAllCountAsync(
            IExpression whereExpression = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync(
                    whereExpression,
                    BuildSearchExpression(searchString));
        }

        public async Task<IEnumerable<Cell>> GetByAisleIdAsync(int aisleId)
        {
            return await this.dataContext.Cells
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
                           CellTypeDescription = c.CellType.Description,
                           XCoordinate = c.XCoordinate,
                           YCoordinate = c.YCoordinate,
                           ZCoordinate = c.ZCoordinate,
                       })
                       .ToArrayAsync();
        }

        public async Task<IEnumerable<Cell>> GetByAreaIdAsync(int areaId)
        {
            return await this.dataContext.Cells
                       .Include(c => c.Aisle)
                       .ThenInclude(a => a.Area)
                       .Where(c => c.Aisle.AreaId == areaId)
                       .OrderBy(c => c.Aisle.Name)
                       .ThenBy(c => c.CellNumber)
                       .Include(c => c.AbcClass)
                       .Include(c => c.CellType)
                       .Include(c => c.CellStatus)
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
                           CellTypeDescription = c.CellType.Description,
                           XCoordinate = c.XCoordinate,
                           YCoordinate = c.YCoordinate,
                           ZCoordinate = c.ZCoordinate,
                       })
                       .ToArrayAsync();
        }

        public async Task<CellDetails> GetByIdAsync(int id)
        {
            var loadingUnitsCount = await this.dataContext.LoadingUnits
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

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.dataContext.Cells,
                       this.GetAllBase());
        }

        public async Task<IOperationResult<CellDetails>> UpdateAsync(CellDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.Cells.Find(model.Id);

            if (existingModel == null)
            {
                return new NotFoundOperationResult<CellDetails>();
            }

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<CellDetails>(model);
        }

        private static Expression<Func<Cell, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (c) =>
                c.AbcClassDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.AisleName.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.AreaName.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.LoadingUnitsDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Status.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.CellTypeDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Column.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Floor.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Number.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Side.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Priority.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        private IQueryable<Cell> GetAllBase()
        {
            return this.dataContext.Cells
                .Include(c => c.AbcClass)
                .Include(c => c.Aisle)
                .ThenInclude(a => a.Area)
                .Include(c => c.CellStatus)
                .Include(c => c.CellType)
                .GroupJoin(
                    this.dataContext.LoadingUnits
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
                        CellTypeDescription = c.Cell.CellType.Description,
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
