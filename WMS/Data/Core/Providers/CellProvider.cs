using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class CellProvider : ICellProvider
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
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var models = await this.GetAllBase()
                .ToArrayAsync<Cell, Common.DataModels.Cell>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));

            foreach (var model in models)
            {
                this.SetPolicies(model);
            }

            return models;
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<Cell, Common.DataModels.Cell>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<IEnumerable<Cell>> GetByAisleIdAsync(int aisleId)
        {
            return await this.dataContext.Cells
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

            var model = await this.dataContext.Cells
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

            if (model != null)
            {
                this.SetPolicies(model);
            }

            return model;
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

            var existingModel = await this.GetByIdAsync(model.Id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<CellDetails>();
            }

            if (!existingModel.CanUpdate())
            {
                return new UnprocessableEntityOperationResult<CellDetails>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            var existingDataModel = this.dataContext.Cells.Find(model.Id);
            this.dataContext.Entry(existingDataModel).CurrentValues.SetValues(model);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<CellDetails>(model);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4058:Overloads with a \"StringComparison\" parameter should be used",
            Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<Cell, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsInt = int.TryParse(search, out var searchAsInt);

            return (c) =>
                (c.AbcClassDescription != null && c.AbcClassDescription.Contains(search))
                || (c.AisleName != null && c.AisleName.Contains(search))
                || (c.AreaName != null && c.AreaName.Contains(search))
                || (c.LoadingUnitsDescription != null && c.LoadingUnitsDescription.Contains(search))
                || (c.Status != null && c.Status.Contains(search))
                || (c.CellTypeDescription != null && c.CellTypeDescription.Contains(search))
                || c.Side.ToString().Contains(search)
                || (successConversionAsInt
                    && (Equals(c.Floor, searchAsInt)
                        || Equals(c.Number, searchAsInt)
                        || Equals(c.Priority, searchAsInt)));
        }

        private IQueryable<Cell> GetAllBase()
        {
            return this.dataContext.Cells
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
