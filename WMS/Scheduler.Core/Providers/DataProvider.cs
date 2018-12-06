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

        public void Add(SchedulerRequest model)
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
                }
            );

            if (this.dataContext.SaveChanges() > 0)
            {
                model.Id = entry.Entity.Id;
            }
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
                })
            );

            this.dataContext.SaveChanges();
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
                        LoadingUnitsBufferUsage = b.Missions.Where(m => m.Status != Common.DataModels.MissionStatus.Completed).Count()
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
                    LoadingUnitsBufferUsage = b.Missions.Where(m => m.Status != Common.DataModels.MissionStatus.Completed).Count()
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
                }
                )
                .SingleAsync(i => i.Id == itemId);
        }

        /// <summary>
        /// Gets all the pending requests that:
        /// - are not completed (dispatched qty is not equal to requested qty)
        /// - are already allocated to a bay
        /// - the allocated bay has buffer to accept new missions
        /// - are associated to a list that is in execution
        ///
        /// Requests are sorted by:
        /// - Instant first
        /// - All others after, giving priority to the lists ones that are already started
        ///
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SchedulerRequest>> GetRequestsToProcessAsync()
        {
            return await this.dataContext.SchedulerRequests
               .Where(r => r.BayId.HasValue && r.RequestedQuantity > r.DispatchedQuantity && r.BayId.HasValue)
               .Include(r => r.List)
               .Include(r => r.ListRow)
               .Include(r => r.Bay)
               .ThenInclude(b => b.Missions)
               .Where(r => r.Bay.LoadingUnitsBufferSize > r.Bay.Missions.Count())
               .Select(r => new SchedulerRequest
               {
                   Id = r.Id,
                   AreaId = r.AreaId,
                   BayId = r.BayId,
                   CreationDate = r.CreationDate,
                   IsInstant = r.IsInstant,
                   ListStatus = r.List != null ? (ListStatus)r.List.Status : ListStatus.NotSpecified,
                   ListRowStatus = r.ListRow != null ? (ListRowStatus)r.ListRow.Status : ListRowStatus.NotSpecified,
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
                   DispatchedQuantity = r.DispatchedQuantity,
                   Sub1 = r.Sub1,
                   Sub2 = r.Sub2
               })
               .Where(r => r.ListRowStatus == ListRowStatus.Executing)
               .ToListAsync();
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

        #endregion Methods
    }
}
