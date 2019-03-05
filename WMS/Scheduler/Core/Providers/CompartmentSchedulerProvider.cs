using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    public class CompartmentSchedulerProvider : ICompartmentSchedulerProvider
    {
        #region Fields

        private readonly DatabaseContext databaseContext;

        #endregion

        #region Constructors

        public CompartmentSchedulerProvider(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        #endregion

        #region Methods

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

            return this.databaseContext.Compartments
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

        public async Task<Compartment> UpdateAsync(Compartment compartment)
        {
            if (compartment == null)
            {
                throw new ArgumentNullException(nameof(compartment));
            }

            var existingModel = this.databaseContext.Compartments.Find(compartment.Id);
            this.databaseContext.Entry(existingModel).CurrentValues.SetValues(compartment);

            await this.databaseContext.SaveChangesAsync();

            return compartment;
        }

        #endregion
    }
}
