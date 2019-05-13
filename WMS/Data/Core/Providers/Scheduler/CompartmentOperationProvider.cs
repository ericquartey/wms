﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

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
                    ItemId = c.ItemId,
                    ReservedForPick = c.ReservedForPick,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                    Stock = c.Stock,
                    LoadingUnitId = c.LoadingUnitId
                })
                .Where(c => c.Id == id)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Gets all compartments in the specified area/bay that have availability for the specified item.
        /// </summary>
        /// <param name="schedulerRequest"></param>
        /// <returns>The unsorted set of compartments matching the specified request.</returns>
        public IQueryable<CompartmentWithdraw> GetCandidateWithdrawalCompartments(ItemSchedulerRequest schedulerRequest)
        {
            if (schedulerRequest == null)
            {
                throw new ArgumentNullException(nameof(schedulerRequest));
            }

            if (schedulerRequest.OperationType != OperationType.Withdrawal)
            {
                throw new ArgumentException("Only withdrawal requests are supported.", nameof(schedulerRequest));
            }

            return this.dataContext.Compartments
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
                .Select(c => new CompartmentWithdraw
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
                    ReservedToStore = c.ReservedToStore,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                });
        }

        public IQueryable<T> OrderPickCompartmentsByManagementType<T>(IQueryable<T> compartments, ItemManagementType type)
            where T : IOrderableCompartment
        {
            switch (type)
            {
                case ItemManagementType.FIFO:
                    return compartments
                        .OrderBy(c => c.FifoStartDate)
                        .ThenBy(c => c.Availability);

                case ItemManagementType.Volume:
                    return compartments
                        .OrderBy(c => c.Availability)
                        .ThenBy(c => c.FifoStartDate);

                default:
                    throw new ArgumentException(
                        $"Unable to interpret enumeration value for {nameof(ItemManagementType)}",
                        nameof(type));
            }
        }

        public async Task<IOperationResult<StockUpdateCompartment>> UpdateAsync(StockUpdateCompartment model)
        {
            return await this.UpdateAsync<Common.DataModels.Compartment, StockUpdateCompartment, int>(
                model,
                this.dataContext.Compartments,
                this.dataContext);
        }

        public async Task<IOperationResult<CompartmentWithdraw>> UpdateAsync(CompartmentWithdraw model)
        {
            return await this.UpdateAsync<Common.DataModels.Compartment, CompartmentWithdraw, int>(
                model,
                this.dataContext.Compartments,
                this.dataContext);
        }

        #endregion
    }
}
