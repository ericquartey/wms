using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class CellProvider : ICellProvider
    {
        #region Fields

        private static readonly Expression<Func<DataModels.Cell, bool>> ClassAFilter =
                   cell => cell.AbcClassId == "A";

        private static readonly Expression<Func<DataModels.Cell, bool>> StatusEmptyFilter =
cell => cell.CellStatusId == 1;

        private static readonly Expression<Func<DataModels.Cell, bool>> StatusFullFilter =
                   cell => cell.CellStatusId == 3;

        private readonly IAbcClassProvider abcClassProvider;

        private readonly ICellStatusProvider cellStatusProvider;

        private readonly ICellTypeProvider cellTypeProvider;

        private readonly IDatabaseContextService dataContext;

        private readonly EnumerationProvider enumerationProvider;

        #endregion

        #region Constructors

        public CellProvider(
            IDatabaseContextService context,
            EnumerationProvider enumerationProvider,
            IAbcClassProvider abcClassProvider,
            ICellStatusProvider cellStatusProvider,
            ICellTypeProvider cellTypeProvider)
        {
            this.dataContext = context;
            this.enumerationProvider = enumerationProvider;
            this.abcClassProvider = abcClassProvider;
            this.cellStatusProvider = cellStatusProvider;
            this.cellTypeProvider = cellTypeProvider;
        }

        #endregion

        #region Methods

        public Task<IOperationResult> AddAsync(CellDetails model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<Cell> GetAll()
        {
            return GetAllCellsWithFilter(this.dataContext.Current);
        }

        public int GetAllCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Cells.AsNoTracking().Count();
            }
        }

        public IQueryable<Enumeration> GetByAisleId(int aisleId)
        {
            return this.dataContext.Current.Cells
                       .AsNoTracking()
                       .Include(c => c.Aisle)
                       .ThenInclude(a => a.Area)
                       .Where(c => c.AisleId == aisleId)
                       .OrderBy(c => c.CellNumber)
                       .Select(c => new Enumeration(
                                   c.Id,
                                   $"{c.Aisle.Area.Name} - {c.Aisle.Name} - Cell {c.CellNumber} (Floor {c.Floor}, Column {c.Column}, {c.Side})")); // TODO: localize string
        }

        public IQueryable<Enumeration> GetByAreaId(int areaId)
        {
            return this.dataContext.Current.Cells
                .AsNoTracking()
                .Include(c => c.Aisle)
                .ThenInclude(a => a.Area)
                .Where(c => c.Aisle.AreaId == areaId)
                .OrderBy(c => c.Aisle.Name)
                .ThenBy(c => c.CellNumber)
                .Select(c => new Enumeration(
                    c.Id,
                    $"{c.Aisle.Area.Name} - {c.Aisle.Name} - Cell {c.CellNumber} (Floor {c.Floor}, Column {c.Column}, {c.Side})")); // TODO: localize string
        }

        public async Task<CellDetails> GetByIdAsync(int id)
        {
            var dc = this.dataContext.Current;

            var cellDetails = await dc.Cells
                .Where(c => c.Id == id)
                .Include(c => c.Aisle)
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
                    Number = c.CellNumber,
                    Priority = c.Priority,
                    Side = (Side)c.Side,
                    XCoordinate = c.XCoordinate,
                    YCoordinate = c.YCoordinate,
                    ZCoordinate = c.ZCoordinate,
                })
                .SingleAsync();

            cellDetails.AbcClassChoices = await this.abcClassProvider.GetAllAsync();
            cellDetails.AisleChoices = this.enumerationProvider.GetAislesByAreaId(cellDetails.AreaId);
            cellDetails.CellStatusChoices = await this.cellStatusProvider.GetAllAsync();
            cellDetails.CellTypeChoices = await this.cellTypeProvider.GetAllAsync();

            return cellDetails;
        }

        public CellDetails GetNew()
        {
            throw new NotImplementedException();
        }

        public IQueryable<Cell> GetWithClassA()
        {
            return GetAllCellsWithFilter(this.dataContext.Current, ClassAFilter);
        }

        public int GetWithClassACount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Cells.AsNoTracking().Count(ClassAFilter);
            }
        }

        public IQueryable<Cell> GetWithStatusEmpty()
        {
            return GetAllCellsWithFilter(this.dataContext.Current, StatusEmptyFilter);
        }

        public int GetWithStatusEmptyCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Cells.AsNoTracking().Count(StatusEmptyFilter);
            }
        }

        public IQueryable<Cell> GetWithStatusFull()
        {
            return GetAllCellsWithFilter(this.dataContext.Current, StatusFullFilter);
        }

        public int GetWithStatusFullCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Cells.AsNoTracking().Count(StatusFullFilter);
            }
        }

        public bool HasAnyLoadingUnits(int cellId)
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.LoadingUnits.AsNoTracking().Any(l => l.CellId == cellId);
            }
        }

        public async Task<IOperationResult> SaveAsync(CellDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                using (var dc = this.dataContext.Current)
                {
                    var existingModel = dc.Cells.Find(model.Id);

                    dc.Entry(existingModel).CurrentValues.SetValues(model);

                    var changedEntityCount = await dc.SaveChangesAsync();

                    return new OperationResult(changedEntityCount > 0);
                }
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        private static IQueryable<Cell> GetAllCellsWithFilter(DatabaseContext context, Expression<Func<DataModels.Cell, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.Cells
               .AsNoTracking()
               .Include(c => c.AbcClass)
               .Include(c => c.Aisle)
               .ThenInclude(a => a.Area)
               .Include(c => c.CellStatus)
               .Include(c => c.CellType)
               .Where(actualWhereFunc)
               .GroupJoin(
                    context.LoadingUnits
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
                    (a, b) => new Cell
                    {
                        Id = a.Cell.Id,
                        AbcClassDescription = a.Cell.AbcClass.Description,
                        AisleName = a.Cell.Aisle.Name,
                        AreaName = a.Cell.Aisle.Area.Name,
                        Column = a.Cell.Column,
                        Floor = a.Cell.Floor,
                        Number = a.Cell.CellNumber,
                        Priority = a.Cell.Priority,
                        Side = (Side)a.Cell.Side,
                        Status = a.Cell.CellStatus.Description,
                        Type = a.Cell.CellType.Description,
                        XCoordinate = a.Cell.XCoordinate,
                        YCoordinate = a.Cell.YCoordinate,
                        ZCoordinate = a.Cell.ZCoordinate,
                        LoadingUnitsCount = b != null ? b.LoadingUnitsCount : 0,
                        LoadingUnitsDescription = b != null ? b.LoadingUnitsDescription : string.Empty,
                    });
        }

        #endregion
    }
}
