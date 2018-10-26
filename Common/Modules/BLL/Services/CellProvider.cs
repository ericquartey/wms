using System;
using System.Linq;
using System.Linq.Expressions;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class CellProvider : ICellProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public CellProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;

            //TODO: use interface for EnumerationProvider
            this.enumerationProvider = new EnumerationProvider(dataContext);
        }

        #endregion Constructors

        #region Methods

        public IQueryable<Cell> GetAll()
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            return GetAllCellsWithFilter(context);
        }

        public int GetAllCount()
        {
            return this.dataContext.Cells.AsNoTracking().Count();
        }

        public CellDetails GetById(int id)
        {
            var cellDetails = this.dataContext.Cells
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
                    Side = (int) c.Side,
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

        public bool HasAnyLoadingUnits(int cellId)
        {
            return this.dataContext.LoadingUnits.AsNoTracking().Any(l => l.CellId == cellId);
        }

        public int Save(CellDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.Cells.Find(model.Id);

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

            return this.dataContext.SaveChanges();
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
