using System;
using System.Linq;
using System.Linq.Expressions;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class AreaProvider : IAreaProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public AreaProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion Constructors

        #region Methods

        public IQueryable<Area> GetAll()
        {
            lock (this.dataContext)
            {
                return GetAllAreasWithFilter(this.dataContext);
            }
        }

        public int GetAllCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Areas.AsNoTracking().Count();
            }
        }

        public Area GetById(int id)
        {
            lock (this.dataContext)
            {
                var areaDetails = this.dataContext.Areas
                .Where(a => a.Id == id)
                .Select(a => new Area
                {
                    Id = a.Id,
                    Name = a.Name,
                })
                .Single();

                return areaDetails;
            }
        }

        public IQueryable<Area> GetByItemIdAvailability(int id)
        {
            lock (this.dataContext)
            {
                var areaDetails = this.dataContext.Compartments
                    .Include(c => c.LoadingUnit)
                        .ThenInclude(l => l.Cell)
                        .ThenInclude(c => c.Aisle)
                        .ThenInclude(a => a.Area)
                    .Where(c => c.ItemId == id)
                    .Where(c => (c.Stock - c.ReservedForPick + c.ReservedToStore) > 0)
                    .Select(c => new Area
                    {
                        Id = c.LoadingUnit.Cell.Aisle.AreaId,
                        Name = c.LoadingUnit.Cell.Aisle.Area.Name,
                    })
                    .Distinct();

                return areaDetails;
            }
        }

        public int Save(Area model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (this.dataContext)
            {
                var existingModel = this.dataContext.Areas.Find(model.Id);

                this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return this.dataContext.SaveChanges();
            }
        }

        private static IQueryable<Area> GetAllAreasWithFilter(DatabaseContext context,
            Expression<Func<DataModels.Area, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            lock (context)
            {
                return context.Areas
                    .AsNoTracking()
                    .Where(actualWhereFunc)
                    .Select(a => new Area
                    {
                        Id = a.Id,
                        Name = a.Name,
                    });
            }
        }

        #endregion Methods
    }
}
