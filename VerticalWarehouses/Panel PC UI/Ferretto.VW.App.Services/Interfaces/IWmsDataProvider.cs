using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IWmsDataProvider
    {
        #region Methods

        Task<string> GetItemImagePathAsync(int itemId);

        Task PickAsync(int itemId, int requestedQuantity);

        #endregion
    }
}
