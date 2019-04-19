using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class SchedulerRequestProvider : ISchedulerRequestProvider
    {
        #region Fields

        public const int InstantRequestPriority = 1;

        private readonly ICompartmentSchedulerProvider compartmentSchedulerProvider;

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public SchedulerRequestProvider(
            DatabaseContext dataContext,
            ICompartmentSchedulerProvider compartmentSchedulerProvider)
        {
            this.dataContext = dataContext;
            this.compartmentSchedulerProvider = compartmentSchedulerProvider;
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

        public async Task<ItemSchedulerRequest> FullyQualifyWithdrawalRequestAsync(
            int itemId,
            ItemWithdrawOptions options,
            ItemListRow row = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var aggregatedCompartments = this.dataContext.Compartments
                .Include(c => c.LoadingUnit)
                .ThenInclude(l => l.Cell)
                .ThenInclude(c => c.Aisle)
                .ThenInclude(a => a.Area)
                .Where(c =>
                    c.ItemId == itemId
                    &&
                    c.LoadingUnit.Cell.Aisle.Area.Id == options.AreaId
                    &&
                    (options.BayId.HasValue == false || c.LoadingUnit.Cell.Aisle.Area.Bays.Any(b => b.Id == options.BayId))
                    &&
                    (options.Sub1 == null || c.Sub1 == options.Sub1)
                    &&
                    (options.Sub2 == null || c.Sub2 == options.Sub2)
                    &&
                    (options.Lot == null || c.Lot == options.Lot)
                    &&
                    (options.PackageTypeId.HasValue == false || c.PackageTypeId == options.PackageTypeId)
                    &&
                    (options.MaterialStatusId.HasValue == false || c.MaterialStatusId == options.MaterialStatusId)
                    &&
                    (options.RegistrationNumber == null || c.RegistrationNumber == options.RegistrationNumber))
                .GroupBy(
                    x => new { x.Sub1, x.Sub2, x.Lot, x.PackageTypeId, x.MaterialStatusId, x.RegistrationNumber },
                    (key, group) => new
                    {
                        Key = key,
                        Availability = group.Sum(c => c.Stock - c.ReservedForPick + c.ReservedToStore),
                        Sub1 = key.Sub1,
                        Sub2 = key.Sub2,
                        Lot = key.Lot,
                        PackageTypeId = key.PackageTypeId,
                        MaterialStatusId = key.MaterialStatusId,
                        RegistrationNumber = key.RegistrationNumber,
                        FirstStoreDate = group.Min(c => c.FirstStoreDate)
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
                    FirstStoreDate = g.c.FirstStoreDate
                })
                .Where(x => x.Availability >= options.RequestedQuantity);

            var item = await this.dataContext.Items
                .Select(i => new { i.Id, i.ManagementType })
                .SingleAsync(i => i.Id == itemId);

            var bestCompartment = await this.compartmentSchedulerProvider
                .OrderPickCompartmentsByManagementType(compartmentSets, (ItemManagementType)item.ManagementType)
                .FirstOrDefaultAsync();

            if (bestCompartment == null)
            {
                return null;
            }

            var qualifiedRequest = ItemSchedulerRequest.FromWithdrawalOptions(itemId, options, row);

            qualifiedRequest.Lot = bestCompartment.Lot;
            qualifiedRequest.MaterialStatusId = bestCompartment.MaterialStatusId;
            qualifiedRequest.PackageTypeId = bestCompartment.PackageTypeId;
            qualifiedRequest.RegistrationNumber = bestCompartment.RegistrationNumber;
            qualifiedRequest.Sub1 = bestCompartment.Sub1;
            qualifiedRequest.Sub2 = bestCompartment.Sub2;
            qualifiedRequest.Priority = await this.ComputeRequestPriorityAsync(qualifiedRequest, row?.Priority);
            qualifiedRequest.Status = SchedulerRequestStatus.New;

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
               .Where(r => r.ItemId.HasValue == false || r.RequestedQuantity > r.ReservedQuantity)
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
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.SchedulerRequests.Find(model.Id);
            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemSchedulerRequest>(model);
        }

        public async Task<IOperationResult<LoadingUnitSchedulerRequest>> UpdateAsync(LoadingUnitSchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.SchedulerRequests.Find(model.Id);
            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<LoadingUnitSchedulerRequest>(model);
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

        private async Task<int?> ComputeRequestPriorityAsync(ItemSchedulerRequest schedulerRequest, int? rowPriority)
        {
            int? priority = null;
            if (rowPriority.HasValue)
            {
                priority = rowPriority.Value;
            }

            if (schedulerRequest.BayId.HasValue)
            {
                var bay = await this.dataContext.Bays.SingleAsync(b => b.Id == schedulerRequest.BayId.Value);
                priority = priority.HasValue ? priority + bay.Priority : bay.Priority;
            }

            return priority;
        }

        #endregion
    }
}
