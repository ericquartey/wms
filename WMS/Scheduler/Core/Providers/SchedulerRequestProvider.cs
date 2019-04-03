using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class SchedulerRequestProvider : ISchedulerRequestProvider
    {
        #region Fields

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

        public async Task<IOperationResult<SchedulerRequest>> CreateAsync(SchedulerRequest model)
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

            return new SuccessOperationResult<SchedulerRequest>(model);
        }

        public async Task<IEnumerable<SchedulerRequest>> CreateRangeAsync(IEnumerable<SchedulerRequest> models)
        {
            var requests = models.Select(r => CreateDataModel(r));

            await this.dataContext.AddRangeAsync(requests);

            await this.dataContext.SaveChangesAsync();

            return models;
        }

        public async Task<SchedulerRequest> FullyQualifyWithdrawalRequestAsync(
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
                .Where(r => r.ItemId == itemId);

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
                    Availability = g.c.Availability - g.r.Sum(r => r.RequestedQuantity - r.DispatchedQuantity),
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
                .OrderCompartmentsByManagementType(compartmentSets, (ItemManagementType)item.ManagementType)
                .FirstOrDefaultAsync();

            if (bestCompartment == null)
            {
                return null;
            }

            var qualifiedRequest = SchedulerRequest.FromWithdrawalOptions(itemId, options);

            qualifiedRequest.Lot = bestCompartment.Lot;
            qualifiedRequest.MaterialStatusId = bestCompartment.MaterialStatusId;
            qualifiedRequest.PackageTypeId = bestCompartment.PackageTypeId;
            qualifiedRequest.RegistrationNumber = bestCompartment.RegistrationNumber;
            qualifiedRequest.Sub1 = bestCompartment.Sub1;
            qualifiedRequest.Sub2 = bestCompartment.Sub2;
            qualifiedRequest.ListId = row?.ListId;
            qualifiedRequest.ListRowId = row?.Id;
            qualifiedRequest.Priority = await this.ComputeRequestPriorityAsync(qualifiedRequest, row?.Priority);

            return qualifiedRequest;
        }

        /// <summary>
        /// Gets all the pending requests that:
        /// - are not completed (dispatched qty is not equal to requested qty)
        /// - are already allocated to a bay
        /// - the allocated bay has buffer to accept new missions
        /// - if related to a list row, the row is marked for execution
        ///
        /// Requests are sorted by:
        /// - Instant first
        /// - All others after, giving priority to the lists ones that are already started
        ///
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task<IEnumerable<SchedulerRequest>> GetRequestsToProcessAsync()
        {
            return await this.dataContext.SchedulerRequests
               .Where(r => r.RequestedQuantity > r.DispatchedQuantity)
               .Where(r => r.BayId.HasValue
                            && r.Bay.LoadingUnitsBufferSize > r.Bay.Missions.Count(m =>
                                m.Status != Common.DataModels.MissionStatus.Completed
                                && m.Status != Common.DataModels.MissionStatus.Incomplete))
               .Where(r => r.ListRowId.HasValue == false
                    || (r.ListRow.Status == Common.DataModels.ItemListRowStatus.Executing
                    || r.ListRow.Status == Common.DataModels.ItemListRowStatus.Waiting))
               .OrderBy(r => r.Priority)
               .Select(r => new SchedulerRequest
               {
                   Id = r.Id,
                   AreaId = r.AreaId,
                   BayId = r.BayId,
                   CreationDate = r.CreationDate,
                   IsInstant = r.IsInstant,
                   ItemId = r.ItemId,
                   ListId = r.ListId,
                   ListRowId = r.ListRowId,
                   LoadingUnitId = r.LoadingUnitId,
                   LoadingUnitTypeId = r.LoadingUnitTypeId,
                   Lot = r.Lot,
                   Type = (OperationType)r.OperationType,
                   MaterialStatusId = r.MaterialStatusId,
                   PackageTypeId = r.PackageTypeId,
                   RegistrationNumber = r.RegistrationNumber,
                   RequestedQuantity = r.RequestedQuantity,
                   DispatchedQuantity = r.DispatchedQuantity,
                   Sub1 = r.Sub1,
                   Sub2 = r.Sub2,
                   Priority = r.Priority
               })
               .ToArrayAsync();
        }

        public async Task<IOperationResult<SchedulerRequest>> UpdateAsync(SchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.SchedulerRequests.Find(model.Id);
            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<SchedulerRequest>(model);
        }

        private static Common.DataModels.SchedulerRequest CreateDataModel(SchedulerRequest model)
        {
            return new Common.DataModels.SchedulerRequest
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
                OperationType = (Common.DataModels.OperationType)(int)model.Type,
                RequestedQuantity = model.RequestedQuantity,
                Sub1 = model.Sub1,
                Sub2 = model.Sub2,
                Priority = model.Priority
            };
        }

        private async Task<int?> ComputeRequestPriorityAsync(SchedulerRequest schedulerRequest, int? rowPriority)
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
