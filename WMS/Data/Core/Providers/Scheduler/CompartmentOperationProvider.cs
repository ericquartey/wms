using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;
using Compartment = Ferretto.Common.DataModels.Compartment;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class CompartmentOperationProvider : ICompartmentOperationProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public CompartmentOperationProvider(
            DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<CandidateCompartment> GetByIdForStockUpdateAsync(int id)
        {
            return await this.dataContext.Compartments
                .GroupJoin(
                    this.dataContext.ItemsCompartmentTypes,
                    cmp => new { CompartmentTypeId = cmp.CompartmentTypeId, ItemId = cmp.ItemId },
                    ict => new { CompartmentTypeId = ict.CompartmentTypeId, ItemId = (int?)ict.ItemId },
                    (c, ict) => new { c, ict = ict.SingleOrDefault() })
                .Where(j => j.c.Id == id)
                .Select(j => new CandidateCompartment
                {
                    Id = j.c.Id,
                    LastPickDate = j.c.LastPickDate,
                    LastPutDate = j.c.LastPutDate,
                    ItemId = j.c.ItemId,
                    ReservedForPick = j.c.ReservedForPick,
                    ReservedToPut = j.c.ReservedToPut,
                    IsItemPairingFixed = j.c.IsItemPairingFixed,
                    MaxCapacity = j.ict == null ? null : j.ict.MaxCapacity,
                    Stock = j.c.Stock,
                    LoadingUnitId = j.c.LoadingUnitId,
                    CompartmentTypeId = j.c.CompartmentTypeId,
                    Sub1 = j.c.Sub1,
                    Sub2 = j.c.Sub2,
                    MaterialStatusId = j.c.MaterialStatusId,
                    PackageTypeId = j.c.PackageTypeId,
                    RegistrationNumber = j.c.RegistrationNumber,
                })
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Gets all compartments in the specified area/bay that have availability for the specified item.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The unsorted set of compartments matching the specified request.</returns>
        public IQueryable<CandidateCompartment> GetCandidateCompartments(ItemSchedulerRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var compartmentIsInBay = this.GetCompartmentIsInBayFunction(request.BayId);

            var filteredCompartments = this.dataContext.Compartments
                .Where(compartmentIsInBay)
                .Where(c => c.LoadingUnit.Cell.Aisle.AreaId == request.AreaId)
                .Where(c =>
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
                    c.Sub2 == request.Sub2);

            IQueryable<CandidateCompartment> candidateCompartments;
            switch (request.OperationType)
            {
                case OperationType.Withdrawal:
                    candidateCompartments = filteredCompartments
                        .Where(c => c.ItemId == request.ItemId)
                        .Select(c => new CandidateCompartment
                        {
                            AreaId = c.LoadingUnit.Cell.Aisle.AreaId,
                            CellId = c.LoadingUnit.CellId,
                            CompartmentTypeId = c.CompartmentTypeId,
                            FifoStartDate = c.FifoStartDate,
                            Id = c.Id,
                            ItemId = c.ItemId.Value,
                            LoadingUnitId = c.LoadingUnitId,
                            Lot = c.Lot,
                            MaterialStatusId = c.MaterialStatusId,
                            PackageTypeId = c.PackageTypeId,
                            RegistrationNumber = c.RegistrationNumber,
                            ReservedForPick = c.ReservedForPick,
                            ReservedToPut = c.ReservedToPut,
                            Stock = c.Stock,
                            Sub1 = c.Sub1,
                            Sub2 = c.Sub2,
                            IsItemPairingFixed = c.IsItemPairingFixed,
                        })
                        .Where(c => c.Availability > 0);

                    break;

                case OperationType.Insertion:

                    var filteredCompartmentsWithMaxCapacity = filteredCompartments
                        .Join(
                            this.dataContext.ItemsCompartmentTypes
                                .Where(ict => ict.ItemId == request.ItemId),
                            c => c.CompartmentTypeId,
                            ict => ict.CompartmentTypeId,
                            (c, ict) => new { c, ict.MaxCapacity });

                    candidateCompartments = filteredCompartmentsWithMaxCapacity
                       .Where(info => info.c.ItemId == request.ItemId || info.c.ItemId == null)
                       .Select(info => new CandidateCompartment
                       {
                           AreaId = info.c.LoadingUnit.Cell.Aisle.AreaId,
                           MaxCapacity = info.MaxCapacity,
                           CellId = info.c.LoadingUnit.CellId,
                           CompartmentTypeId = info.c.CompartmentTypeId,
                           FifoStartDate = info.c.FifoStartDate,
                           Id = info.c.Id,
                           ItemId = info.c.ItemId.Value,
                           LoadingUnitId = info.c.LoadingUnitId,
                           Lot = info.c.Lot,
                           MaterialStatusId = info.c.MaterialStatusId,
                           PackageTypeId = info.c.PackageTypeId,
                           RegistrationNumber = info.c.RegistrationNumber,
                           ReservedForPick = info.c.ReservedForPick,
                           ReservedToPut = info.c.ReservedToPut,
                           Stock = info.c.Stock,
                           Sub1 = info.c.Sub1,
                           Sub2 = info.c.Sub2,
                           IsItemPairingFixed = info.c.IsItemPairingFixed,
                       })
                       .Where(c => c.RemainingCapacity > 0);

                    break;

                default:
                    throw new ArgumentException("Only pick and put requests are supported.", nameof(request));
            }

            return candidateCompartments;
        }

        public Expression<Func<Compartment, bool>> GetCompartmentIsInBayFunction(
                            int? bayId,
            bool isVertimag = true)
        {
            if (!bayId.HasValue)
            {
                return compartment => true;
            }

            if (isVertimag)
            {
                return c => c.LoadingUnit.Cell.Aisle.Machines.Any(m => m.Bays.Any(b => b.Id == bayId));
            }

            return c => c.LoadingUnit.Cell.Aisle.Area.Bays.Any(b => b.Id == bayId.Value);
        }

        public IQueryable<T> OrderCompartmentsByManagementType<T>(
            IQueryable<T> compartments,
            ItemManagementType managementType,
            OperationType operationType)
            where T : IOrderableCompartment
        {
            if (compartments == null)
            {
                throw new ArgumentNullException(nameof(compartments));
            }

            var selector = GetFieldSelectorForOrdering<T>(operationType);

            switch (managementType)
            {
                case ItemManagementType.FIFO:
                    {
                        var orderedCompartments = compartments
                            .OrderBy(c => c.FifoStartDate)
                            .ThenBy(selector);

                        return orderedCompartments;
                    }

                case ItemManagementType.Volume:
                    {
                        var orderedCompartments = compartments
                            .OrderBy(selector);

                        return orderedCompartments;
                    }

                default:
                    throw new ArgumentException(
                        $"Unable to interpret enumeration value for {nameof(ItemManagementType)}",
                        nameof(managementType));
            }
        }

        public async Task<IOperationResult<CandidateCompartment>> UpdateAsync(CandidateCompartment model)
        {
            return await this.UpdateAsync<Common.DataModels.Compartment, CandidateCompartment, int>(
                model,
                this.dataContext.Compartments,
                this.dataContext,
                checkForPolicies: false);
        }

        private static Expression<Func<T, double>> GetFieldSelectorForOrdering<T>(OperationType operationType)
            where T : IOrderableCompartment
        {
            if (typeof(T).GetInterface(nameof(IOrderableCompartmentSet)) != null)
            {
                Expression<Func<T, double>> availabilitySetSelector = c => c.Availability / ((IOrderableCompartmentSet)c).Size;

                Expression<Func<T, double>> remainingCapacitySetSelector = c => c.RemainingCapacity / ((IOrderableCompartmentSet)c).Size;

                return operationType == OperationType.Withdrawal ? availabilitySetSelector : remainingCapacitySetSelector;
            }
            else
            {
                Expression<Func<T, double>> availabilitySelector = c => c.Availability;

                Expression<Func<T, double>> remainingCapacitySelector = c => c.RemainingCapacity;

                return operationType == OperationType.Withdrawal ? availabilitySelector : remainingCapacitySelector;
            }
        }

        #endregion
    }
}
