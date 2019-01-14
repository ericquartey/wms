using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class BayProvider : IBayProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContext;

        #endregion Fields

        #region Constructors

        public BayProvider(IDatabaseContextService context)
        {
            this.dataContext = context;
        }

        #endregion Constructors

        #region Methods

        public Task<OperationResult> AddAsync(Bay model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<Bay> GetAll()
        {
            return GetAllBaysWithFilter(this.dataContext.Current);
        }

        public int GetAllCount()
        {
            return this.dataContext.Current.Bays.AsNoTracking().Count();
        }

        public IQueryable<Bay> GetByAreaId(int id)
        {
            return this.dataContext.Current.Bays
                .Include(b => b.BayType)
                .Include(b => b.Area)
                .Include(b => b.Machine)
                .Where(b => b.AreaId == id)
                .Select(b => new Bay
                {
                    Id = b.Id,
                    Description = b.Description,
                    LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                    BayTypeId = b.BayTypeId,
                    BayTypeDescription = b.BayType.Description,
                    AreaId = b.AreaId,
                    AreaName = b.Area.Name,
                    MachineId = b.MachineId,
                    MachineNickname = b.Machine.Nickname,
                });
        }

        public async Task<Bay> GetByIdAsync(int id)
        {
            return await this.dataContext.Current.Bays
                   .Include(b => b.BayType)
                   .Include(b => b.Area)
                   .Include(b => b.Machine)
                   .Where(b => b.Id == id)
                   .Select(b => new Bay
                   {
                       Id = b.Id,
                       Description = b.Description,
                       LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                       BayTypeId = b.BayTypeId,
                       BayTypeDescription = b.BayType.Description,
                       AreaId = b.AreaId,
                       AreaName = b.Area.Name,
                       MachineId = b.MachineId,
                       MachineNickname = b.Machine.Nickname,
                   })
                   .SingleAsync();
        }

        public async Task<OperationResult> SaveAsync(Bay model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                using (var dc = this.dataContext.Current)
                {
                    var existingModel = dc.Bays.Find(model.Id);

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

        private static IQueryable<Bay> GetAllBaysWithFilter(
            DatabaseContext context,
            Expression<Func<DataModels.Bay, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.Bays
                .AsNoTracking()
                .Include(b => b.BayType)
                .Include(b => b.Area)
                .Include(b => b.Machine)
                .Where(actualWhereFunc)
                .Select(b => new Bay
                {
                    Id = b.Id,
                    Description = b.Description,
                    LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                    BayTypeId = b.BayTypeId,
                    BayTypeDescription = b.BayType.Description,
                    AreaId = b.AreaId,
                    AreaName = b.Area.Name,
                    MachineId = b.MachineId,
                    MachineNickname = b.Machine.Nickname,
                });
        }

        #endregion Methods
    }
}
