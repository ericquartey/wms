using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core
{
    public class SchedulerRequestProvider : ISchedulerRequestProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContext;

        #endregion Fields

        #region Constructors

        public SchedulerRequestProvider(IDatabaseContextService dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion Constructors

        #region Methods

        public async Task<SchedulerRequest> FullyQualifyWithdrawalRequest(SchedulerRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Type != OperationType.Withdrawal)
            {
                throw new ArgumentException("Only withdrawal requests are supported.", nameof(request));
            }

            var dataContext = this.dataContext.Current;
            var aggregatedCompartments = dataContext.Compartments
               .Include(c => c.LoadingUnit)
               .ThenInclude(l => l.Cell)
               .ThenInclude(c => c.Aisle)
               .ThenInclude(a => a.Area)
               .Where(c =>
                   c.ItemId == request.ItemId
                   &&
                   c.LoadingUnit.Cell.Aisle.Area.Id == request.AreaId
                   &&
                   (request.BayId.HasValue == false || c.LoadingUnit.Cell.Aisle.Area.Bays.Any(b => b.Id == request.BayId))
                   &&
                   (request.Sub1 == null || c.Sub1 == request.Sub1)
                   &&
                   (request.Sub2 == null || c.Sub2 == request.Sub2)
                   &&
                   (request.Lot == null || c.Lot == request.Lot)
                   &&
                   (request.PackageTypeId.HasValue == false || c.PackageTypeId == request.PackageTypeId)
                   &&
                   (request.MaterialStatusId.HasValue == false || c.MaterialStatusId == request.MaterialStatusId)
                   &&
                   (request.RegistrationNumber == null || c.RegistrationNumber == request.RegistrationNumber)
               )
               .GroupBy(
                   x => new Tuple<string, string, string, int?, int?, string>(x.Sub1, x.Sub2, x.Lot, x.PackageTypeId, x.MaterialStatusId, x.RegistrationNumber),
                   (key, group) => new
                   {
                       Key = key,
                       Availability = group.Sum(c => c.Stock - c.ReservedForPick + c.ReservedToStore),
                       Sub1 = key.Item1,
                       Sub2 = key.Item2,
                       Lot = key.Item3,
                       PackageTypeId = key.Item4,
                       MaterialStatusId = key.Item5,
                       RegistrationNumber = key.Item6,
                       FirstStoreDate = group.Min(c => c.FirstStoreDate)
                   }
               );

            var aggregatedRequests = dataContext.SchedulerRequests
                .Where(r => r.ItemId == request.ItemId);

            var compartmentSets = aggregatedCompartments
                .GroupJoin(
                    aggregatedRequests,
                    c => new Tuple<string, string, string, int?, int?, string>(c.Sub1, c.Sub2, c.Lot, c.PackageTypeId, c.MaterialStatusId, c.RegistrationNumber),
                    r => new Tuple<string, string, string, int?, int?, string>(r.Sub1, r.Sub2, r.Lot, r.PackageTypeId, r.MaterialStatusId, r.RegistrationNumber),
                    (c, r) => new
                    {
                        c = c,
                        r = r.DefaultIfEmpty()
                    }
                )
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
                }
                )
                .Where(x => x.Availability >= request.RequestedQuantity);

            var item = await dataContext.Items
                .Select(i => new { i.Id, i.ManagementType })
                .SingleAsync(i => i.Id == request.ItemId);

            var orderedCompartmentSets = this.OrderCompartmentsByManagementType(compartmentSets, (ItemManagementType)item.ManagementType);

            return await orderedCompartmentSets
                  .Select(
                  c => new SchedulerRequest(request)
                  {
                      Lot = c.Lot,
                      MaterialStatusId = c.MaterialStatusId,
                      PackageTypeId = c.PackageTypeId,
                      RegistrationNumber = c.RegistrationNumber,
                      Sub1 = c.Sub1,
                      Sub2 = c.Sub2
                  }
              )
              .FirstOrDefaultAsync();
        }

        public IQueryable<Compartment> GetCandidateWithdrawalCompartments(SchedulerRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Type != OperationType.Withdrawal)
            {
                throw new ArgumentException("Only withdrawal requests are supported.", nameof(request));
            }

            return this.dataContext.Current.Compartments
                .Include(c => c.LoadingUnit)
                .ThenInclude(l => l.Cell)
                .ThenInclude(c => c.Aisle)
                .ThenInclude(a => a.Area)
                .ThenInclude(a => a.Bays)
                .Where(c =>
                    c.ItemId == request.ItemId
                    &&
                    c.Lot == request.Lot
                    &&
                    c.MaterialStatusId == request.MaterialStatusId
                    &&
                    c.MaterialStatusId == request.PackageTypeId
                    &&
                    c.RegistrationNumber == request.RegistrationNumber
                    &&
                    c.Sub1 == request.Sub1
                    &&
                    c.Sub2 == request.Sub2
                    &&
                    (c.Stock - c.ReservedForPick + c.ReservedToStore) > 0
                    &&
                    (request.BayId.HasValue == false || c.LoadingUnit.Cell.Aisle.Area.Bays.Any(b => b.Id == request.BayId))
                    &&
                    (c.LoadingUnit.Cell.Aisle.AreaId == request.AreaId)
                    )
                .Select(c => new Compartment
                {
                    Id = c.Id,
                    ItemId = c.ItemId.Value,
                    LoadingUnitId = c.LoadingUnit.Id,
                    CellId = c.LoadingUnit.Cell.Id,
                    AreaId = c.LoadingUnit.Cell.Aisle.Area.Id,
                    Code = c.Code,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    FifoTime = c.FifoTime,
                    FirstStoreDate = c.FirstStoreDate,
                    Stock = c.Stock,
                    ReservedToStore = c.ReservedToStore,
                    ReservedForPick = c.ReservedForPick,
                    PackageTypeId = c.PackageTypeId,
                    Lot = c.Lot,
                    Bays = c.LoadingUnit.Cell.Aisle.Area.Bays.Select(
                        b => new Bay
                        {
                            Id = b.Id,
                            LoadingUnitsBufferSize = b.LoadingUnitsBufferSize
                        }
                        ),
                });
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
                    throw new Exception($"Unable to interpret enumeration value for {nameof(ItemManagementType)}");
            }
        }

        public int Save(SchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                var existingModel = dataContext.Areas.Find(model.Id);

                dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return dataContext.SaveChanges();
            }
        }

        #endregion Methods

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

            #endregion Properties
        };

        #endregion Classes
    }
}
