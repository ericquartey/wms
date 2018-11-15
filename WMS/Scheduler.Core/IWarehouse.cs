using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface IWarehouse
    {
        #region Methods

        Task<WarehouseHandlingRequest> Withdraw(int itemId, int quantity, string lot, string registrationNumber, string sub1, string sub2);

        #endregion Methods
    }
}
