using System.Threading.Tasks;

namespace Ferretto.VW.App.Modules.Operator
{
    public interface IWmsDataProvider
    {
        #region Properties

        bool IsEnabled { get; }

        bool IsSocketLinkEnabled { get; }

        #endregion

        #region Methods

        Task CheckAsync(int itemId, int compartmentId, string lot = null, string serialNumber = null, string userName = null);

        Task<string> GetItemImagePathAsync(int itemId);

        Task PickAsync(int itemId, double requestedQuantity, int? reasonId = null, string reasonNotes = null, int? compartmentId = null, string lot = null, string serialNumber = null, string userName = null, int? orderId = null);

        Task PutAsync(int itemId, double requestedQuantity, int? reasonId = null, string reasonNotes = null, int? compartmentId = null, string lot = null, string serialNumber = null, string userName = null, int? orderId = null);

        void Start();

        Task UpdateItemStockAfterFillingAsync(int compartmentId, int itemId, double quantity, int? reasonId = null, string reasonNotes = null, string lot = null, string serialNumber = null, string userName = null, int? orderId = null);

        Task UpdateItemStockAfterPickingAsync(int compartmentId, int itemId, double quantity, int? reasonId = null, string reasonNotes = null, string lot = null, string serialNumber = null, string userName = null, int? orderId = null);

        Task UpdateItemStockAsync(int compartmentId, int itemId, double stock, int? reasonId = null, string reasonNotes = null, string lot = null, string serialNumber = null, string userName = null, int? orderId = null);

        #endregion
    }
}
