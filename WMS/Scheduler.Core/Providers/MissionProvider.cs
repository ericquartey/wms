using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core
{
    public class MissionProvider : IMissionProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public MissionProvider(DatabaseContext context)
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

            this.dataContext.AddRange(missions.Select(m => new Common.DataModels.Mission
            {
                CompartmentId = m.CompartmentId,
                // BayId = m.BayId,  // TODO: remove destination/source bay id
                // CellId = m.CellId,        // TODO: remove destination/source cell id
                ItemId = m.ItemId,
                ItemListId = m.ItemListId,
                ItemListRowId = m.ItemListRowId,
                LoadingUnitId = m.LoadingUnitId,
                MaterialStatusId = m.MaterialStatusId,
                Type = (Common.DataModels.MissionType)m.Type,
                PackageTypeId = m.PackageTypeId,
                Sub1 = m.Sub1,
                Sub2 = m.Sub2,
            }));

            return await this.dataContext.SaveChangesAsync();
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
            lock (this.dataContext)
            {
                return this.dataContext.Cells.AsNoTracking().Count();
            }
        }

        public Mission GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Item> GetItemByIdAsync(int itemId)
        {
            return await this.dataContext.Items
                .Select(i => new Item
                {
                    Id = i.Id,
                }
                )
                .SingleAsync(i => i.Id == itemId);
        }

        public int Save(Mission model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (this.dataContext)
            {
                var existingModel = this.dataContext.Cells.Find(model.Id);

                this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return this.dataContext.SaveChanges();
            }
        }

        #endregion Methods
    }
}
