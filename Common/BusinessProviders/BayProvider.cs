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

        public Task<OperationResult> Add(Bay model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

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

        public async Task<Bay> GetById(int id)
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

        public int Save(Bay model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                var existingModel = dataContext.Bays.Find(model.Id);

                dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return dataContext.SaveChanges();
            }
        }

        private static IQueryable<Bay> GetAllBaysWithFilter(DatabaseContext context,
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
