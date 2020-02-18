using System.Threading.Tasks;

namespace Ferretto.VW.App.Modules.Operator
{
    public interface IWmsDataProvider
    {
        #region Methods

        Task<string> GetItemImagePathAsync(int itemId);

        Task PickAsync(int itemId, double requestedQuantity);

        Task PutAsync(int itemId, double requestedQuantity);

        #endregion
    }
}
