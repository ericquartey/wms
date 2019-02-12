using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core
{
    internal class SchedulerRequestProvider : ISchedulerRequestProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public SchedulerRequestProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public Task CreateRangeAsync(IEnumerable<SchedulerRequest> requests)
        {
            throw new NotImplementedException();
        }

        public async Task<SchedulerRequest> FullyQualifyWithdrawalRequestAsync(SchedulerRequest schedulerRequest)
        {
            if (schedulerRequest == null)
            {
                throw new ArgumentNullException(nameof(schedulerRequest));
            }

            if (schedulerRequest.Type != OperationType.Withdrawal)
            {
                throw new ArgumentException("Only withdrawal requests are supported.", nameof(schedulerRequest));
            }

            var aggregatedCompartments = this.dataContext.Compartments
                .Include(c => c.LoadingUnit)
                .ThenInclude(l => l.Cell)
                .ThenInclude(c => c.Aisle)
                .ThenInclude(a => a.Area)
                .Where(c =>
                    c.ItemId == schedulerRequest.ItemId
                    &&
                    c.LoadingUnit.Cell.Aisle.Area.Id == schedulerRequest.AreaId
                    &&
                    (schedulerRequest.BayId.HasValue == false || c.LoadingUnit.Cell.Aisle.Area.Bays.Any(b => b.Id == schedulerRequest.BayId))
                    &&
                    (schedulerRequest.Sub1 == null || c.Sub1 == schedulerRequest.Sub1)
                    &&
                    (schedulerRequest.Sub2 == null || c.Sub2 == schedulerRequest.Sub2)
                    &&
                    (schedulerRequest.Lot == null || c.Lot == schedulerRequest.Lot)
                    &&
                    (schedulerRequest.PackageTypeId.HasValue == false || c.PackageTypeId == schedulerRequest.PackageTypeId)
                    &&
                    (schedulerRequest.MaterialStatusId.HasValue == false || c.MaterialStatusId == schedulerRequest.MaterialStatusId)
                    &&
                    (schedulerRequest.RegistrationNumber == null || c.RegistrationNumber == schedulerRequest.RegistrationNumber))
                .GroupBy(
                    x => new { x.Sub1, x.Sub2, x.Lot, x.PackageTypeId, x.MaterialStatusId, x.RegistrationNumber },
                    (key, group) => new
                    {
                        Key = key,
                        Availability = group.Sum(c => c.Stock - c.ReservedForPick + c.ReservedToStore),
                        Sub1 = key.Sub1,
                        Sub2 = key.Sub2,
                        Lot = key.Lot,
                        PackageTypeId = key.PackageTypeId,
                        MaterialStatusId = key.MaterialStatusId,
                        RegistrationNumber = key.RegistrationNumber,
                        FirstStoreDate = group.Min(c => c.FirstStoreDate)
                    });

            var aggregatedRequests = this.dataContext.SchedulerRequests
                .Where(r => r.ItemId == schedulerRequest.ItemId);

            var compartmentSets = aggregatedCompartments
                .GroupJoin(
                    aggregatedRequests,
                    c => new { c.Sub1, c.Sub2, c.Lot, c.PackageTypeId, c.MaterialStatusId, c.RegistrationNumber },
                    r => new { r.Sub1, r.Sub2, r.Lot, r.PackageTypeId, r.MaterialStatusId, r.RegistrationNumber },
                    (c, r) => new
                    {
                        c,
                        r = r.DefaultIfEmpty()
                    })
                .Select(g => new CompartmentSet
                {
                    Availability = g.c.Availability - g.r.Sum(r => r.RequestedQuantity - r.DispatchedQuantity),
                    Sub1 = g.c.Sub1,
                    Sub2 = g.c.Sub2,
                    Lot = g.c.Lot,
                    PackageTypeId = g.c.PackageTypeId,
                    MaterialStatusId = g.c.MaterialStatusId,
                    RegistrationNumber = g.c.RegistrationNumber,
                    FirstStoreDate = g.c.FirstStoreDate
                })
                .Where(x => x.Availability >= schedulerRequest.RequestedQuantity);

            var item = await this.dataContext.Items
                .Select(i => new { i.Id, i.ManagementType })
                .SingleAsync(i => i.Id == schedulerRequest.ItemId);

            var orderedCompartmentSets = this.OrderCompartmentsByManagementType(compartmentSets, (ItemManagementType)item.ManagementType);

            return await orderedCompartmentSets
                  .Select(
                  c => new SchedulerRequest(schedulerRequest)
                  {
                      Lot = c.Lot,
                      MaterialStatusId = c.MaterialStatusId,
                      PackageTypeId = c.PackageTypeId,
                      RegistrationNumber = c.RegistrationNumber,
                      Sub1 = c.Sub1,
                      Sub2 = c.Sub2
                  })
              .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets all compartments in the specified area/bay that have availability for the specified item.
        /// </summary>
        /// <param name="schedulerRequest"></param>
        /// <returns>The unsorted set of compartments matching the specified request.</returns>
        public IQueryable<Compartment> GetCandidateWithdrawalCompartments(SchedulerRequest schedulerRequest)
        {
            if (schedulerRequest == null)
            {
                throw new ArgumentNullException(nameof(schedulerRequest));
            }

            if (schedulerRequest.Type != OperationType.Withdrawal)
            {
                throw new ArgumentException("Only withdrawal requests are supported.", nameof(schedulerRequest));
            }

            return this.dataContext.Compartments
                .Include(c => c.LoadingUnit)
                .ThenInclude(l => l.Cell)
                .ThenInclude(c => c.Aisle)
                .ThenInclude(a => a.Area)
                .ThenInclude(a => a.Bays)
                .Where(c =>
                    c.ItemId == schedulerRequest.ItemId
                    &&
                    c.Lot == schedulerRequest.Lot
                    &&
                    c.MaterialStatusId == schedulerRequest.MaterialStatusId
                    &&
                    c.MaterialStatusId == schedulerRequest.PackageTypeId
                    &&
                    c.RegistrationNumber == schedulerRequest.RegistrationNumber
                    &&
                    c.Sub1 == schedulerRequest.Sub1
                    &&
                    c.Sub2 == schedulerRequest.Sub2
                    &&
                    (c.Stock - c.ReservedForPick + c.ReservedToStore) > 0
                    &&
                    (schedulerRequest.BayId.HasValue == false || c.LoadingUnit.Cell.Aisle.Area.Bays.Any(b => b.Id == schedulerRequest.BayId))
                    &&
                    (c.LoadingUnit.Cell.Aisle.AreaId == schedulerRequest.AreaId))
                .Select(c => new Compartment
                {
                    AreaId = c.LoadingUnit.Cell.Aisle.AreaId,
                    CellId = c.LoadingUnit.CellId,
                    FifoTime = c.FifoTime,
                    FirstStoreDate = c.FirstStoreDate,
                    Id = c.Id,
                    ItemId = c.ItemId.Value,
                    LoadingUnitId = c.LoadingUnitId,
                    Lot = c.Lot,
                    MaterialStatusId = c.MaterialStatusId,
                    PackageTypeId = c.PackageTypeId,
                    RegistrationNumber = c.RegistrationNumber,
                    ReservedForPick = c.ReservedForPick,
                    ReservedToStore = c.ReservedToStore,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                });
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
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task<IEnumerable<SchedulerRequest>> GetRequestsToProcessAsync()
        {
            return await this.dataContext.SchedulerRequests
               .Include(r => r.List)
               .Include(r => r.ListRow)
               .Include(r => r.Bay)
               .ThenInclude(b => b.Missions)
               .Where(r =>
                    r.BayId.HasValue
                    &&
                    r.RequestedQuantity > r.DispatchedQuantity
                    &&
                    r.Bay.LoadingUnitsBufferSize > r.Bay.Missions.Count
                    &&
                    (r.ListRowId.HasValue == false || r.ListRow.Status == Common.DataModels.ItemListRowStatus.Executing)
                    &&
                    (r.ListId.HasValue == false || r.List.Status == Common.DataModels.ItemListStatus.Executing))
               .OrderBy(r => r.ListId.HasValue ? r.List.Priority : int.MaxValue)
               .ThenBy(r => r.ListRowId.HasValue ? r.ListRow.Priority : int.MaxValue)
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
                   ListRowId = r.ListRowId,
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
               .ToArrayAsync();
        }

        public IQueryable<T> OrderCompartmentsByManagementType<T>(IQueryable<T> compartments, ItemManagementType type)
                where T : IOrderableCompartment
        {
            switch (type)
            {
                case ItemManagementType.FIFO:
                    return compartments
                        .OrderBy(c => c.FirstStoreDate)
                        .ThenBy(c => c.Availability);

                case ItemManagementType.Volume:
                    return compartments
                        .OrderBy(c => c.Availability)
                        .ThenBy(c => c.FirstStoreDate);

                default:
                    throw new ArgumentException(
                        $"Unable to interpret enumeration value for {nameof(ItemManagementType)}",
                        nameof(type));
            }
        }

        public int Save(SchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (this.dataContext)
            {
                var existingModel = this.dataContext.Areas.Find(model.Id);

                this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return this.dataContext.SaveChanges();
            }
        }

        #endregion

        #region Classes

        private class CompartmentSet : IOrderableCompartment
        {
            #region Properties

            public int Availability { get; set; }

            public DateTime? FirstStoreDate { get; set; }

            public string Lot { get; set; }

            public int? MaterialStatusId { get; set; }

            public int? PackageTypeId { get; set; }

            public string RegistrationNumber { get; set; }

            public string Sub1 { get; set; }

            public string Sub2 { get; set; }

            #endregion
        }

        #endregion
    }
}
