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

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public BayProvider(DatabaseContext context)
        {
            this.dataContext = context;
        }

        #endregion Constructors

        #region Methods

        public Task<int> Add(Bay model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Bay> GetAll()
        {
            return GetAllBaysWithFilter(this.dataContext);
        }

        public int GetAllCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays.AsNoTracking().Count();
            }
        }

        public IQueryable<Bay> GetByAreaId(int id)
        {
            return this.dataContext.Bays
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

        public Bay GetById(int id)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays
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
                    .Single();
            }
        }

        public int Save(Bay model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (this.dataContext)
            {
                var existingModel = this.dataContext.Bays.Find(model.Id);

                this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return this.dataContext.SaveChanges();
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
