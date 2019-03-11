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

        public async Task<SchedulerRequest> FullyQualifyWithdrawalRequestAsync(SchedulerRequest schedulerRequest)
        {
            if (schedulerRequest == null)
            {
                throw new ArgumentNullException(nameof(schedulerRequest));
            }

            if (schedulerRequest.Type != OperationType.Withdrawal)
            {
                throw new ArgumentException("Only withdrawal requests are supported.", nameof(schedulerRequest));
            }

            var aggregatedCompartments = this.dataContext.Compartments
                .Include(c => c.LoadingUnit)
                .ThenInclude(l => l.Cell)
                .ThenInclude(c => c.Aisle)
                .ThenInclude(a => a.Area)
                .Where(c =>
                    c.ItemId == schedulerRequest.ItemId
                    &&
                    c.LoadingUnit.Cell.Aisle.Area.Id == schedulerRequest.AreaId
                    &&
                    (schedulerRequest.BayId.HasValue == false || c.LoadingUnit.Cell.Aisle.Area.Bays.Any(b => b.Id == schedulerRequest.BayId))
                    &&
                    (schedulerRequest.Sub1 == null || c.Sub1 == schedulerRequest.Sub1)
                    &&
                    (schedulerRequest.Sub2 == null || c.Sub2 == schedulerRequest.Sub2)
                    &&
                    (schedulerRequest.Lot == null || c.Lot == schedulerRequest.Lot)
                    &&
                    (schedulerRequest.PackageTypeId.HasValue == false || c.PackageTypeId == schedulerRequest.PackageTypeId)
                    &&
                    (schedulerRequest.MaterialStatusId.HasValue == false || c.MaterialStatusId == schedulerRequest.MaterialStatusId)
                    &&
                    (schedulerRequest.RegistrationNumber == null || c.RegistrationNumber == schedulerRequest.RegistrationNumber))
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
                .Where(r => r.ItemId == schedulerRequest.ItemId);

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
                .Where(x => x.Availability >= schedulerRequest.RequestedQuantity);

            var item = await this.dataContext.Items
                .Select(i => new { i.Id, i.ManagementType })
                .SingleAsync(i => i.Id == schedulerRequest.ItemId);

            var orderedCompartmentSets = this.compartmentSchedulerProvider
                .OrderCompartmentsByManagementType(compartmentSets, (ItemManagementType)item.ManagementType);

            return await orderedCompartmentSets
                  .Select(
                  c => new SchedulerRequest(schedulerRequest)
                  {
                      Lot = c.Lot,
                      MaterialStatusId = c.MaterialStatusId,
                      PackageTypeId = c.PackageTypeId,
                      RegistrationNumber = c.RegistrationNumber,
                      Sub1 = c.Sub1,
                      Sub2 = c.Sub2
                  })
              .FirstOrDefaultAsync();
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
               .Where(r =>
                    r.BayId.HasValue
                    &&
                    r.RequestedQuantity > r.DispatchedQuantity
                    &&
                    r.Bay.LoadingUnitsBufferSize > r.Bay.Missions.Count
                    &&
                    (r.ListRowId.HasValue == false || r.ListRow.Status == Common.DataModels.ItemListRowStatus.Executing))
               .OrderBy(r => r.ListId.HasValue ? r.List.Priority : int.MaxValue)
               .ThenBy(r => r.ListRowId.HasValue ? r.ListRow.Priority : int.MaxValue)
               .Select(r => new SchedulerRequest
               {
                   Id = r.Id,
                   AreaId = r.AreaId,
                   BayId = r.BayId,
                   CreationDate = r.CreationDate,
                   IsInstant = r.IsInstant,
                   ListRowStatus = r.ListRow != null ? (ListRowStatus)r.ListRow.Status : ListRowStatus.NotSpecified,
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
                   Sub2 = r.Sub2
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
                Sub2 = model.Sub2
            };
        }

        #endregion
    }
}
