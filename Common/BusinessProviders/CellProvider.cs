using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class CellProvider : ICellProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IAbcClassesDataService abcClassesDataService;

        private readonly WMS.Data.WebAPI.Contracts.IAislesDataService aislesDataService;

        private readonly WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService;

        private readonly WMS.Data.WebAPI.Contracts.ICellsDataService cellsDataService;

        private readonly ICellStatusProvider cellStatusProvider;

        private readonly ICellTypeProvider cellTypeProvider;

        #endregion

        #region Constructors

        public CellProvider(
            ICellStatusProvider cellStatusProvider,
            ICellTypeProvider cellTypeProvider,
            WMS.Data.WebAPI.Contracts.ICellsDataService cellsDataService,
            WMS.Data.WebAPI.Contracts.IAbcClassesDataService abcClassesDataService,
            WMS.Data.WebAPI.Contracts.IAislesDataService aislesDataService,
            WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService)
        {
            this.cellStatusProvider = cellStatusProvider;
            this.cellTypeProvider = cellTypeProvider;
            this.cellsDataService = cellsDataService;
            this.abcClassesDataService = abcClassesDataService;
            this.aislesDataService = aislesDataService;
            this.areasDataService = areasDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Aisle>> GetAislesByAreaIdAsync(int areaId)
        {
            return (await this.areasDataService.GetAislesAsync(areaId))
                .Select(a => new Aisle
                {
                    Id = a.Id,
                    AreaId = a.AreaId,
                    AreaName = a.AreaName,
                    Name = a.Name
                });
        }

        public async Task<IEnumerable<Cell>> GetAllAsync(
                    int skip = 0,
            int take = 0,
            IEnumerable<SortOption> orderBy = null,
            IExpression whereExpression = null,
            IExpression searchExpression = null)
        {
            var orderByString = orderBy != null ? string.Join(",", orderBy.Select(s => $"{s.PropertyName} {s.Direction}")) : null;

            return (await this.cellsDataService.GetAllAsync(skip, take, whereExpression?.ToString(), orderByString, searchExpression?.ToString()))
                .Select(c => new Cell
                {
                    Id = c.Id,
                    AbcClassDescription = c.AbcClassDescription,
                    AisleName = c.AisleName,
                    AreaName = c.AreaName,
                    LoadingUnitsCount = c.LoadingUnitsCount,
                    LoadingUnitsDescription = c.LoadingUnitsDescription,
                    Status = c.Status,
                    Type = c.Type,
                    Column = c.Column,
                    Floor = c.Floor,
                    Number = c.Number,
                    Priority = c.Priority,
                    Side = (Side)c.Side,
                    XCoordinate = c.XCoordinate,
                    YCoordinate = c.YCoordinate,
                    ZCoordinate = c.ZCoordinate,
                });
        }

        public async Task<int> GetAllCountAsync(IExpression whereExpression = null, IExpression searchExpression = null)
        {
            return await this.cellsDataService.GetAllCountAsync(whereExpression?.ToString(), searchExpression?.ToString());
        }

        public async Task<IEnumerable<Enumeration>> GetByAisleIdAsync(int aisleId)
        {
            return (await this.aislesDataService.GetCellsAsync(aisleId))
                .Select(c => new Enumeration(
                                   c.Id,
                                   $"{c.AreaName} - {c.AisleName} - Cell {c.Number} (Floor {c.Floor}, Column {c.Column}, {c.Side})")); // TODO: localize string
        }

        public async Task<IEnumerable<Enumeration>> GetByAreaIdAsync(int areaId)
        {
            return (await this.areasDataService.GetCellsAsync(areaId))
                .Select(c => new Enumeration(
                    c.Id,
                    $"{c.AreaName} - {c.AisleName} - Cell {c.Number} (Floor {c.Floor}, Column {c.Column}, {c.Side})")); // TODO: localize string
        }

        public async Task<CellDetails> GetByIdAsync(int id)
        {
            var cell = await this.cellsDataService.GetByIdAsync(id);

            var abcClass = await this.abcClassesDataService.GetAllAsync();
            var abcClassChoices = abcClass.Select(abc => new EnumerationString(abc.Id, abc.Description));
            var aisles = await this.GetAislesByAreaIdAsync(cell.AreaId);
            var aisleChoices = aisles.Select(aisle => new Enumeration(aisle.Id, aisle.Name));
            var cellStatusChoices = await this.cellStatusProvider.GetAllAsync();
            var cellTypeChoices = await this.cellTypeProvider.GetAllAsync();

            return new CellDetails
            {
                Id = cell.Id,
                AbcClassId = cell.AbcClassId,
                AisleId = cell.AisleId,
                AreaId = cell.AreaId,
                CellStatusId = cell.CellStatusId,
                CellTypeId = cell.CellTypeId,
                Column = cell.Column,
                Floor = cell.Floor,
                Number = cell.Number,
                Priority = cell.Priority,
                Side = (Side)cell.Side,
                XCoordinate = cell.XCoordinate,
                YCoordinate = cell.YCoordinate,
                ZCoordinate = cell.ZCoordinate,
                AbcClassChoices = abcClassChoices,
                AisleChoices = aisleChoices,
                CellStatusChoices = cellStatusChoices,
                CellTypeChoices = cellTypeChoices
            };
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.cellsDataService.GetUniqueValuesAsync(propertyName);
        }

        public async Task<IOperationResult> UpdateAsync(CellDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var cell = await this.cellsDataService.GetByIdAsync(model.Id);

                await this.cellsDataService.UpdateAsync(new WMS.Data.WebAPI.Contracts.CellDetails
                {
                    Id = cell.Id,
                    AbcClassId = cell.AbcClassId,
                    AisleId = cell.AisleId,
                    AreaId = cell.AreaId,
                    CellStatusId = cell.CellStatusId,
                    CellTypeId = cell.CellTypeId,
                    Column = cell.Column,
                    Floor = cell.Floor,
                    Number = cell.Number,
                    Priority = cell.Priority,
                    Side = cell.Side,
                    XCoordinate = cell.XCoordinate,
                    YCoordinate = cell.YCoordinate,
                    ZCoordinate = cell.ZCoordinate,
                    LoadingUnitsCount = cell.LoadingUnitsCount
                });

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        #endregion
    }
}
