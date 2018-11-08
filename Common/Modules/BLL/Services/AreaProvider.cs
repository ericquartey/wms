using System;
using System.Linq;
using System.Linq.Expressions;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class AreaProvider : IAreaProvider
    {
        #region Methods

        public IQueryable<Area> GetAll()
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            return GetAllAreasWithFilter(context);
        }

        public int GetAllCount()
        {
            using (var context = ServiceLocator.Current.GetInstance<DatabaseContext>())
            {
                return context.Areas.AsNoTracking().Count();
            }
        }

        public Area GetById(int id)
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            var areaDetails = context.Areas
                .Where(a => a.Id == id)
                .Select(a => new Area
                {
                    Id = a.Id,
                    Name = a.Name,
                })
                .Single();

            return areaDetails;
        }

        public IQueryable<Area> GetByItemIdAvailability(int id)
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            var areaDetails = context.Compartments
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

        public int Save(Area model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            using (var context = ServiceLocator.Current.GetInstance<DatabaseContext>())
            {
                var existingModel = context.Areas.Find(model.Id);

                context.Entry(existingModel).CurrentValues.SetValues(model);

                return context.SaveChanges();
            }
        }

        private static IQueryable<Area> GetAllAreasWithFilter(DatabaseContext context,
            Expression<Func<DataModels.Area, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.Areas
                .AsNoTracking()
                .Where(actualWhereFunc)
                .Select(a => new Area
                {
                    Id = a.Id,
                    Name = a.Name,
                });
        }

        #endregion Methods
    }
}
