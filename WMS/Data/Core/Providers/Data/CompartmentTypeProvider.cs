using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
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

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class CompartmentTypeProvider : BaseProvider, ICompartmentTypeProvider
    {
        #region Fields

        private readonly IGlobalSettingsProvider globalSettingsProvider;

        private readonly IMapper mapper;

        private readonly IItemCompartmentTypeProvider itemCompartmentTypeProvider;

        #endregion

        #region Constructors

        public CompartmentTypeProvider(
            DatabaseContext dataContext,
            IMapper mapper,
            IItemCompartmentTypeProvider itemCompartmentTypeProvider,
            IGlobalSettingsProvider globalSettingsProvider,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.mapper = mapper;
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
            this.globalSettingsProvider = globalSettingsProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<CompartmentType>> CreateAsync(CompartmentType model)
        {
            return await this.CreateAsync(model, null, null);
        }

        public async Task<IOperationResult<CompartmentType>> CreateAsync(
            CompartmentType model,
            int? itemId,
            double? maxCapacity)
        {
            if (model == null ||
                !model.Depth.HasValue ||
                !model.Width.HasValue)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var validationError = model.ValidateBusinessModel(this.DataContext.CompartmentTypes);
            if (!string.IsNullOrEmpty(validationError))
            {
                return new BadRequestOperationResult<CompartmentType>(
                    validationError,
                    model);
            }

            var globalSettings = await this.globalSettingsProvider.GetGlobalSettingsAsync();
            if (!model.ApplyCorrection(globalSettings.MinStepCompartment))
            {
                return new CreationErrorOperationResult<CompartmentType>();
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var existingCompartmentType =
                    await this.DataContext.CompartmentTypes
                        .SingleOrDefaultAsync(
                            ct =>
                                ((int)ct.Width == (int)model.Width && (int)ct.Depth == (int)model.Depth)
                                ||
                                ((int)ct.Width == (int)model.Depth && (int)ct.Depth == (int)model.Width));

                if (existingCompartmentType == null)
                {
                    var entry = await this.DataContext.CompartmentTypes.AddAsync(
                        this.mapper.Map<Common.DataModels.CompartmentType>(model));

                    if (await this.DataContext.SaveChangesAsync() <= 0)
                    {
                        return new CreationErrorOperationResult<CompartmentType>();
                    }

                    model.Id = entry.Entity.Id;

                    this.NotificationService.PushCreate(model);
                }
                else
                {
                    model.Id = existingCompartmentType.Id;
                }

                if (itemId.HasValue && maxCapacity.HasValue)
                {
                    var result = await this.CreateOrUpdateItemCompartmentTypeAsync(
                                     itemId.Value,
                                     maxCapacity.Value,
                                     model.Id);

                    if (!result.Success)
                    {
                        return new CreationErrorOperationResult<CompartmentType>();
                    }
                }

                scope.Complete();
            }

            return new SuccessOperationResult<CompartmentType>(model);
        }

        public async Task<IOperationResult<CompartmentType>> CreateIfNotExistsAsync(
            CompartmentType model,
            int? itemId,
            double? maxCapacity)
        {
            var existingCompartmentType =
                await this.DataContext.CompartmentTypes
                    .SingleOrDefaultAsync(
                        ct =>
                            ((int)ct.Width == (int)model.Width && (int)ct.Depth == (int)model.Depth)
                            ||
                            ((int)ct.Width == (int)model.Depth && (int)ct.Depth == (int)model.Width));

            if (existingCompartmentType != null && !itemId.HasValue)
            {
                return new CreationErrorOperationResult<CompartmentType>(Resources.Errors.DuplicateCompartmentType);
            }

            return await this.CreateAsync(model, itemId, maxCapacity);
        }

        public async Task<IOperationResult<CompartmentType>> DeleteAsync(int id)
        {
            var existingModel = await this.GetByIdAsync(id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<CompartmentType>();
            }

            if (!existingModel.CanDelete())
            {
                return new UnprocessableEntityOperationResult<CompartmentType>();
            }

            this.DataContext.Remove(new Common.DataModels.CompartmentType { Id = id });

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                this.NotificationService.PushDelete(existingModel);
                return new SuccessOperationResult<CompartmentType>(existingModel);
            }

            return new UnprocessableEntityOperationResult<CompartmentType>();
        }

        public async Task<IEnumerable<CompartmentType>> GetAllAsync(
                    int skip,
                    int take,
                    IEnumerable<SortOption> orderBySortOptions = null,
                    string whereString = null,
                    string searchString = null)
        {
            var models = await this.GetAllBase()
                .ToArrayAsync<CompartmentType, Common.DataModels.CompartmentType>(
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
                .CountAsync<CompartmentType, Common.DataModels.CompartmentType>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<CompartmentType> GetByIdAsync(int id)
        {
            var model = await this.GetAllBase()
                .SingleOrDefaultAsync(a => a.Id == id);

            if (model != null)
            {
                SetPolicies(model);
            }

            return model;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                propertyName,
                this.DataContext.CompartmentTypes,
                this.GetAllBase());
        }

        private static Expression<Func<CompartmentType, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successDouble = double.TryParse(search, out var resultDouble);
            var successInt = int.TryParse(search, out var resultInt);

            return (ct) => (successDouble
                &&
                (Equals(ct.Width, resultDouble)
                ||
                Equals(ct.Depth, resultDouble)))
                ||
                (successInt &&
                Equals(ct.CompartmentsCount, resultInt));
        }

        private static void SetPolicies(BaseModel<int> model)
        {
            model.AddPolicy((model as ICompartmentTypeDeletePolicy).ComputeDeletePolicy());
        }

        private async Task<IOperationResult<ItemCompartmentType>> CreateOrUpdateItemCompartmentTypeAsync(
                    int itemId,
                    double maxCapacity,
                    int compartmentTypeId)
        {
            var existingIcTModel =
                await this.DataContext.ItemsCompartmentTypes
                    .SingleOrDefaultAsync(
                        ict =>
                            ict.CompartmentTypeId == compartmentTypeId
                            &&
                            ict.ItemId == itemId);

            if (existingIcTModel != null)
            {
                var updateResult = await this.itemCompartmentTypeProvider.UpdateAsync(
                    new ItemCompartmentType
                    {
                        ItemId = existingIcTModel.ItemId,
                        MaxCapacity = maxCapacity,
                        CompartmentTypeId = existingIcTModel.CompartmentTypeId,
                    });

                return updateResult;
            }

            var createResult = await this.itemCompartmentTypeProvider.CreateAsync(
                new ItemCompartmentType
                {
                    ItemId = itemId,
                    MaxCapacity = maxCapacity,
                    CompartmentTypeId = compartmentTypeId,
                });

            this.NotificationService.PushCreate(typeof(ItemCompartmentType));

            return createResult;
        }

        private IQueryable<CompartmentType> GetAllBase()
        {
            return this.DataContext.CompartmentTypes
                .Select(ct => new CompartmentType
                {
                    Id = ct.Id,
                    Depth = ct.Depth,
                    Width = ct.Width,
                    CompartmentsCount = ct.Compartments.Count(),
                    EmptyCompartmentsCount = ct.Compartments.Count(c => c.Stock.Equals(0)),
                    ItemCompartmentsCount = ct.ItemsCompartmentTypes.Count(),
                });
        }

        #endregion
    }
}
