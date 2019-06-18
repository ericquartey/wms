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
    internal class SchedulerRequestExecutionProvider : BaseProvider, ISchedulerRequestExecutionProvider
    {
        #region Constructors

        public SchedulerRequestExecutionProvider(
            DatabaseContext dataContext,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemSchedulerRequest>> CreateAsync(ItemSchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = this.DataContext.SchedulerRequests
                .Add(CreateDataModel(model));

            if (await this.DataContext.SaveChangesAsync() > 0)
            {
                model.Id = entry.Entity.Id;

                this.NotificationService.PushCreate(model);
                this.NotificationService.PushUpdate(new Item { Id = model.ItemId });
            }

            return new SuccessOperationResult<ItemSchedulerRequest>(model);
        }

        public async Task<IOperationResult<LoadingUnitSchedulerRequest>> CreateAsync(LoadingUnitSchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = this.DataContext.SchedulerRequests
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

            if (await this.DataContext.SaveChangesAsync() > 0)
            {
                model.Id = entry.Entity.Id;

                this.NotificationService.PushCreate(model);
                this.NotificationService.PushUpdate(new LoadingUnit { Id = model.LoadingUnitId });
            }

            return new SuccessOperationResult<LoadingUnitSchedulerRequest>(model);
        }

        public async Task<IOperationResult<ItemListRowSchedulerRequest>> CreateAsync(ItemListRowSchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var entry = this.DataContext.SchedulerRequests
                    .Add(CreateDataModel(model));

                if (await this.DataContext.SaveChangesAsync() > 0)
                {
                    model.Id = entry.Entity.Id;

                    this.NotificationService.PushCreate(model);
                    this.NotificationService.PushUpdate(new ItemListRow { Id = model.ListRowId });
                }

                return new SuccessOperationResult<ItemListRowSchedulerRequest>(model);
            }
            catch (Exception ex)
            {
                return new CreationErrorOperationResult<ItemListRowSchedulerRequest>(ex);
            }
        }

        public async Task<IOperationResult<IEnumerable<ItemSchedulerRequest>>> CreateRangeAsync(IEnumerable<ItemSchedulerRequest> models)
        {
            if (models == null)
            {
                throw new ArgumentNullException(nameof(models));
            }

            try
            {
                foreach (var model in models)
                {
                    var entry = this.DataContext.SchedulerRequests
                        .Add(CreateDataModel(model));

                    if (await this.DataContext.SaveChangesAsync() > 0)
                    {
                        model.Id = entry.Entity.Id;

                        this.NotificationService.PushCreate(model);
                        this.NotificationService.PushUpdate(new Item { Id = model.ItemId });
                    }
                }

                return new SuccessOperationResult<IEnumerable<ItemSchedulerRequest>>(models);
            }
            catch (Exception ex)
            {
                return new CreationErrorOperationResult<IEnumerable<ItemSchedulerRequest>>(ex);
            }
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
            return await this.DataContext.SchedulerRequests
               .Where(r => r.Status == Common.DataModels.SchedulerRequestStatus.New)
               .Where(r => r.BayId.HasValue
                    && r.Bay.LoadingUnitsBufferSize > r.Bay.Missions.Count(m =>
                        m.Status != Common.DataModels.MissionStatus.Completed
                        && m.Status != Common.DataModels.MissionStatus.Incomplete))
               .Where(r => !r.ListRowId.HasValue
                    || (r.ListRow.Status == Common.DataModels.ItemListRowStatus.Executing
                    || r.ListRow.Status == Common.DataModels.ItemListRowStatus.Ready))
               .OrderBy(r => r.Priority)
               .Select(r => SelectRequest(r))
               .ToArrayAsync();
        }

        public async Task<IOperationResult<ItemSchedulerRequest>> UpdateAsync(ItemSchedulerRequest model)
        {
            var result = await this.UpdateAsync<Common.DataModels.SchedulerRequest, ItemSchedulerRequest, int>(
                model,
                this.DataContext.SchedulerRequests,
                this.DataContext);

            this.NotificationService.PushUpdate(model);
            this.NotificationService.PushUpdate(new Item { Id = model.ItemId });

            return result;
        }

        public async Task<IOperationResult<LoadingUnitSchedulerRequest>> UpdateAsync(LoadingUnitSchedulerRequest model)
        {
            var result = await this.UpdateAsync<Common.DataModels.SchedulerRequest, LoadingUnitSchedulerRequest, int>(
                model,
                this.DataContext.SchedulerRequests,
                this.DataContext);

            this.NotificationService.PushUpdate(model);
            this.NotificationService.PushUpdate(new LoadingUnit { Id = model.LoadingUnitId });

            return result;
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

                    if (!r.RequestedQuantity.HasValue
                        ||
                        !r.ReservedQuantity.HasValue)
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

                    if (!r.LoadingUnitId.HasValue
                        ||
                        !r.LoadingUnitTypeId.HasValue
                        ||
                        !r.BayId.HasValue)
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

        #endregion
    }
}
