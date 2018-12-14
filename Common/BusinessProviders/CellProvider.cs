using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class CellProvider : ICellProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContext;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public CellProvider(
            IDatabaseContextService context,
            EnumerationProvider enumerationProvider)
        {
            this.dataContext = context;
            this.enumerationProvider = enumerationProvider;
        }

        #endregion Constructors

        #region Methods

        public Task<OperationResult> Add(CellDetails model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Cell> GetAll()
        {
            return GetAllCellsWithFilter(this.dataContext.Current);
        }

        public int GetAllCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.Cells.AsNoTracking().Count();
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
                    $"{c.Aisle.Area.Name} - {c.Aisle.Name} - Cell {c.CellNumber} (Floor {c.Floor}, Column {c.Column}, {c.Side})") //TODO: localize string
                );
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
                    $"{c.Aisle.Area.Name} - {c.Aisle.Name} - Cell {c.CellNumber} (Floor {c.Floor}, Column {c.Column}, {c.Side})") //TODO: localize string
                );
        }

        public CellDetails GetById(int id)
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                var cellDetails = dataContext.Cells
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
                        Side = (int)c.Side,
                        XCoordinate = c.XCoordinate,
                        YCoordinate = c.YCoordinate,
                        ZCoordinate = c.ZCoordinate,
                    })
                    .Single();

                cellDetails.AbcClassChoices = this.enumerationProvider.GetAllAbcClasses();
                cellDetails.AisleChoices = this.enumerationProvider.GetAislesByAreaId(cellDetails.AreaId);
                cellDetails.SideChoices =
                    ((DataModels.Side[])Enum.GetValues(typeof(DataModels.Side)))
                    .Select(i => new Enumeration((int)i, i.ToString())).ToList();
                cellDetails.CellStatusChoices = this.enumerationProvider.GetAllCellStatuses();
                cellDetails.CellTypeChoices = this.enumerationProvider.GetAllCellTypes();

                return cellDetails;
            }
        }

        public bool HasAnyLoadingUnits(int cellId)
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.LoadingUnits.AsNoTracking().Any(l => l.CellId == cellId);
            }
        }

        public int Save(CellDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                var existingModel = dataContext.Cells.Find(model.Id);

                dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return dataContext.SaveChanges();
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
                        SideDescription = a.Cell.Side.ToString(),
                        Status = a.Cell.CellStatus.Description,
                        Type = a.Cell.CellType.Description,
                        XCoordinate = a.Cell.XCoordinate,
                        YCoordinate = a.Cell.YCoordinate,
                        ZCoordinate = a.Cell.ZCoordinate,
                        LoadingUnitsCount = b != null ? b.LoadingUnitsCount : 0,
                        LoadingUnitsDescription = b != null ? b.LoadingUnitsDescription : "",
                    });
        }

        #endregion Methods
    }
}
