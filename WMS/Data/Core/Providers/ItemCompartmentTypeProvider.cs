using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class ItemCompartmentTypeProvider : IItemCompartmentTypeProvider
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

        public async Task<OperationResult<ItemCompartmentType>> CreateAsync(ItemCompartmentType model)
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
                if (existingModel.MaxCapacity == model.MaxCapacity)
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
                return new NotCreatedOperationResult<ItemCompartmentType>();
            }

            return new SuccessOperationResult<ItemCompartmentType>(model);
        }

        public async Task<OperationResult<ItemCompartmentType>> UpdateAsync(ItemCompartmentType model)
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

            existingModel.MaxCapacity = model.MaxCapacity;
            this.dataContext.ItemsCompartmentTypes.Update(existingModel);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemCompartmentType>(model);
        }

        #endregion
    }
}
