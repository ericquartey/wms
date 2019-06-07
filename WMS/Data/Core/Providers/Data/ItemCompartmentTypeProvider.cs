using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemCompartmentTypeProvider : BaseProvider, IItemCompartmentTypeProvider
    {
        #region Constructors

        public ItemCompartmentTypeProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
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
                if (existingModel.MaxCapacity?.CompareTo(model.MaxCapacity) == 0)
                {
                    return new SuccessOperationResult<ItemCompartmentType>(model);
                }

                return new AlreadyCreatedOperationResult<ItemCompartmentType>();
            }

            this.DataContext.ItemsCompartmentTypes.Add(
                new Common.DataModels.ItemCompartmentType
                {
                    CompartmentTypeId = model.CompartmentTypeId,
                    ItemId = model.ItemId,
                    MaxCapacity = model.MaxCapacity
                });

            if (await this.DataContext.SaveChangesAsync() <= 0)
            {
                return new CreationErrorOperationResult<ItemCompartmentType>();
            }

            this.NotificationService.PushCreate(model);

            return new SuccessOperationResult<ItemCompartmentType>(model);
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
            await this.DataContext.SaveChangesAsync();

            this.NotificationService.PushDelete(typeof(ItemCompartmentType));

            return new SuccessOperationResult<ItemCompartmentType>();
        }

        public async Task<IOperationResult<IEnumerable<ItemCompartmentType>>> GetAllByCompartmentTypeIdAsync(int id)
        {
            try
            {
                var itemCompartmentTypes = await this.GetAllBase().Where(ct => ct.CompartmentTypeId == id).ToListAsync();

                return new SuccessOperationResult<IEnumerable<ItemCompartmentType>>(itemCompartmentTypes);
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityOperationResult<IEnumerable<ItemCompartmentType>>(ex);
            }
        }

        public async Task<IOperationResult<IEnumerable<ItemCompartmentType>>> GetAllByItemIdAsync(int id)
        {
            try
            {
                var itemCompartmentTypes = await this.GetAllBase().Where(ct => ct.ItemId == id).ToListAsync();

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

            existingModel.MaxCapacity = model.MaxCapacity;
            this.DataContext.ItemsCompartmentTypes.Update(existingModel);
            await this.DataContext.SaveChangesAsync();

            this.NotificationService.PushUpdate(model);

            return new SuccessOperationResult<ItemCompartmentType>(model);
        }

        private IQueryable<ItemCompartmentType> GetAllBase()
        {
            return this.DataContext.ItemsCompartmentTypes
                .Select(ct => new ItemCompartmentType
                {
                    CompartmentTypeId = ct.CompartmentTypeId,
                    ItemId = ct.ItemId,
                    MaxCapacity = ct.MaxCapacity
                });
        }

        #endregion
    }
}
