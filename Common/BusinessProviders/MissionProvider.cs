using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class MissionProvider : IMissionProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContext;

        #endregion Fields

        #region Constructors

        public MissionProvider(IDatabaseContextService context)
        {
            this.dataContext = context;
        }

        #endregion Constructors

        #region Methods

        public Task<int> Add(Mission model)
        {
            throw new NotImplementedException();
        }

        public async Task<int> AddRange(IEnumerable<Mission> missions)
        {
            if (missions == null)
            {
                throw new ArgumentNullException(nameof(missions));
            }

            var dataContext = this.dataContext.Current;
            dataContext.AddRange(missions.Select(m => new DataModels.Mission
            {
                CompartmentId = m.CompartmentId,
                // BayId = m.BayId,  // TODO: remove destination/source bay id
                // CellId = m.CellId,        // TODO: remove destination/source cell id
                ItemId = m.ItemId,
                ItemListId = m.ItemListId,
                ItemListRowId = m.ItemListRowId,
                LoadingUnitId = m.LoadingUnitId,
                MaterialStatusId = m.MaterialStatusId,
                Type = (DataModels.MissionType)m.Type,
                PackageTypeId = m.PackageTypeId,
                Sub1 = m.Sub1,
                Sub2 = m.Sub2,
            }));

            return await dataContext.SaveChangesAsync();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Mission> GetAll()
        {
            throw new NotImplementedException();
        }

        public int GetAllCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.Cells.AsNoTracking().Count();
            }
        }

        public Mission GetById(int id)
        {
            throw new NotImplementedException();
        }

        public int Save(Mission model)
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

        #endregion Methods
    }
}
