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

        #endregion

        #region Constructors

        public DataProvider(DatabaseContext context)
        {
            this.dataContext = context;
        }

        #endregion

        #region Methods

        public bool Add(SchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = this.dataContext.SchedulerRequests.Add(
                new Common.DataModels.SchedulerRequest
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

            if (this.dataContext.SaveChanges() > 0)
            {
                model.Id = entry.Entity.Id;
                return true;
            }

            return false;
        }

        public void AddRange(IEnumerable<Mission> missions)
        {
            if (missions == null)
            {
                throw new ArgumentNullException(nameof(missions));
            }

            this.dataContext.AddRange(missions
                .Select(m => new Common.DataModels.Mission
                {
                    CompartmentId = m.CompartmentId,
                    BayId = m.BayId,
                    CellId = m.CellId,
                    ItemId = m.ItemId,
                    ItemListId = m.ItemListId,
                    ItemListRowId = m.ItemListRowId,
                    LoadingUnitId = m.LoadingUnitId,
                    RequiredQuantity = m.Quantity,
                    Lot = m.Lot,
                    MaterialStatusId = m.MaterialStatusId,
                    Type = (Common.DataModels.MissionType)m.Type,
                    PackageTypeId = m.PackageTypeId,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    RegistrationNumber = m.RegistrationNumber
                }));

            this.dataContext.SaveChanges();
        }

        public void AddRange(IEnumerable<SchedulerRequest> requests)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }

            foreach (var request in requests)
            {
                this.Add(request);
            }
        }

        public async Task<Area> GetAreaByIdAsync(int areaId)
        {
            return await this.dataContext.Areas
                .Include(a => a.Bays)
                .ThenInclude(b => b.Missions)
                .Select(a => new Area
                {
                    Id = a.Id,
                    Bays = a.Bays.Select(b => new Bay
                    {
                        Id = b.Id,
                        LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                        LoadingUnitsBufferUsage = b.Missions.Count(m => m.Status != Common.DataModels.MissionStatus.Completed)
                    })
                })
                .SingleAsync(a => a.Id == areaId);
        }

        public async Task<Bay> GetBayByIdAsync(int bayId)
        {
            return await this.dataContext.Bays
                .Include(b => b.Missions)
                .Select(b => new Bay
                {
                    Id = b.Id,
                    LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                    LoadingUnitsBufferUsage = b.Missions.Count(m => m.Status != Common.DataModels.MissionStatus.Completed)
                })
                .SingleAsync(b => b.Id == bayId);
        }

        public async Task<Item> GetItemByIdAsync(int itemId)
        {
            return await this.dataContext.Items
                .Select(i => new Item
                {
                    Id = i.Id,
                    ManagementType = (ItemManagementType)i.ManagementType,
                })
                .SingleAsync(i => i.Id == itemId);
        }

        public void Update(Mission mission)
        {
            if (mission == null)
            {
                throw new ArgumentNullException(nameof(mission));
            }

            var existingModel = this.dataContext.Missions.Find(mission.Id);
            this.dataContext.Entry(existingModel).CurrentValues.SetValues(mission);

            this.dataContext.SaveChanges();
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

        #endregion
    }
}
