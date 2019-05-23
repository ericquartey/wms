using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    [SuppressMessage(
    "Critical Code Smell",
    "S3776:Cognitive Complexity of methods should not be too high",
    Justification = "To refactor return anonymous type")]
    public class SchedulerRequestPutProvider : ISchedulerRequestPutProvider
    {
        #region Fields

        public const int InstantRequestPriority = 1;

        private readonly IBayProvider bayProvider;

        private readonly DatabaseContext dataContext;

        private readonly IItemProvider itemProvider;

        #endregion

        #region Constructors

        public SchedulerRequestPutProvider(
            DatabaseContext dataContext,
            IBayProvider bayProvider,
            IItemProvider itemProvider)
        {
            this.dataContext = dataContext;
            this.bayProvider = bayProvider;
            this.itemProvider = itemProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemSchedulerRequest>> FullyQualifyPutRequestAsync(
             int itemId,
             ItemOptions itemPutOptions,
             ItemListRowOperation row = null,
             int? previousRowRequestPriority = null)
        {
            if (itemPutOptions == null)
            {
                throw new ArgumentNullException(nameof(itemPutOptions));
            }

            if (itemPutOptions.RequestedQuantity <= 0)
            {
                return new BadRequestOperationResult<ItemSchedulerRequest>(
                    null,
                    "Requested quantity must be positive.");
            }

            var item = await this.itemProvider.GetByIdAsync(itemId);
            if (item == null)
            {
                return new NotFoundOperationResult<ItemSchedulerRequest>(null, "The specified item does not exist.");
            }

            if (!item.CanExecuteOperation(nameof(ItemPolicy.Put)))
            {
                return new BadRequestOperationResult<ItemSchedulerRequest>(
                    null,
                    item.GetCanExecuteOperationReason(nameof(ItemPolicy.Put)));
            }

            var compartmentSets = this.GetCompartmentSetsForRequest(item, itemPutOptions)
                .Where(x => x.RemainingCapacity >= itemPutOptions.RequestedQuantity);

            if (item.ManagementType == ItemManagementType.FIFO)
            {
                compartmentSets = compartmentSets
                    .OrderBy(x => x.FifoStartDate)
                    .ThenBy(x => x.RemainingCapacity);
            }
            else
            {
                compartmentSets = compartmentSets
                    .OrderBy(x => x.RemainingCapacity);
            }

            var bestCompartmentSet = await compartmentSets.FirstOrDefaultAsync();
            if (bestCompartmentSet == null)
            {
                return new BadRequestOperationResult<ItemSchedulerRequest>(null, "No available compartments to serve the request.");
            }

            var qualifiedRequest = ItemSchedulerRequest.FromPutOptions(itemId, itemPutOptions, row);
            await this.CompileRequestDataAsync(itemPutOptions, row, previousRowRequestPriority, bestCompartmentSet, qualifiedRequest);

            return new SuccessOperationResult<ItemSchedulerRequest>(qualifiedRequest);
        }

        public async Task<IOperationResult<double>> GetAvailableCapacityAsync(int itemId, ItemOptions itemPutOptions)
        {
            if (itemPutOptions == null)
            {
                throw new ArgumentNullException(nameof(itemPutOptions));
            }

            var item = await this.itemProvider.GetByIdAsync(itemId);
            if (item == null)
            {
                return new NotFoundOperationResult<double>(0, "The specified item does not exist.");
            }

            var compartments = this.GetCompartmentSetsForRequest(item, itemPutOptions);

            var availableCapacity = await compartments.SumAsync(set => set.RemainingCapacity);

            return new SuccessOperationResult<double>(availableCapacity);
        }

        private static int ComputeRequestBasePriority(ISchedulerRequest schedulerRequest, int? rowPriority, int? previousRowRequestPriority)
        {
            var priority = 0;

            if (schedulerRequest.IsInstant)
            {
                return InstantRequestPriority;
            }

            if (rowPriority.HasValue)
            {
                priority = rowPriority.Value;
            }
            else if (previousRowRequestPriority.HasValue)
            {
                priority = previousRowRequestPriority.Value;
            }
            else
            {
                priority = InstantRequestPriority;
            }

            return priority;
        }

        private async Task CompileRequestDataAsync(
            ItemOptions itemPutOptions,
            ItemListRowOperation row,
            int? previousRowRequestPriority,
            CompartmentSet bestCompartmentSet,
            ItemSchedulerRequest qualifiedRequest)
        {
            var baseRequestPriority = ComputeRequestBasePriority(qualifiedRequest, row?.Priority, previousRowRequestPriority);
            if (row?.Priority.HasValue == true)
            {
                qualifiedRequest.Priority = await this.ComputeRequestPriorityAsync(baseRequestPriority, itemPutOptions.BayId);
            }
            else
            {
                qualifiedRequest.Priority = baseRequestPriority;
            }

            qualifiedRequest.Lot = bestCompartmentSet.Lot;
            qualifiedRequest.MaterialStatusId = bestCompartmentSet.MaterialStatusId;
            qualifiedRequest.PackageTypeId = bestCompartmentSet.PackageTypeId;
            qualifiedRequest.RegistrationNumber = bestCompartmentSet.RegistrationNumber;
            qualifiedRequest.Sub1 = bestCompartmentSet.Sub1;
            qualifiedRequest.Sub2 = bestCompartmentSet.Sub2;

            if (itemPutOptions.BayId.HasValue && itemPutOptions.RunImmediately)
            {
                await this.bayProvider.UpdatePriorityAsync(itemPutOptions.BayId.Value, baseRequestPriority);
            }
        }

        private async Task<int?> ComputeRequestPriorityAsync(int baseSchedulerRequestPriority, int? bayId)
        {
            var priority = baseSchedulerRequestPriority;

            if (bayId.HasValue)
            {
                var bay = await this.dataContext.Bays.SingleAsync(b => b.Id == bayId.Value);
                priority += bay.Priority;
            }

            return priority;
        }

        private IQueryable<CompartmentSetForPut> GetCompartmentSetsForRequest(ItemDetails item, ItemOptions itemPutOptions)
        {
            System.Diagnostics.Debug.Assert(item != null, "Parameter 'item' should not be null");
            System.Diagnostics.Debug.Assert(itemPutOptions != null, "Parameter 'itemPutOptions' should not be null");

            var now = DateTime.UtcNow;

            var aggregatedCompartments =
                      this.dataContext.ItemsCompartmentTypes
                      .Where(ict => ict.ItemId == item.Id)
                      .Join(
                          this.dataContext.Compartments,
                          ict => ict.CompartmentTypeId,
                          c => c.CompartmentTypeId,
                          (ict, c) => new
                          {
                              c,
                              ict.ItemId,
                              ict.MaxCapacity,
                          })
                      .Where(j => (j.c.ItemId == j.ItemId || j.c.ItemId == null))
                      .Where(j =>
                           j.c.LoadingUnit.Cell.Aisle.Area.Id == itemPutOptions.AreaId
                           &&
                           (!itemPutOptions.BayId.HasValue || j.c.LoadingUnit.Cell.Aisle.Area.Bays.Any(b => b.Id == itemPutOptions.BayId)))
                      .Where(j => // Get all good compartments to PUT, split them in two cases:
                          j.c.Stock.Equals(0) // get all empty Compartments
                          ||
                          (
                              j.c.ItemId == item.Id // get all Compartments filtered by user input, that are not full
                              &&
                              j.c.Stock < j.MaxCapacity
                              &&
                              (!item.FifoTimePut.HasValue || now.Subtract(j.c.FifoStartDate.Value).TotalDays < item.FifoTimePut.Value)
                              &&
                              (itemPutOptions.Sub1 == null || j.c.Sub1 == itemPutOptions.Sub1)
                              &&
                              (itemPutOptions.Sub2 == null || j.c.Sub2 == itemPutOptions.Sub2)
                              &&
                              (itemPutOptions.Lot == null || j.c.Lot == itemPutOptions.Lot)
                              &&
                              (!itemPutOptions.PackageTypeId.HasValue || j.c.PackageTypeId == itemPutOptions.PackageTypeId)
                              &&
                              (!itemPutOptions.MaterialStatusId.HasValue || j.c.MaterialStatusId == itemPutOptions.MaterialStatusId)
                              &&
                              (itemPutOptions.RegistrationNumber == null || j.c.RegistrationNumber == itemPutOptions.RegistrationNumber)))
                      .GroupBy(
                          j => new { j.c.Sub1, j.c.Sub2, j.c.Lot, j.c.PackageTypeId, j.c.MaterialStatusId, j.c.RegistrationNumber },
                          (key, compartments) => new
                          {
                              Key = key,
                              RemainingCapacity = compartments.Sum(
                                  j => j.MaxCapacity.HasValue == true ? j.MaxCapacity.Value - j.c.Stock - j.c.ReservedForPick + j.c.ReservedToPut
                                          :
                                          double.MaxValue),
                              Sub1 = key.Sub1,
                              Sub2 = key.Sub2,
                              Lot = key.Lot,
                              PackageTypeId = key.PackageTypeId,
                              MaterialStatusId = key.MaterialStatusId,
                              RegistrationNumber = key.RegistrationNumber,
                              FifoStartDate = compartments.Min(j => j.c.FifoStartDate.HasValue ? j.c.FifoStartDate.Value : now)
                          });

            var aggregatedRequests = this.dataContext.SchedulerRequests
                .Where(r => r.ItemId == item.Id && r.Status != Common.DataModels.SchedulerRequestStatus.Completed);

            return aggregatedCompartments
            .GroupJoin(
                aggregatedRequests,
                c => new { c.Sub1, c.Sub2, c.Lot, c.PackageTypeId, c.MaterialStatusId, c.RegistrationNumber },
                r => new { r.Sub1, r.Sub2, r.Lot, r.PackageTypeId, r.MaterialStatusId, r.RegistrationNumber },
                (c, r) => new
                {
                    c,
                    requests = r.DefaultIfEmpty()
                })
            .Select(g => new CompartmentSetForPut
            {
                RemainingCapacity = g.c.RemainingCapacity - g.requests.Sum(
                    r => (r.OperationType == Common.DataModels.OperationType.Insertion ? 1 : -1) * (r.RequestedQuantity.Value - r.ReservedQuantity.Value)),
                Sub1 = g.c.Sub1,
                Sub2 = g.c.Sub2,
                Lot = g.c.Lot,
                PackageTypeId = g.c.PackageTypeId,
                MaterialStatusId = g.c.MaterialStatusId,
                RegistrationNumber = g.c.RegistrationNumber,
                FifoStartDate = g.c.FifoStartDate
            });
        }

        #endregion
    }
}
