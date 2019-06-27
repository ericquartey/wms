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
    internal class SchedulerRequestPickProvider : BaseProvider, ISchedulerRequestPickProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly ICompartmentOperationProvider compartmentOperationProvider;

        private readonly IItemProvider itemProvider;

        #endregion

        #region Constructors

        public SchedulerRequestPickProvider(
            DatabaseContext dataContext,
            ICompartmentOperationProvider compartmentOperationProvider,
            IBayProvider bayProvider,
            IItemProvider itemProvider,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.compartmentOperationProvider = compartmentOperationProvider;
            this.bayProvider = bayProvider;
            this.itemProvider = itemProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<IEnumerable<ItemSchedulerRequest>>> FullyQualifyPickRequestAsync(
              int itemId,
              ItemOptions itemOptions,
              ItemListRowOperation row = null,
              int? previousRowRequestPriority = null)
        {
            if (itemOptions == null)
            {
                throw new ArgumentNullException(nameof(itemOptions));
            }

            if (itemOptions.RequestedQuantity <= 0)
            {
                return new BadRequestOperationResult<IEnumerable<ItemSchedulerRequest>>(
                    Resources.Errors.RequestedQuantityMustBePositive);
            }

            if (!string.IsNullOrEmpty(itemOptions.RegistrationNumber)
                && itemOptions.RequestedQuantity > 1)
            {
                return new BadRequestOperationResult<IEnumerable<ItemSchedulerRequest>>(
                    Resources.Errors.WhenRegistrationNumberIsSpecifiedTheRequestedQuantityMustBeOne);
            }

            var item = await this.itemProvider.GetByIdAsync(itemId);
            if (item == null)
            {
                return new NotFoundOperationResult<IEnumerable<ItemSchedulerRequest>>(
                    null,
                    Resources.Errors.TheSpecifiedItemDoesNotExist);
            }

            if (!item.CanExecuteOperation(nameof(ItemPolicy.Pick)))
            {
                return new BadRequestOperationResult<IEnumerable<ItemSchedulerRequest>>(
                    item.GetCanExecuteOperationReason(nameof(ItemPolicy.Pick)));
            }

            var compartmentSets = this.GetCompartmentSetsForRequest(item, itemOptions);

            compartmentSets = this.compartmentOperationProvider
                .OrderCompartmentsByManagementType(compartmentSets, item.ManagementType, OperationType.Withdrawal);

            var selectedSets = SelectMinimumCompartmentSets(compartmentSets, itemOptions.RequestedQuantity);
            if (selectedSets.Sum(s => s.Availability) < itemOptions.RequestedQuantity)
            {
                return new BadRequestOperationResult<IEnumerable<ItemSchedulerRequest>>(
                    Resources.Errors.NotEnoughAvailableCompartmentsToServeTheRequest);
            }

            var qualifiedRequests = new List<ItemSchedulerRequest>();
            foreach (var compartmentSet in selectedSets)
            {
                if (itemOptions.RequestedQuantity > 0)
                {
                    var qualifiedRequest = ItemSchedulerRequest.FromPickOptions(itemId, itemOptions, row);
                    await this.CompileRequestDataAsync(itemOptions, row, previousRowRequestPriority, compartmentSet, qualifiedRequest);

                    qualifiedRequest.RequestedQuantity = Math.Min(compartmentSet.Availability, itemOptions.RequestedQuantity);
                    itemOptions.RequestedQuantity -= qualifiedRequest.RequestedQuantity;

                    qualifiedRequests.Add(qualifiedRequest);
                }
            }

            return new SuccessOperationResult<IEnumerable<ItemSchedulerRequest>>(qualifiedRequests);
        }

        public async Task<IOperationResult<double>> GetItemAvailabilityAsync(int itemId, ItemOptions itemPickOptions)
        {
            if (itemPickOptions == null)
            {
                throw new ArgumentNullException(nameof(itemPickOptions));
            }

            var item = await this.itemProvider.GetByIdAsync(itemId);
            if (item == null)
            {
                return new NotFoundOperationResult<double>(
                    0,
                    Resources.Errors.TheSpecifiedItemDoesNotExist);
            }

            var compartments = this.GetCompartmentSetsForRequest(item, itemPickOptions);

            var availability = await compartments.SumAsync(set => set.Availability);

            return new SuccessOperationResult<double>(availability);
        }

        private static int ComputeRequestBasePriority(ISchedulerRequest schedulerRequest, int? rowPriority, int? previousRowRequestPriority)
        {
            var priority = 0;

            if (schedulerRequest.IsInstant)
            {
                return SchedulerRequest.InstantRequestPriority;
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
                priority = SchedulerRequest.InstantRequestPriority;
            }

            return priority;
        }

        private static List<CompartmentSet> SelectMinimumCompartmentSets(
            IQueryable<CompartmentSet> compartmentSets,
            double requestedQuantity)
        {
            var selectedSets = new List<CompartmentSet>();

            foreach (var compartmentSet in compartmentSets)
            {
                if (selectedSets.Sum(s => s.Availability) < requestedQuantity)
                {
                    selectedSets.Add(compartmentSet);
                }
            }

            return selectedSets;
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
                var bay = await this.DataContext.Bays.SingleAsync(b => b.Id == bayId.Value);
                priority += bay.Priority;
            }

            return priority;
        }

        private IQueryable<CompartmentSet> GetCompartmentSetsForRequest(ItemDetails item, ItemOptions itemPickOptions)
        {
            System.Diagnostics.Debug.Assert(item != null, "Parameter 'item' should not be null");
            System.Diagnostics.Debug.Assert(itemPickOptions != null, "Parameter 'itemPickOptions' should not be null");

            var compartmentIsInBay = this.compartmentOperationProvider.GetCompartmentIsInBayFunction(itemPickOptions.BayId);

            var aggregatedCompartments = this.DataContext.Compartments
                .Include(c => c.LoadingUnit)
                .ThenInclude(l => l.Cell)
                .ThenInclude(c => c.Aisle)
                .ThenInclude(a => a.Area)
                .Where(c =>
                    c.ItemId == item.Id
                    &&
                    c.LoadingUnit.Cell.Aisle.Area.Id == itemPickOptions.AreaId)
                .Where(compartmentIsInBay)
                .Where(c =>
                    (itemPickOptions.Sub1 == null || c.Sub1 == itemPickOptions.Sub1)
                    &&
                    (itemPickOptions.Sub2 == null || c.Sub2 == itemPickOptions.Sub2)
                    &&
                    (itemPickOptions.Lot == null || c.Lot == itemPickOptions.Lot)
                    &&
                    (!itemPickOptions.PackageTypeId.HasValue || c.PackageTypeId == itemPickOptions.PackageTypeId)
                    &&
                    (!itemPickOptions.MaterialStatusId.HasValue || c.MaterialStatusId == itemPickOptions.MaterialStatusId)
                    &&
                    (itemPickOptions.RegistrationNumber == null || c.RegistrationNumber == itemPickOptions.RegistrationNumber))
                .GroupBy(
                    x => new { x.Sub1, x.Sub2, x.Lot, x.PackageTypeId, x.MaterialStatusId, x.RegistrationNumber },
                    (key, group) => new
                    {
                        Key = key,
                        Availability = group.Sum(c => c.Stock - c.ReservedForPick + c.ReservedToPut),
                        CompartmentsCount = group.Count(),
                        Sub1 = key.Sub1,
                        Sub2 = key.Sub2,
                        Lot = key.Lot,
                        PackageTypeId = key.PackageTypeId,
                        MaterialStatusId = key.MaterialStatusId,
                        RegistrationNumber = key.RegistrationNumber,
                        FifoStartDate = group.Min(c => c.FifoStartDate)
                    });

            var aggregatedRequests = this.DataContext.SchedulerRequests
                .Where(r => r.ItemId == item.Id && r.Status != Common.DataModels.SchedulerRequestStatus.Completed);

            return aggregatedCompartments
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
                    Availability = g.c.Availability - g.r.Sum(
                        r => (r.OperationType == Common.DataModels.OperationType.Withdrawal ? 1 : -1) * (r.RequestedQuantity.Value - r.ReservedQuantity.Value)),
                    Size = g.c.CompartmentsCount,
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
