using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemCompartmentTypeProvider : BaseProvider, IItemCompartmentTypeProvider
    {
        #region Fields

        private readonly IMapper mapper;

        #endregion

        #region Constructors

        public ItemCompartmentTypeProvider(
            DatabaseContext dataContext,
            IMapper mapper,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.mapper = mapper;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemCompartmentType>> CreateAsync(ItemCompartmentType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = await this.DataContext.ItemsCompartmentTypes
                .SingleOrDefaultAsync(ict =>
                    ict.CompartmentTypeId == model.CompartmentTypeId
                    &&
                    ict.ItemId == model.ItemId);

            if (existingModel != null)
            {
                if (existingModel.MaxCapacity.CompareTo(model.MaxCapacity) == 0)
                {
                    return new SuccessOperationResult<ItemCompartmentType>(model);
                }

                return new AlreadyCreatedOperationResult<ItemCompartmentType>();
            }

            this.DataContext.ItemsCompartmentTypes.Add(
                this.mapper.Map<Common.DataModels.ItemCompartmentType>(model));

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount <= 0)
            {
                return new CreationErrorOperationResult<ItemCompartmentType>();
            }

            this.NotificationService.PushCreate(model);
            this.NotificationService.PushUpdate(new Item { Id = model.ItemId });
            this.NotificationService.PushUpdate(new CompartmentType { Id = model.CompartmentTypeId });

            return new SuccessOperationResult<ItemCompartmentType>(model);
        }

        public async Task<IOperationResult<IEnumerable<ItemCompartmentType>>> CreateItemCompartmentTypesRangeByItemIdAsync(
                     IEnumerable<ItemCompartmentType> itemCompartmentTypes)
        {
            if (itemCompartmentTypes == null)
            {
                throw new ArgumentNullException(nameof(itemCompartmentTypes));
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var itemCompartmentType in itemCompartmentTypes)
                {
                    var result = await this.CreateAsync(itemCompartmentType);
                    if (!result.Success)
                    {
                        return new CreationErrorOperationResult<IEnumerable<ItemCompartmentType>>(result.Description);
                    }

                    itemCompartmentType.Id = result.Entity.Id;
                }

                scope.Complete();
                return new SuccessOperationResult<IEnumerable<ItemCompartmentType>>(itemCompartmentTypes);
            }
        }

        public async Task<IOperationResult<ItemCompartmentType>> DeleteAsync(int itemId, int compartmentTypeId)
        {
            var existingModel = await this.DataContext.ItemsCompartmentTypes
                .SingleOrDefaultAsync(ct => ct.CompartmentTypeId == compartmentTypeId && ct.ItemId == itemId);

            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemCompartmentType>();
            }

            this.DataContext.ItemsCompartmentTypes.Remove(existingModel);

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                this.NotificationService.PushDelete(typeof(ItemCompartmentType));
                this.NotificationService.PushUpdate(new Item { Id = itemId });
                this.NotificationService.PushUpdate(new CompartmentType { Id = compartmentTypeId });

                return new SuccessOperationResult<ItemCompartmentType>(new ItemCompartmentType { CompartmentTypeId = compartmentTypeId, ItemId = itemId });
            }

            return new UnprocessableEntityOperationResult<ItemCompartmentType>();
        }

        public async Task<IOperationResult<IEnumerable<ItemCompartmentType>>> GetAllByItemIdAsync(int id)
        {
            var itemCompartmentTypes = await this.GetAllBase().Where(ct => ct.ItemId == id).ToListAsync();

            return new SuccessOperationResult<IEnumerable<ItemCompartmentType>>(itemCompartmentTypes);
        }

        public async Task<IOperationResult<IEnumerable<ItemCompartmentType>>> GetAllUnassociatedByItemIdAsync(int id)
        {
            try
            {
                var itemCompartmentTypes = await this.DataContext.CompartmentTypes.Where(
                        ct => !this.DataContext.ItemsCompartmentTypes.Where(ict => ict.ItemId == id)
                            .Select(ict => ict.CompartmentTypeId).Contains(ct.Id))
                    .Select(ic => new ItemCompartmentType
                    {
                        CompartmentTypeId = ic.Id,
                        Depth = ic.Depth,
                        Width = ic.Width,
                        CompartmentsCount = ic.Compartments.Count(),
                        EmptyCompartmentsCount = ic.Compartments.Count(c => c.Stock.Equals(0)),
                    })
                    .ToArrayAsync();

                return new SuccessOperationResult<IEnumerable<ItemCompartmentType>>(itemCompartmentTypes);
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityOperationResult<IEnumerable<ItemCompartmentType>>(ex);
            }
        }

        public async Task<IOperationResult<ItemCompartmentType>> UpdateAsync(ItemCompartmentType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.DataContext.ItemsCompartmentTypes
                .SingleOrDefault(ict =>
                                     ict.CompartmentTypeId == model.CompartmentTypeId
                                     &&
                                     ict.ItemId == model.ItemId);

            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemCompartmentType>();
            }

            if (existingModel.MaxCapacity > model.MaxCapacity)
            {
                return new BadRequestOperationResult<ItemCompartmentType>(
                    string.Format(
                        Resources.Errors.NewMaxCapacityMustBeEqualOrGreaterThanCurrent,
                        model.MaxCapacity,
                        existingModel.MaxCapacity));
            }

            existingModel.MaxCapacity = model.MaxCapacity;
            this.DataContext.ItemsCompartmentTypes.Update(existingModel);
            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                this.NotificationService.PushUpdate(model);
                this.NotificationService.PushUpdate(new Item { Id = model.ItemId });
                this.NotificationService.PushUpdate(new CompartmentType { Id = model.CompartmentTypeId });
            }

            return new SuccessOperationResult<ItemCompartmentType>(model);
        }

        private IQueryable<ItemCompartmentType> GetAllBase()
        {
            return this.DataContext.ItemsCompartmentTypes
                .Select(ct => new ItemCompartmentType
                {
                    CompartmentTypeId = ct.CompartmentTypeId,
                    ItemId = ct.ItemId,
                    MaxCapacity = ct.MaxCapacity,
                });
        }

        #endregion
    }
}
