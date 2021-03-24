using System.Threading.Tasks;

namespace Ferretto.VW.App.Modules.Operator
{
    public interface IWmsDataProvider
    {
        #region Properties

        bool IsEnabled { get; }

        #endregion

        #region Methods

        Task<string> GetItemImagePathAsync(int itemId);

        Task PickAsync(int itemId, double requestedQuantity, int? reasonId = null, string reasonNotes = null, string userName = null);

        Task PutAsync(int itemId, double requestedQuantity, int? reasonId = null, string reasonNotes = null, string userName = null);

        void Start();

        #endregion
    }
}
