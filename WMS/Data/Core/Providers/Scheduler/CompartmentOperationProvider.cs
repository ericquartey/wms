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
    public class CompartmentOperationProvider : ICompartmentOperationProvider
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

        public async Task<StockUpdateCompartment> GetByIdForStockUpdateAsync(int id)
        {
            return await this.dataContext.Compartments
                .Select(c => new StockUpdateCompartment
                {
                    Id = c.Id,
                    LastPickDate = c.LastPickDate,
                    LastPutDate = c.LastPutDate,
                    ItemId = c.ItemId,
                    ReservedForPick = c.ReservedForPick,
                    ReservedForPut = c.ReservedToPut,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                    Stock = c.Stock,
                    LoadingUnitId = c.LoadingUnitId,
                    ItemCompartmentTypeId = c.CompartmentType.ItemsCompartmentTypes
                        .SingleOrDefault(ct => ct.ItemId == c.ItemId).CompartmentTypeId
                })
                .Where(c => c.Id == id)
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

            var filteredCompartments = FilterCompartmentsByMachineType(this.dataContext.Compartments, request.BayId);

            filteredCompartments = filteredCompartments
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
                    (c.LoadingUnit.Cell.Aisle.AreaId == request.AreaId));

            IQueryable<CandidateCompartment> candidateCompartments;
            switch (request.OperationType)
            {
                case OperationType.Withdrawal:
                    candidateCompartments = filteredCompartments
                        .Select(c => new CandidateCompartment
                        {
                            AreaId = c.LoadingUnit.Cell.Aisle.AreaId,
                            CellId = c.LoadingUnit.CellId,
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
                    candidateCompartments = filteredCompartments
                        .Join(
                            this.dataContext.ItemsCompartmentTypes
                                .Where(ict => ict.ItemId == request.ItemId),
                            c => c.CompartmentTypeId,
                            ict => ict.CompartmentTypeId,
                            (c, ict) => new { c, ict.MaxCapacity })
                       .Select(info => new CandidateCompartment
                       {
                           AreaId = info.c.LoadingUnit.Cell.Aisle.AreaId,
                           MaxCapacity = info.MaxCapacity,
                           CellId = info.c.LoadingUnit.CellId,
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

            Expression<Func<T, double>> availabilitySelector = c => c.Availability;

            Expression<Func<T, double>> remainingCapacitySelector = c => c.RemainingCapacity;

            var selector = operationType == OperationType.Withdrawal ? availabilitySelector : remainingCapacitySelector;

            switch (managementType)
            {
                case ItemManagementType.FIFO:
                    return compartments
                        .OrderBy(c => c.FifoStartDate)
                        .ThenBy(selector);

                case ItemManagementType.Volume:
                    var orderedCompartments = compartments
                        .OrderBy(selector);

                    if (orderedCompartments is IOrderedQueryable<IOrderableCompartmentSet> compartmentSets)
                    {
                        return compartmentSets
                            .ThenByDescending(set => set.Size)
                            .Cast<T>();
                    }

                    return orderedCompartments;

                default:
                    throw new ArgumentException(
                        $"Unable to interpret enumeration value for {nameof(ItemManagementType)}",
                        nameof(managementType));
            }
        }

        public async Task<IOperationResult<StockUpdateCompartment>> UpdateAsync(StockUpdateCompartment model)
        {
            return await this.UpdateAsync<Common.DataModels.Compartment, StockUpdateCompartment, int>(
                model,
                this.dataContext.Compartments,
                this.dataContext);
        }

        public async Task<IOperationResult<CandidateCompartment>> UpdateAsync(CandidateCompartment model)
        {
            return await this.UpdateAsync<Common.DataModels.Compartment, CandidateCompartment, int>(
                model,
                this.dataContext.Compartments,
                this.dataContext);
        }

        private static IQueryable<Compartment> FilterCompartmentsByMachineType(IQueryable<Compartment> input, int? bayId, bool isVertimag = true)
        {
            if (!bayId.HasValue)
            {
                return input;
            }

            if (isVertimag)
            {
                return input.Where(c => c.LoadingUnit.Cell.Aisle.Machines.Any(m => m.Bays.Any(b => b.Id == bayId)));
            }

            return input.Where(c => c.LoadingUnit.Cell.Aisle.Area.Bays.Any(b => b.Id == bayId.Value));
        }

        #endregion
    }
}
