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
    internal class ItemCompartmentTypeProvider : IItemCompartmentTypeProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public ItemCompartmentTypeProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemCompartmentType>> CreateAsync(ItemCompartmentType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = await this.dataContext.ItemsCompartmentTypes
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

            this.dataContext.ItemsCompartmentTypes.Add(
                new Common.DataModels.ItemCompartmentType
                {
                    CompartmentTypeId = model.CompartmentTypeId,
                    ItemId = model.ItemId,
                    MaxCapacity = model.MaxCapacity
                });

            if (await this.dataContext.SaveChangesAsync() <= 0)
            {
                return new CreationErrorOperationResult<ItemCompartmentType>();
            }

            return new SuccessOperationResult<ItemCompartmentType>(model);
        }

        public async Task<IOperationResult<ItemCompartmentType>> DeleteAsync(int itemId, int compartmentTypeId)
        {
            var item = await this.dataContext.ItemsCompartmentTypes
                .SingleOrDefaultAsync(ct => ct.CompartmentTypeId == compartmentTypeId && ct.ItemId == itemId);

            if (item == null)
            {
                return new NotFoundOperationResult<ItemCompartmentType>();
            }

            this.dataContext.ItemsCompartmentTypes.Remove(item);

            await this.dataContext.SaveChangesAsync();

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

            var existingModel = this.dataContext.ItemsCompartmentTypes
                .SingleOrDefault(ict =>
                                     ict.CompartmentTypeId == model.CompartmentTypeId
                                     &&
                                     ict.ItemId == model.ItemId);

            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemCompartmentType>();
            }

            if (existingModel.MaxCapacity.HasValue &&
                model.MaxCapacity.HasValue &&
                existingModel.MaxCapacity > model.MaxCapacity)
            {
                return new BadRequestOperationResult<ItemCompartmentType>($"New MaxCapacity {model.MaxCapacity} must be equal or greater than current MaxCapacity {existingModel.MaxCapacity}");
            }

            existingModel.MaxCapacity = model.MaxCapacity;
            this.dataContext.ItemsCompartmentTypes.Update(existingModel);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemCompartmentType>(model);
        }

        private IQueryable<ItemCompartmentType> GetAllBase()
        {
            return this.dataContext.ItemsCompartmentTypes
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
