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
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Providers
{
    [SuppressMessage(
    "Critical Code Smell",
    "S3776:Cognitive Complexity of methods should not be too high",
    Justification = "To refactor return anonymous type")]
    internal class SchedulerRequestPutProvider : BaseProvider, ISchedulerRequestPutProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly ICompartmentOperationProvider compartmentOperationProvider;

        private readonly IItemProvider itemProvider;

        #endregion

        #region Constructors

        public SchedulerRequestPutProvider(
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

        public async Task<IOperationResult<IEnumerable<ItemSchedulerRequest>>> FullyQualifyPutRequestAsync(
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

            if (!string.IsNullOrEmpty(itemOptions.RegistrationNumber))
            {
                if (itemOptions.RequestedQuantity > 1)
                {
                    return new BadRequestOperationResult<IEnumerable<ItemSchedulerRequest>>(
                        Resources.Errors.WhenRegistrationNumberIsSpecifiedTheRequestedQuantityMustBeOne);
                }

                var registrationNumberCount = await this.compartmentOperationProvider
                    .GetAllCountByRegistrationNumberAsync(
                        itemId,
                        itemOptions.RegistrationNumber);

                if (registrationNumberCount > 0)
                {
                    return new BadRequestOperationResult<IEnumerable<ItemSchedulerRequest>>(
                        Resources.Errors.ThisRegistrationNumberIsAlreadyPresentForThisItem);
                }
            }

            var item = await this.itemProvider.GetByIdAsync(itemId);
            if (item == null)
            {
                return new NotFoundOperationResult<IEnumerable<ItemSchedulerRequest>>(
                    null,
                    Resources.Errors.TheSpecifiedItemDoesNotExist);
            }

            if (!item.CanExecuteOperation(nameof(ItemPolicy.Put)))
            {
                return new BadRequestOperationResult<IEnumerable<ItemSchedulerRequest>>(
                    item.GetCanExecuteOperationReason(nameof(ItemPolicy.Put)));
            }

            var compartmentSets = this.GetCompartmentSetsForRequest(item, itemOptions);

            compartmentSets = this.compartmentOperationProvider
               .OrderCompartmentsByManagementType(compartmentSets, item.ManagementType, Enums.OperationType.Put);

            var selectedSets = SelectMinimumCompartmentSets(compartmentSets, itemOptions.RequestedQuantity);
            if (selectedSets.Sum(s => s.RemainingCapacity) < itemOptions.RequestedQuantity)
            {
                return new BadRequestOperationResult<IEnumerable<ItemSchedulerRequest>>(
                    Resources.Errors.NotEnoughAvailableCompartmentsToServeTheRequest);
            }

            var qualifiedRequests = new List<ItemSchedulerRequest>();
            foreach (var compartmentSet in selectedSets)
            {
                if (itemOptions.RequestedQuantity > 0)
                {
                    var qualifiedRequest = ItemSchedulerRequest.FromPutOptions(itemId, itemOptions, row);
                    await this.CompileRequestDataAsync(itemOptions, row, previousRowRequestPriority, compartmentSet, qualifiedRequest);

                    qualifiedRequest.RequestedQuantity = Math.Min(compartmentSet.RemainingCapacity, itemOptions.RequestedQuantity);

                    System.Diagnostics.Debug.Assert(
                        qualifiedRequest.RequestedQuantity > 0,
                        "The requested quantity should always be positive");

                    itemOptions.RequestedQuantity -= qualifiedRequest.RequestedQuantity;

                    qualifiedRequests.Add(qualifiedRequest);
                }
            }

            return new SuccessOperationResult<IEnumerable<ItemSchedulerRequest>>(qualifiedRequests);
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
                return new NotFoundOperationResult<double>(
                    0,
                    Resources.Errors.TheSpecifiedItemDoesNotExist);
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
                if (selectedSets.Sum(s => s.RemainingCapacity) < requestedQuantity
                    && compartmentSet.RemainingCapacity > 0)
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

        private IQueryable<CompartmentSet> GetCompartmentSetsForRequest(ItemDetails item, ItemOptions itemPutOptions)
        {
            System.Diagnostics.Debug.Assert(item != null, "Parameter 'item' should not be null");
            System.Diagnostics.Debug.Assert(itemPutOptions != null, "Parameter 'itemPutOptions' should not be null");

            var now = DateTime.UtcNow;

            var compartmentIsInBayFunction = this.compartmentOperationProvider.GetCompartmentIsInBayFunction(itemPutOptions.BayId);

            var compartmentIsInBayWithMaxCapacity =
                this.DataContext.Compartments
                    .Where(compartmentIsInBayFunction)
                    .Join(
                        this.DataContext.ItemsCompartmentTypes.Where(ict => ict.ItemId == item.Id),
                        c => c.CompartmentTypeId,
                        ict => ict.CompartmentTypeId,
                        (c, ict) => new
                        {
                            c,
                            ict.MaxCapacity,
                            Availability = c.Stock - c.ReservedForPick + c.ReservedToPut,
                        });

            var candidateCompartments = compartmentIsInBayWithMaxCapacity
                .Where(j => j.c.ItemId == item.Id || j.c.ItemId == null)
                .Where(j => j.c.LoadingUnit.Cell.Aisle.Area.Id == itemPutOptions.AreaId)
                .Where(j => // Get all good compartments to PUT, split them in two cases:
                    (j.c.Stock.Equals(0) && (!j.c.IsItemPairingFixed || j.c.ItemId == item.Id)) // get all empty Compartments
                    ||
                    (
                        string.IsNullOrEmpty(itemPutOptions.RegistrationNumber) // if registration number is specified, the compartment should be empty
                        &&
                        j.c.ItemId == item.Id // get all Compartments filtered by user input, that are not full
                        &&
                        j.c.Stock < j.MaxCapacity // only non-full compartments
                        &&
                        (!item.FifoTimePut.HasValue || now.Subtract(j.c.FifoStartDate.Value).TotalDays < item.FifoTimePut.Value) // if item is type by FIFO, evaluate date with time
                        &&
                        (itemPutOptions.Sub1 == null || j.c.Sub1 == itemPutOptions.Sub1) // check filter input data from user
                        &&
                        (itemPutOptions.Sub2 == null || j.c.Sub2 == itemPutOptions.Sub2) // check filter input data from user
                        &&
                        (itemPutOptions.Lot == null || j.c.Lot == itemPutOptions.Lot) // check filter input data from user
                        &&
                        (!itemPutOptions.PackageTypeId.HasValue || j.c.PackageTypeId == itemPutOptions.PackageTypeId) // check filter input data from user
                        &&
                        (!itemPutOptions.MaterialStatusId.HasValue || j.c.MaterialStatusId == itemPutOptions.MaterialStatusId))); // check filter input data from user

            var candidateCompartmentSets = candidateCompartments
                .GroupBy( // grouping all compartments with same properties
                    j => new { j.c.Sub1, j.c.Sub2, j.c.Lot, j.c.PackageTypeId, j.c.MaterialStatusId, j.c.RegistrationNumber },
                    (key, compartments) => new
                    {
                        Key = key,
                        RemainingCapacity = compartments.Sum(
                            j => j.MaxCapacity - j.Availability), // calculated the amount of free remaining capacity of grouping of compartments
                        CompartmentsCount = compartments.Count(),
                        Sub1 = key.Sub1,
                        Sub2 = key.Sub2,
                        Lot = key.Lot,
                        PackageTypeId = key.PackageTypeId,
                        MaterialStatusId = key.MaterialStatusId,
                        RegistrationNumber = key.RegistrationNumber,
                        FifoStartDate = compartments.Min(j => j.c.FifoStartDate.HasValue ? j.c.FifoStartDate.Value : now),
                    });

            System.Diagnostics.Debug.WriteLine($"Put request for item (id={item.Id}): A total of {candidateCompartmentSets.Count()} compartment sets match the request.");

            var aggregatedRequests = this.DataContext.SchedulerRequests
                .Where(r => r.ItemId == item.Id && r.Status != Enums.SchedulerRequestStatus.Completed);

            System.Diagnostics.Debug.WriteLine($"Put request for item (id={item.Id}): There are {aggregatedRequests.Count()} accepted requests for the same item.");

            var compartmentSets = candidateCompartmentSets
            .GroupJoin(
                aggregatedRequests,
                c => new { c.Sub1, c.Sub2, c.Lot, c.PackageTypeId, c.MaterialStatusId, c.RegistrationNumber },
                r => new { r.Sub1, r.Sub2, r.Lot, r.PackageTypeId, r.MaterialStatusId, r.RegistrationNumber },
                (c, r) => new
                {
                    c,
                    requests = r.DefaultIfEmpty(),
                })
            .Select(g => new CompartmentSet
            {
                RemainingCapacity = g.c.RemainingCapacity - g.requests.Sum(
                    r => (r.OperationType == Enums.OperationType.Put ? 1 : -1) * (r.RequestedQuantity.Value - r.ReservedQuantity.Value)),
                Size = g.c.CompartmentsCount,
                Sub1 = g.c.Sub1,
                Sub2 = g.c.Sub2,
                Lot = g.c.Lot,
                PackageTypeId = g.c.PackageTypeId,
                MaterialStatusId = g.c.MaterialStatusId,
                RegistrationNumber = g.c.RegistrationNumber,
                FifoStartDate = g.c.FifoStartDate,
            });

            System.Diagnostics.Debug.WriteLine($"Put request for item (id={item.Id}): There is a total of {compartmentSets.Sum(c => c.Availability)} availability in the identified sets.");
            System.Diagnostics.Debug.WriteLine(
                string.Join(
                    Environment.NewLine,
                    compartmentSets.Select(
                        c => string.Format(
                                "Availability={0}, Sub1='{1}', Sub2='{2}', Lot='{3}', PackageTypeId={4}, MaterialStatusId={5}, RegistrationNumber='{6}'",
                                c.Availability,
                                c.Sub1,
                                c.Sub2,
                                c.Lot,
                                c.PackageTypeId,
                                c.MaterialStatusId,
                                c.RegistrationNumber))));

            return compartmentSets;
        }

        #endregion
    }
}
