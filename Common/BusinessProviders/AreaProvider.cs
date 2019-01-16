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

        #endregion Fields

        #region Constructors

        public AreaProvider(IDatabaseContextService dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion Constructors

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

        public async Task<OperationResult> SaveAsync(Area model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                using (var dc = this.dataContext.Current)
                {
                    var existingModel = dc.Areas.Find(model.Id);

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

        #endregion Methods
    }
}
