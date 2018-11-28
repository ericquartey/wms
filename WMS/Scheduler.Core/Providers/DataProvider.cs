using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core
{
    public class DataProvider : IDataProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public DataProvider(DatabaseContext context)
        {
            this.dataContext = context;
        }

        #endregion Constructors

        #region Methods

        public async Task<int> AddAsync(SchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            this.dataContext.SchedulerRequests.Add(new Common.DataModels.SchedulerRequest
            {
                AreaId = model.AreaId,
                BayId = model.BayId,
                IsInstant = model.IsInstant,
                ItemId = model.ItemId,
                ListId = model.ListId,
                ListRowId = model.ListRowId,
                LoadingUnitId = model.LoadingUnitId,
                LoadingUnitTypeId = model.LoadingUnitTypeId,
                Lot = model.Lot,
                MaterialStatusId = model.MaterialStatusId,
                PackageTypeId = model.PackageTypeId,
                RegistrationNumber = model.RegistrationNumber,
                OperationType = (Common.DataModels.OperationType)(int)model.Type,
                RequestedQuantity = model.RequestedQuantity,
                Sub1 = model.Sub1,
                Sub2 = model.Sub2
            });

            return await this.dataContext.SaveChangesAsync();
        }

        public async Task<int> AddRangeAsync(IEnumerable<Mission> missions)
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

        public async Task<SchedulerRequest> GetNextRequestToProcessAsync()
        {
            return await this.dataContext.SchedulerRequests
                .OrderBy(s => new { s.IsInstant, s.CreationDate })
                .Select(r => new SchedulerRequest
                {
                    Id = r.Id,
                    AreaId = r.AreaId,
                    BayId = r.BayId,
                    CreationDate = r.CreationDate,
                    IsInstant = r.IsInstant,
                    ItemId = r.ItemId,
                    ListId = r.ListId,
                    ListRowId = r.LoadingUnitId,
                    LoadingUnitId = r.LoadingUnitId,
                    LoadingUnitTypeId = r.LoadingUnitTypeId,
                    Lot = r.Lot,
                    Type = (OperationType)r.OperationType,
                    MaterialStatusId = r.MaterialStatusId,
                    PackageTypeId = r.PackageTypeId,
                    RegistrationNumber = r.RegistrationNumber,
                    RequestedQuantity = r.RequestedQuantity,
                    Sub1 = r.Sub1,
                    Sub2 = r.Sub2
                })
                .FirstOrDefaultAsync();
        }

        public int Save(Mission model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

        public void Update(Compartment compartment)
        {
            if (compartment == null)
            {
                throw new ArgumentNullException(nameof(compartment));
            }

            var existingModel = this.dataContext.Compartments.Find(compartment.Id);
            this.dataContext.Entry(existingModel).CurrentValues.SetValues(compartment);

            this.dataContext.SaveChanges();
        }

        public void Update(SchedulerRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var existingModel = this.dataContext.SchedulerRequests.Find(request.Id);
            this.dataContext.Entry(existingModel).CurrentValues.SetValues(request);

            this.dataContext.SaveChanges();
        }

        #endregion Methods
    }
}
