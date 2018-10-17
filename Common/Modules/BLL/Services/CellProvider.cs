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
            throw new NotImplementedException();
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
               .Include(c => c.CellStatus)
               .Include(c => c.CellType)
               .Where(actualWhereFunc)
               .Select(c => new Cell
               {
                   Id = c.Id,
                   AbcClass = c.AbcClass.Description,
                   Column = c.Column,
                   Floor = c.Floor,
                   Number = c.CellNumber,
                   Priority = c.Priority,
                   Side = c.Side.ToString(),
                   Status = c.CellStatus.Description,
                   Type = c.CellType.Description,
                   XCoordinate = c.XCoordinate,
                   YCoordinate = c.YCoordinate,
                   ZCoordinate = c.ZCoordinate
               }
               );
        }

        #endregion Methods
    }
}
