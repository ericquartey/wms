using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemCompartmentTypeProvider : IItemCompartmentTypeProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IItemCompartmentTypesDataService itemCompartmentTypesDataService;

        #endregion

        #region Constructors

        public ItemCompartmentTypeProvider(
            WMS.Data.WebAPI.Contracts.IItemCompartmentTypesDataService itemCompartmentTypesDataService)
        {
            this.itemCompartmentTypesDataService = itemCompartmentTypesDataService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult> CreateAsync(ItemCompartmentType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var itemCompartmentType = await this.itemCompartmentTypesDataService.CreateAsync(new WMS.Data.WebAPI.Contracts.ItemCompartmentType
                {
                    CompartmentTypeId = model.CompartmentTypeId,
                    ItemId = model.ItemId,
                    MaxCapacity = model.MaxCapacity
                });

                model.Id = itemCompartmentType.Id;

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, description: ex.Message);
            }
        }

        #endregion
    }
}
