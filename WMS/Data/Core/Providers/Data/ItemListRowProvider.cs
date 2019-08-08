using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemListRowProvider : BaseProvider, IItemListRowProvider
    {
        #region Fields

        private readonly IMapper mapper;

        private readonly IItemListProvider itemListProvider;

        #endregion

        #region Constructors

        public ItemListRowProvider(
            DatabaseContext dataContext,
            IMapper mapper,
            IItemListProvider itemListProvider,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.mapper = mapper;
            this.itemListProvider = itemListProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemListRowDetails>> CreateAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var validationError = model.ValidateBusinessModel(this.DataContext.ItemListRows);
            if (!string.IsNullOrEmpty(validationError))
            {
                return new BadRequestOperationResult<ItemListRowDetails>(
                    validationError,
                    model);
            }

            var list = await this.itemListProvider.GetByIdAsync(model.ItemListId);
            if (!list.CanExecuteOperation(nameof(ItemListPolicy.AddRow)))
            {
                return new BadRequestOperationResult<ItemListRowDetails>(
                    list.GetCanExecuteOperationReason(nameof(ItemListPolicy.AddRow)));
            }

            var entry = await this.DataContext.ItemListRows.AddAsync(
                this.mapper.Map<Common.DataModels.ItemListRow>(model));

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount <= 0)
            {
                return new CreationErrorOperationResult<ItemListRowDetails>();
            }

            var createdModel = await this.GetByIdAsync(entry.Entity.Id);

            this.NotificationService.PushCreate(createdModel);
            this.NotificationService.PushUpdate(new ItemList { Id = createdModel.ItemListId }, createdModel);
            this.NotificationService.PushUpdate(new Item { Id = createdModel.ItemId }, createdModel);

            return new SuccessOperationResult<ItemListRowDetails>(createdModel);
        }

        public async Task<IOperationResult<ItemListRowDetails>> DeleteAsync(int id)
        {
            var existingModel = await this.GetByIdAsync(id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemListRowDetails>();
            }

            if (!existingModel.CanDelete())
            {
                return new UnprocessableEntityOperationResult<ItemListRowDetails>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            this.DataContext.Remove(new Common.DataModels.ItemListRow { Id = id });

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                this.NotificationService.PushDelete(existingModel);
                this.NotificationService.PushUpdate(new ItemList { Id = existingModel.ItemListId }, existingModel);
                this.NotificationService.PushUpdate(new Item { Id = existingModel.ItemId }, existingModel);

                return new SuccessOperationResult<ItemListRowDetails>(existingModel);
            }

            return new UnprocessableEntityOperationResult<ItemListRowDetails>();
        }

        public async Task<IEnumerable<ItemListRow>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var models = await this.GetAllBase()
                .ToArrayAsync<ItemListRow, Common.DataModels.ItemListRow>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));

            foreach (var model in models)
            {
                SetPolicies(model);
            }

            return models;
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<ItemListRow, Common.DataModels.ItemListRow>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<ItemListRowDetails> GetByIdAsync(int id)
        {
            var model = await this.GetAllDetailsBase()
                .SingleOrDefaultAsync(i => i.Id == id);

            if (model != null)
            {
                SetPolicies(model);
            }

            return model;
        }

        public async Task<IEnumerable<ItemListRow>> GetByItemListIdAsync(int id)
        {
            var models = await this.GetAllBase()
                .Where(l => l.ItemListId == id)
                .ToArrayAsync();

            foreach (var model in models)
            {
                SetPolicies(model);
            }

            return models;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                propertyName,
                this.DataContext.ItemListRows,
                this.GetAllBase());
        }

        public async Task<IOperationResult<ItemListRowDetails>> UpdateAsync(ItemListRowDetails model)
        {
            var result = await this.UpdateAsync(
                model,
                this.DataContext.ItemListRows,
                this.DataContext);

            this.NotificationService.PushUpdate(model);
            this.NotificationService.PushUpdate(new ItemList { Id = model.ItemListId }, model);
            this.NotificationService.PushUpdate(new Item { Id = model.ItemId }, model);

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4058:Overloads with a \"StringComparison\" parameter should be used",
            Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<ItemListRow, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsDouble = double.TryParse(search, out var searchAsDouble);
            var successConversionAsInt = int.TryParse(search, out var searchAsInt);

            return (r) =>
                (r.Code != null && r.Code.Contains(search))
                || (r.ItemDescription != null && r.ItemDescription.Contains(search))
                || (r.ItemUnitMeasure != null && r.ItemUnitMeasure.Contains(search))
                || (r.MaterialStatusDescription != null && r.MaterialStatusDescription.Contains(search))
                || (successConversionAsInt
                    && Equals(r.Priority, searchAsInt))
                || (successConversionAsDouble
                    && (Equals(r.RequestedQuantity, searchAsDouble)
                        || Equals(r.DispatchedQuantity, searchAsDouble)));
        }

        private static void SetPolicies(BasePolicyModel model)
        {
            model.AddPolicy((model as IItemListRowUpdatePolicy).ComputeUpdatePolicy());
            model.AddPolicy((model as IItemListRowDeletePolicy).ComputeDeletePolicy());
            model.AddPolicy((model as IItemListRowExecutePolicy).ComputeExecutePolicy());
        }

        private IQueryable<ItemListRow> GetAllBase()
        {
            return this.DataContext.ItemListRows
                .Select(l => new ItemListRow
                {
                    Id = l.Id,
                    Code = l.Code,
                    Priority = l.Priority,
                    ItemDescription = l.Item.Description,
                    ItemCode = l.Item.Code,
                    RequestedQuantity = l.RequestedQuantity,
                    DispatchedQuantity = l.DispatchedQuantity,
                    ItemListId = l.ItemListId,
                    Status = l.Status,
                    MaterialStatusDescription = l.MaterialStatus.Description,
                    CreationDate = l.CreationDate,
                    ItemUnitMeasure = l.Item.MeasureUnit.Description,
                    ActiveSchedulerRequestsCount = l.SchedulerRequests.Count(),
                    ActiveMissionsCount = l.MissionOperations.Count(
                        m => m.Status != Enums.MissionOperationStatus.Completed &&
                            m.Status != Enums.MissionOperationStatus.Incomplete),
                    Machines = this.DataContext.Compartments.Where(c => c.ItemId == l.ItemId)
                        .Join(
                            this.DataContext.Machines,
                            j => j.LoadingUnit.Cell.AisleId,
                            m => m.AisleId,
                            (j, m) => new
                            {
                                Machine = m,
                            })
                        .Select(x => x.Machine).Distinct()
                        .Select(m1 => new Machine
                        {
                            Id = m1.Id,
                            ErrorTime = m1.ErrorTime,
                            Image = m1.Image,
                            Model = m1.Model,
                            Nickname = m1.Nickname,
                        }),
                });
        }

        private IQueryable<ItemListRowDetails> GetAllDetailsBase()
        {
            return this.DataContext.ItemListRows
                .Select(l => new ItemListRowDetails
                {
                    Id = l.Id,
                    Code = l.Code,
                    Priority = l.Priority,
                    ItemId = l.Item.Id,
                    ItemImage = l.Item.Image,
                    RequestedQuantity = l.RequestedQuantity,
                    DispatchedQuantity = l.DispatchedQuantity,
                    Status = l.Status,
                    ItemDescription = l.Item.Description,
                    CreationDate = l.CreationDate,
                    ItemListCode = l.ItemList.Code,
                    ItemListDescription = l.ItemList.Description,
                    ItemListId = l.ItemListId,
                    ItemListType = l.ItemList.ItemListType,
                    CompletionDate = l.CompletionDate,
                    LastExecutionDate = l.LastExecutionDate,
                    LastModificationDate = l.LastModificationDate,
                    Lot = l.Lot,
                    RegistrationNumber = l.RegistrationNumber,
                    Sub1 = l.Sub1,
                    Sub2 = l.Sub2,
                    PackageTypeId = l.PackageTypeId,
                    MaterialStatusId = l.MaterialStatusId,
                    ItemUnitMeasure = l.Item.MeasureUnit.Description,

                    ActiveSchedulerRequestsCount = l.SchedulerRequests.Count(),
                    ActiveMissionsCount = l.MissionOperations.Count(
                        m => m.Status != Enums.MissionOperationStatus.Completed &&
                            m.Status != Enums.MissionOperationStatus.Incomplete),
                });
        }

        #endregion
    }
}
