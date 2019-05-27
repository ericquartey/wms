using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Extensions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
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

        private readonly Data.WebAPI.Contracts.ILoadingUnitTypesDataService loadingUnitTypesDataService;

        #endregion

        #region Constructors

        public CellProvider(
            ICellStatusProvider cellStatusProvider,
            ICellTypeProvider cellTypeProvider,
            WMS.Data.WebAPI.Contracts.ICellsDataService cellsDataService,
            WMS.Data.WebAPI.Contracts.IAbcClassesDataService abcClassesDataService,
            WMS.Data.WebAPI.Contracts.IAislesDataService aislesDataService,
            WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService,
            Data.WebAPI.Contracts.ILoadingUnitTypesDataService loadingUnitTypesDataService)
        {
            this.cellStatusProvider = cellStatusProvider;
            this.cellTypeProvider = cellTypeProvider;
            this.cellsDataService = cellsDataService;
            this.abcClassesDataService = abcClassesDataService;
            this.aislesDataService = aislesDataService;
            this.areasDataService = areasDataService;
            this.loadingUnitTypesDataService = loadingUnitTypesDataService;
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
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var cells = await this.cellsDataService
                .GetAllAsync(skip, take, whereString, orderBySortOptions.ToQueryString(), searchString);

            return cells
                .Select(c => new Cell
                {
                    Id = c.Id,
                    AbcClassDescription = c.AbcClassDescription,
                    AisleName = c.AisleName,
                    AreaName = c.AreaName,
                    LoadingUnitsCount = c.LoadingUnitsCount,
                    LoadingUnitsDescription = c.LoadingUnitsDescription,
                    Status = c.Status,
                    CellTypeDescription = c.CellTypeDescription,
                    Column = c.Column,
                    Floor = c.Floor,
                    Number = c.Number,
                    Priority = c.Priority,
                    Side = (Side)c.Side,
                    XCoordinate = c.XCoordinate,
                    YCoordinate = c.YCoordinate,
                    ZCoordinate = c.ZCoordinate,
                    Policies = c.GetPolicies(),
                });
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            return await this.cellsDataService
                .GetAllCountAsync(whereString, searchString);
        }

        public async Task<IOperationResult<IEnumerable<Enumeration>>> GetByAisleIdAsync(int aisleId)
        {
            try
            {
                var result = (await this.aislesDataService.GetCellsAsync(aisleId))
                    .Select(c => new Enumeration(
                        c.Id,
                        $"{c.AreaName} - {c.AisleName} - Cell {c.Number} (Floor {c.Floor}, Column {c.Column}, {c.Side})")); // TODO: localize string

                return new OperationResult<IEnumerable<Enumeration>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<Enumeration>>(e);
            }
        }

        public async Task<IOperationResult<IEnumerable<Enumeration>>> GetByAreaIdAsync(int areaId)
        {
            try
            {
                var result = (await this.areasDataService.GetCellsAsync(areaId))
                    .Select(c => new Enumeration(
                        c.Id,
                        $"{c.AreaName} - {c.AisleName} - Cell {c.Number} (Floor {c.Floor}, Column {c.Column}, {c.Side})")); // TODO: localize string

                return new OperationResult<IEnumerable<Enumeration>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<Enumeration>>(e);
            }
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
                AbcClassChoices = abcClassChoices,
                AbcClassId = cell.AbcClassId,
                AisleChoices = aisleChoices,
                AisleId = cell.AisleId,
                AreaId = cell.AreaId,
                CellStatusChoices = cellStatusChoices,
                CellStatusId = cell.CellStatusId,
                CellTypeChoices = cellTypeChoices,
                CellTypeId = cell.CellTypeId,
                Column = cell.Column,
                Floor = cell.Floor,
                Id = cell.Id,
                LoadingUnitsCount = cell.LoadingUnitsCount,
                Number = cell.Number,
                Priority = cell.Priority,
                Side = (Side)cell.Side,
                XCoordinate = cell.XCoordinate,
                YCoordinate = cell.YCoordinate,
                ZCoordinate = cell.ZCoordinate,
                Policies = cell.GetPolicies(),
            };
        }

        public async Task<IOperationResult<IEnumerable<Enumeration>>> GetByLoadingUnitTypeIdAsync(int loadingUnitTypeId)
        {
            try
            {
                var result = (await this.loadingUnitTypesDataService.GetByLoadingUnitTypeIdAsync(loadingUnitTypeId))
                    .Select(c => new Enumeration(
                        c.Id,
                        $"{c.AreaName} - {c.AisleName} - Cell {c.Number} (Floor {c.Floor}, Column {c.Column}, {c.Side})"));

                return new OperationResult<IEnumerable<Enumeration>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<Enumeration>>(e);
            }
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.cellsDataService.GetUniqueValuesAsync(propertyName);
        }

        public async Task<IOperationResult<CellDetails>> UpdateAsync(CellDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                await this.cellsDataService.UpdateAsync(
                    new WMS.Data.WebAPI.Contracts.CellDetails
                    {
                        AbcClassId = model.AbcClassId,
                        AisleId = model.AisleId.GetValueOrDefault(),
                        AreaId = model.AreaId.GetValueOrDefault(),
                        CellStatusId = model.CellStatusId.GetValueOrDefault(),
                        CellTypeId = model.CellTypeId,
                        Column = model.Column,
                        Floor = model.Floor,
                        Id = model.Id,
                        LoadingUnitsCount = model.LoadingUnitsCount,
                        Number = model.Number,
                        Priority = model.Priority.GetValueOrDefault(),
                        Side = (WMS.Data.WebAPI.Contracts.Side)model.Side,
                        XCoordinate = model.XCoordinate,
                        YCoordinate = model.YCoordinate,
                        ZCoordinate = model.ZCoordinate,
                    },
                    model.Id);

                return new OperationResult<CellDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<CellDetails>(ex);
            }
        }

        #endregion
    }
}
