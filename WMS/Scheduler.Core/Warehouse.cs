using System.Threading.Tasks;
using Ferretto.Common.BusinessProviders;

namespace Ferretto.WMS.Scheduler.Core
{
    public class Warehouse : IWarehouse
    {
        #region Fields

        private readonly IItemProvider itemProvider;

        #endregion Fields

        #region Constructors

        public Warehouse(
           IItemProvider itemProvider
           )
        {
            this.itemProvider = itemProvider;
        }

        #endregion Constructors

        #region Methods

        public async Task<WarehouseHandlingRequest> Withdraw(int itemId, int quantity, string lot, string registrationNumber, string sub1, string sub2)
        {
            if (quantity <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(quantity));
            }

            var availableQuantity = await this.itemProvider.GetAvailableQuantity(itemId, lot, registrationNumber, sub1, sub2);
            if (availableQuantity >= quantity)
            {
                var warehouseRequest = new WarehouseHandlingRequest();

                // TODO: save request to database

                return warehouseRequest;
            }

            return null;
        }

        #endregion Methods
    }
}
