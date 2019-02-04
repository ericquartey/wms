using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class AreaProvider : IAreaProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContext;

        #endregion

        #region Constructors

        public AreaProvider(IDatabaseContextService dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public Task<OperationResult> AddAsync(Area model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<Area> GetAll()
        {
            return GetAllAreasWithFilter(this.dataContext.Current);
        }

        public int GetAllCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Areas.AsNoTracking().Count();
            }
        }

        public async Task<Area> GetByIdAsync(int id)
        {
            return await this.dataContext.Current.Areas
                .Where(a => a.Id == id)
                .Select(a => new Area
                {
                    Id = a.Id,
                    Name = a.Name,
                })
                .SingleAsync();
        }

        public IQueryable<Area> GetByItemIdAvailability(int id)
        {
            return this.dataContext.Current.Compartments
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
        }

        public Task<OperationResult> SaveAsync(Area model) => throw new NotSupportedException();

        private static IQueryable<Area> GetAllAreasWithFilter(
            DatabaseContext context,
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

        #endregion
    }
}
