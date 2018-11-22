using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class SchedulerRequestProvider : ISchedulerRequestProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public SchedulerRequestProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion Constructors

        #region Methods

        public async Task<int> Add(SchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            this.dataContext.SchedulerRequests.Add(new DataModels.SchedulerRequest
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
                OperationType = (DataModels.OperationType)(int)model.Type,
                RequestedQuantity = model.RequestedQuantity,
                Sub1 = model.Sub1,
                Sub2 = model.Sub2
            });

            return await this.dataContext.SaveChangesAsync();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<SchedulerRequest> FullyQualifyWithdrawalRequest(SchedulerRequest schedulerRequest)
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
                   (schedulerRequest.RegistrationNumber == null || c.RegistrationNumber == schedulerRequest.RegistrationNumber)
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

            var aggregatedRequests = this.dataContext.SchedulerRequests
                .Where(r => r.ItemId == schedulerRequest.ItemId);

            var matches = aggregatedCompartments
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
                .Select(g => new
                {
                    ActualAvailability = g.c.Availability - g.r.Sum(r => r.RequestedQuantity),
                    g.c.Sub1,
                    g.c.Sub2,
                    g.c.Lot,
                    g.c.PackageTypeId,
                    g.c.MaterialStatusId,
                    g.c.RegistrationNumber,
                    g.c.FirstStoreDate
                }
                );

            var matchesWithAvailability = matches.Where(x => x.ActualAvailability >= schedulerRequest.RequestedQuantity);

            var item = await this.dataContext.Items
                .Select(i => new { i.Id, i.ManagementType })
                .SingleAsync(i => i.Id == schedulerRequest.ItemId);

            IOrderedQueryable<CompartmentSet> orderedCompartmentSets;

            switch ((ItemManagementType)item.ManagementType)
            {
                case ItemManagementType.FIFO:
                    orderedCompartmentSets = compartmentSets
                        .OrderBy(c => c.FirstStoreDate)
                        .ThenBy(c => c.ActualAvailability);
                    break;

                case ItemManagementType.Volume:
                    orderedCompartmentSets = compartmentSets
                        .OrderBy(c => c.ActualAvailability)
                        .ThenBy(c => c.FirstStoreDate);
                    break;

                default:
                    orderedCompartmentSets = null;
                    break;
            }

            if (orderedCompartmentSets != null)
            {
                return await orderedCompartmentSets.Select(
                    c => new SchedulerRequest(schedulerRequest)
                    {
                        Lot = c.Lot,
                        MaterialStatusId = c.MaterialStatusId,
                        PackageTypeId = c.PackageTypeId,
                        RegistrationNumber = c.RegistrationNumber,
                        Sub1 = c.Sub1,
                        Sub2 = c.Sub2
                    }
                )
                .FirstAsync();
            }

            return null;
        }

        public IQueryable<SchedulerRequest> GetAll()
        {
            return this.dataContext.SchedulerRequests
                .Select(s =>
                    new SchedulerRequest
                    {
                        Id = s.Id,
                        AreaId = s.AreaId,
                        BayId = s.BayId,
                        CreationDate = s.CreationDate,
                        ItemId = s.ItemId,
                        IsInstant = s.IsInstant,
                        LoadingUnitId = s.LoadingUnitId,
                        LoadingUnitTypeId = s.LoadingUnitTypeId,
                        MaterialStatusId = s.MaterialStatusId,
                        Lot = s.Lot,
                        Type = (OperationType)s.OperationType,
                        PackageTypeId = s.PackageTypeId,
                        RegistrationNumber = s.RegistrationNumber,
                        RequestedQuantity = s.RequestedQuantity,
                        Sub1 = s.Sub1,
                        Sub2 = s.Sub2
                    });
        }

        public int GetAllCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SchedulerRequests.Count();
            }
        }

        public SchedulerRequest GetById(int id)
        {
            return this.dataContext.SchedulerRequests
                .Where(s => s.Id == id)
                .Select(s =>
                    new SchedulerRequest
                    {
                        Id = s.Id,
                        IsInstant = s.IsInstant,
                        AreaId = s.AreaId,
                        BayId = s.BayId,
                        CreationDate = s.CreationDate,
                        ItemId = s.ItemId,
                        LoadingUnitId = s.LoadingUnitId,
                        LoadingUnitTypeId = s.LoadingUnitTypeId,
                        MaterialStatusId = s.MaterialStatusId,
                        Lot = s.Lot,
                        Type = (OperationType)s.OperationType,
                        PackageTypeId = s.PackageTypeId,
                        RegistrationNumber = s.RegistrationNumber,
                        RequestedQuantity = s.RequestedQuantity,
                        Sub1 = s.Sub1,
                        Sub2 = s.Sub2
                    })
                .Single();
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

        #endregion Methods

        #region Classes

        private class CompartmentSet
        {
            #region Fields

            public int ActualAvailability;
            public DateTime? FirstStoreDate;
            public string Lot;
            public int? MaterialStatusId;
            public int? PackageTypeId;
            public string RegistrationNumber;
            public string Sub1;
            public string Sub2;

            #endregion Fields
        };

        #endregion Classes
    }
}
