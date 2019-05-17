using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage(
        "Critical Code Smell",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "To refactor return anonymous type")]
    public class SchedulerRequestExecutionProvider : ISchedulerRequestExecutionProvider
    {
        #region Fields

        public const int InstantRequestPriority = 1;

        private readonly IBayProvider bayProvider;

        private readonly ICompartmentOperationProvider compartmentOperationProvider;

        private readonly DatabaseContext dataContext;

        private readonly IItemProvider itemProvider;

        #endregion

        #region Constructors

        public SchedulerRequestExecutionProvider(
            DatabaseContext dataContext,
            ICompartmentOperationProvider compartmentOperationProvider,
            IBayProvider bayProvider,
            IItemProvider itemProvider)
        {
            this.dataContext = dataContext;
            this.compartmentOperationProvider = compartmentOperationProvider;
            this.bayProvider = bayProvider;
            this.itemProvider = itemProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemSchedulerRequest>> CreateAsync(ItemSchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = this.dataContext.SchedulerRequests
                .Add(CreateDataModel(model));

            if (await this.dataContext.SaveChangesAsync() > 0)
            {
                model.Id = entry.Entity.Id;
            }

            return new SuccessOperationResult<ItemSchedulerRequest>(model);
        }

        public async Task<IOperationResult<LoadingUnitSchedulerRequest>> CreateAsync(LoadingUnitSchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = this.dataContext.SchedulerRequests
                .Add(new Common.DataModels.SchedulerRequest
                {
                    LoadingUnitId = model.LoadingUnitId,
                    LoadingUnitTypeId = model.LoadingUnitTypeId,
                    OperationType = (Common.DataModels.OperationType)model.OperationType,
                    Type = (Common.DataModels.SchedulerRequestType)model.Type,
                    IsInstant = model.IsInstant,
                    Priority = model.Priority,
                    BayId = model.BayId,
                    Status = (Common.DataModels.SchedulerRequestStatus)model.Status,
                });

            if (await this.dataContext.SaveChangesAsync() > 0)
            {
                model.Id = entry.Entity.Id;
            }

            return new SuccessOperationResult<LoadingUnitSchedulerRequest>(model);
        }

        public async Task<IOperationResult<ItemListRowSchedulerRequest>> CreateAsync(ItemListRowSchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = this.dataContext.SchedulerRequests
                .Add(CreateDataModel(model));

            if (await this.dataContext.SaveChangesAsync() > 0)
            {
                model.Id = entry.Entity.Id;
            }

            return new SuccessOperationResult<ItemListRowSchedulerRequest>(model);
        }

        public async Task<IEnumerable<ItemSchedulerRequest>> CreateRangeAsync(IEnumerable<ItemSchedulerRequest> models)
        {
            var requests = models.Select(r => CreateDataModel(r));

            await this.dataContext.AddRangeAsync(requests);

            await this.dataContext.SaveChangesAsync();

            return models;
        }

        public async Task<ItemSchedulerRequest> FullyQualifyPickRequestAsync(
            int itemId,
            ItemOptions itemPickOptions,
            ItemListRowOperation row = null,
            int? previousRowRequestPriority = null)
        {
            if (itemPickOptions == null)
            {
                throw new ArgumentNullException(nameof(itemPickOptions));
            }

            var aggregatedCompartments = this.dataContext.Compartments
                .Include(c => c.LoadingUnit)
                .ThenInclude(l => l.Cell)
                .ThenInclude(c => c.Aisle)
                .ThenInclude(a => a.Area)
                .Where(c =>
                    c.ItemId == itemId
                    &&
                    c.LoadingUnit.Cell.Aisle.Area.Id == itemPickOptions.AreaId
                    &&
                    (itemPickOptions.BayId.HasValue == false || c.LoadingUnit.Cell.Aisle.Area.Bays.Any(b => b.Id == itemPickOptions.BayId))
                    &&
                    (itemPickOptions.Sub1 == null || c.Sub1 == itemPickOptions.Sub1)
                    &&
                    (itemPickOptions.Sub2 == null || c.Sub2 == itemPickOptions.Sub2)
                    &&
                    (itemPickOptions.Lot == null || c.Lot == itemPickOptions.Lot)
                    &&
                    (itemPickOptions.PackageTypeId.HasValue == false || c.PackageTypeId == itemPickOptions.PackageTypeId)
                    &&
                    (itemPickOptions.MaterialStatusId.HasValue == false || c.MaterialStatusId == itemPickOptions.MaterialStatusId)
                    &&
                    (itemPickOptions.RegistrationNumber == null || c.RegistrationNumber == itemPickOptions.RegistrationNumber))
                .GroupBy(
                    x => new { x.Sub1, x.Sub2, x.Lot, x.PackageTypeId, x.MaterialStatusId, x.RegistrationNumber },
                    (key, group) => new
                    {
                        Key = key,
                        Availability = group.Sum(c => c.Stock - c.ReservedForPick + c.ReservedToPut),
                        Sub1 = key.Sub1,
                        Sub2 = key.Sub2,
                        Lot = key.Lot,
                        PackageTypeId = key.PackageTypeId,
                        MaterialStatusId = key.MaterialStatusId,
                        RegistrationNumber = key.RegistrationNumber,
                        FifoStartDate = group.Min(c => c.FifoStartDate)
                    });

            var aggregatedRequests = this.dataContext.SchedulerRequests
                .Where(r => r.ItemId == itemId && r.Status != Common.DataModels.SchedulerRequestStatus.Completed);

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
                    Availability = g.c.Availability - g.r.Sum(r => r.RequestedQuantity.Value - r.ReservedQuantity.Value),
                    Sub1 = g.c.Sub1,
                    Sub2 = g.c.Sub2,
                    Lot = g.c.Lot,
                    PackageTypeId = g.c.PackageTypeId,
                    MaterialStatusId = g.c.MaterialStatusId,
                    RegistrationNumber = g.c.RegistrationNumber,
                    FifoStartDate = g.c.FifoStartDate
                })
                .Where(x => x.Availability >= itemPickOptions.RequestedQuantity);

            var item = await this.dataContext.Items
                .Select(i => new { i.Id, i.ManagementType })
                .SingleAsync(i => i.Id == itemId);

            var bestCompartmentSet = await this.compartmentOperationProvider
                .OrderPickCompartmentsByManagementType(compartmentSets, (ItemManagementType)item.ManagementType)
                .FirstOrDefaultAsync();

            if (bestCompartmentSet == null)
            {
                return null;
            }

            var qualifiedRequest = ItemSchedulerRequest.FromWithdrawalOptions(itemId, itemPickOptions, row);
            await this.CompileRequestDataAsync(itemPickOptions, row, previousRowRequestPriority, bestCompartmentSet, qualifiedRequest);

            return qualifiedRequest;
        }

        public async Task<ItemSchedulerRequest> FullyQualifyPutRequestAsync(
             int itemId,
             ItemOptions itemPutOptions,
             ItemListRowOperation row = null,
             int? previousRowRequestPriority = null)
        {
            if (itemPutOptions == null)
            {
                throw new ArgumentNullException(nameof(itemPutOptions));
            }

            var item = await this.itemProvider.GetByIdAsync(itemId);
            var now = DateTime.UtcNow;

            var aggregatedCompartments =
                      this.dataContext.ItemsCompartmentTypes
                      .Where(ict => ict.ItemId == itemId)
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
                           (itemPutOptions.BayId.HasValue == false || j.c.LoadingUnit.Cell.Aisle.Area.Bays.Any(b => b.Id == itemPutOptions.BayId)))
                      .Where(j => // Get all good compartments to PUT, split them in two cases:
                          j.c.Stock.Equals(0) // OR get All Empty Compartments
                          ||
                          (
                              j.c.ItemId == itemId // OR get all Compartments Filtered by user input + not full
                              &&
                              j.c.Stock < j.MaxCapacity
                              &&
                              (item.FifoTimePut.HasValue == false || now.Subtract(j.c.FifoStartDate.Value).TotalDays < item.FifoTimePut.Value)
                              &&
                              (itemPutOptions.Sub1 == null || j.c.Sub1 == itemPutOptions.Sub1)
                              &&
                              (itemPutOptions.Sub2 == null || j.c.Sub2 == itemPutOptions.Sub2)
                              &&
                              (itemPutOptions.Lot == null || j.c.Lot == itemPutOptions.Lot)
                              &&
                              (itemPutOptions.PackageTypeId.HasValue == false || j.c.PackageTypeId == itemPutOptions.PackageTypeId)
                              &&
                              (itemPutOptions.MaterialStatusId.HasValue == false || j.c.MaterialStatusId == itemPutOptions.MaterialStatusId)
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
                .Where(r => r.ItemId == itemId && r.Status != Common.DataModels.SchedulerRequestStatus.Completed);

            var compartmentSets = aggregatedCompartments
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
           })
           .Where(x => x.RemainingCapacity >= itemPutOptions.RequestedQuantity);

            if (item.ManagementType == ItemManagementType.FIFO)
            {
                compartmentSets = compartmentSets.OrderBy(x => x.FifoStartDate)
                .ThenBy(x => x.RemainingCapacity);
            }
            else
            {
                compartmentSets = compartmentSets.OrderBy(x => x.RemainingCapacity);
            }

            var bestCompartmentSet = await compartmentSets.FirstOrDefaultAsync();
            if (bestCompartmentSet == null)
            {
                return null;
            }

            var qualifiedRequest = ItemSchedulerRequest.FromPutOptions(itemId, itemPutOptions, row);
            await this.CompileRequestDataAsync(itemPutOptions, row, previousRowRequestPriority, bestCompartmentSet, qualifiedRequest);

            return qualifiedRequest;
        }

        /// <summary>
        /// Gets all the pending requests, sorted by priority, that:
        /// - are not completed (dispatched qty is not equal to requested qty)
        /// - are already allocated to a bay
        /// - the allocated bay has buffer to accept new missions
        /// - if related to a list row, the row is marked for execution
        ///
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task<IEnumerable<ISchedulerRequest>> GetRequestsToProcessAsync()
        {
            return await this.dataContext.SchedulerRequests
               .Where(r => r.Status == Common.DataModels.SchedulerRequestStatus.New)
               .Where(r => r.BayId.HasValue
                    && r.Bay.LoadingUnitsBufferSize > r.Bay.Missions.Count(m =>
                        m.Status != Common.DataModels.MissionStatus.Completed
                        && m.Status != Common.DataModels.MissionStatus.Incomplete))
               .Where(r => r.ListRowId.HasValue == false
                    || (r.ListRow.Status == Common.DataModels.ItemListRowStatus.Executing
                    || r.ListRow.Status == Common.DataModels.ItemListRowStatus.Waiting))
               .OrderBy(r => r.Priority)
               .Select(r => SelectRequest(r))
               .ToArrayAsync();
        }

        public async Task<IOperationResult<ItemSchedulerRequest>> UpdateAsync(ItemSchedulerRequest model)
        {
            return await this.UpdateAsync<Common.DataModels.SchedulerRequest, ItemSchedulerRequest, int>(
                model,
                this.dataContext.SchedulerRequests,
                this.dataContext);
        }

        public async Task<IOperationResult<LoadingUnitSchedulerRequest>> UpdateAsync(LoadingUnitSchedulerRequest model)
        {
            return await this.UpdateAsync<Common.DataModels.SchedulerRequest, LoadingUnitSchedulerRequest, int>(
                model,
                this.dataContext.SchedulerRequests,
                this.dataContext);
        }

        private static int ComputeRequestBasePriority(ISchedulerRequest schedulerRequest, int? rowPriority, int? previousRowRequestPriority)
        {
            int priority = 0;

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

        private static Common.DataModels.SchedulerRequest CreateDataModel(ItemSchedulerRequest model)
        {
            var dataModel = new Common.DataModels.SchedulerRequest
            {
                AreaId = model.AreaId,
                BayId = model.BayId,
                IsInstant = model.IsInstant,
                ItemId = model.ItemId,
                Lot = model.Lot,
                MaterialStatusId = model.MaterialStatusId,
                PackageTypeId = model.PackageTypeId,
                RegistrationNumber = model.RegistrationNumber,
                OperationType = (Common.DataModels.OperationType)(int)model.OperationType,
                RequestedQuantity = model.RequestedQuantity,
                ReservedQuantity = model.ReservedQuantity,
                Sub1 = model.Sub1,
                Sub2 = model.Sub2,
                Priority = model.Priority,
                Type = (Common.DataModels.SchedulerRequestType)model.Type,
                Status = (Common.DataModels.SchedulerRequestStatus)model.Status,
            };
            if (model is ItemListRowSchedulerRequest rowRequest)
            {
                dataModel.ListId = rowRequest.ListId;
                dataModel.ListRowId = rowRequest.ListRowId;
            }

            return dataModel;
        }

        private static ISchedulerRequest SelectRequest(Common.DataModels.SchedulerRequest r)
        {
            switch (r.Type)
            {
                case Common.DataModels.SchedulerRequestType.Item:

                    if (r.RequestedQuantity.HasValue == false
                        ||
                        r.ReservedQuantity.HasValue == false)
                    {
                        throw new System.Data.DataException("Item request has missing mandatory fields (BayId, LoadingUnitTypeId, LoadingUnitId)");
                    }

                    return new ItemSchedulerRequest
                    {
                        Id = r.Id,
                        AreaId = r.AreaId.Value,
                        BayId = r.BayId,
                        CreationDate = r.CreationDate,
                        IsInstant = r.IsInstant,
                        ItemId = r.ItemId.Value,
                        Lot = r.Lot,
                        OperationType = (OperationType)r.OperationType,
                        MaterialStatusId = r.MaterialStatusId,
                        PackageTypeId = r.PackageTypeId,
                        RegistrationNumber = r.RegistrationNumber,
                        RequestedQuantity = r.RequestedQuantity.Value,
                        ReservedQuantity = r.ReservedQuantity.Value,
                        Sub1 = r.Sub1,
                        Sub2 = r.Sub2,
                        Priority = r.Priority,
                        Status = (SchedulerRequestStatus)r.Status,
                    };

                case Common.DataModels.SchedulerRequestType.LoadingUnit:

                    if (r.LoadingUnitId.HasValue == false
                        ||
                        r.LoadingUnitTypeId.HasValue == false
                        ||
                        r.BayId.HasValue == false)
                    {
                        throw new System.Data.DataException("Loading unit request has missing mandatory fields (BayId, LoadingUnitTypeId, LoadingUnitId)");
                    }

                    return new LoadingUnitSchedulerRequest
                    {
                        Id = r.Id,
                        CreationDate = r.CreationDate,
                        IsInstant = r.IsInstant,
                        Priority = r.Priority,
                        BayId = r.BayId.Value,
                        LoadingUnitId = r.LoadingUnitId.Value,
                        LoadingUnitTypeId = r.LoadingUnitTypeId.Value,
                        Status = (SchedulerRequestStatus)r.Status,
                    };

                case Common.DataModels.SchedulerRequestType.ItemListRow:
                    return new ItemListRowSchedulerRequest
                    {
                        Id = r.Id,
                        AreaId = r.AreaId.Value,
                        BayId = r.BayId,
                        CreationDate = r.CreationDate,
                        IsInstant = r.IsInstant,
                        ItemId = r.ItemId.Value,
                        Lot = r.Lot,
                        OperationType = (OperationType)r.OperationType,
                        MaterialStatusId = r.MaterialStatusId,
                        PackageTypeId = r.PackageTypeId,
                        RegistrationNumber = r.RegistrationNumber,
                        RequestedQuantity = r.RequestedQuantity.Value,
                        ReservedQuantity = r.ReservedQuantity.Value,
                        Sub1 = r.Sub1,
                        Sub2 = r.Sub2,
                        Priority = r.Priority,
                        ListId = r.ListId.Value,
                        ListRowId = r.ListRowId.Value,
                        Status = (SchedulerRequestStatus)r.Status,
                    };

                default:
                    throw new NotSupportedException("The specified scheduler request type is not supported.");
            }
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

        #endregion
    }
}
