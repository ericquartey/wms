using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
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

        private readonly IItemCompartmentTypeProvider itemCompartmentTypeProvider;

        #endregion

        #region Constructors

        public CompartmentTypeProvider(
            DatabaseContext dataContext,
            IItemCompartmentTypeProvider itemCompartmentTypeProvider,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
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
                !model.Height.HasValue ||
                !model.Width.HasValue)
            {
                throw new ArgumentNullException(nameof(model));
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var existingCompartmentType =
                    await this.DataContext.CompartmentTypes
                        .SingleOrDefaultAsync(
                            ct =>
                                ((int)ct.Width == (int)model.Width && (int)ct.Height == (int)model.Height)
                                ||
                                ((int)ct.Width == (int)model.Height && (int)ct.Height == (int)model.Width));

                if (existingCompartmentType == null)
                {
                    var entry = await this.DataContext.CompartmentTypes.AddAsync(
                                    new Common.DataModels.CompartmentType
                                    {
                                        Height = model.Height.Value,
                                        Width = model.Width.Value
                                    });

                    if (await this.DataContext.SaveChangesAsync() <= 0)
                    {
                        return new CreationErrorOperationResult<CompartmentType>();
                    }

                    existingCompartmentType = entry.Entity;
                    model.Id = entry.Entity.Id;

                    this.NotificationService.PushCreate(model);
                }
                else
                {
                    model.Id = existingCompartmentType.Id;
                    if (itemId.HasValue == false)
                    {
                        return new CreationErrorOperationResult<CompartmentType>(Common.Resources.Errors.DuplicateCompartmentType);
                    }
                }

                if (itemId.HasValue)
                {
                    var result = await this.CreateOrUpdateItemCompartmentTypeAsync(
                                     itemId.Value,
                                     maxCapacity,
                                     existingCompartmentType.Id);

                    if (!result.Success)
                    {
                        return new CreationErrorOperationResult<CompartmentType>();
                    }
                }

                scope.Complete();
            }

            return new SuccessOperationResult<CompartmentType>(model);
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

            SetPolicies(model);
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
                Equals(ct.Height, resultDouble)))
                ||
                (successInt &&
                Equals(ct.CompartmentsCount, resultInt));
        }

        private static void SetPolicies(BaseModel<int> model)
        {
            if (model == null)
            {
                return;
            }

            model.AddPolicy((model as ICompartmentTypeDeletePolicy).ComputeDeletePolicy());
        }

        private async Task<IOperationResult<ItemCompartmentType>> CreateOrUpdateItemCompartmentTypeAsync(
                    int itemId,
            double? maxCapacity,
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
                        CompartmentTypeId = existingIcTModel.CompartmentTypeId
                    });

                return updateResult;
            }

            var createResult = await this.itemCompartmentTypeProvider.CreateAsync(
                new ItemCompartmentType
                {
                    ItemId = itemId,
                    MaxCapacity = maxCapacity,
                    CompartmentTypeId = compartmentTypeId
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
                    Height = ct.Height,
                    Width = ct.Width,
                    CompartmentsCount = ct.Compartments.Count(),
                    EmptyCompartmentsCount = ct.Compartments.Count(c => c.Stock.Equals(0)),
                    ItemCompartmentsCount = ct.ItemsCompartmentTypes.Count(),
                });
        }

        #endregion
    }
}
