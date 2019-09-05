using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IWmsDataProvider
    {
        #region Methods

        Task<string> GetItemImageCodeAsync(int itemId);

        Task<IEnumerable<TrayControlCompartment>> GetTrayControlCompartmentsAsync(MissionInfo mission);

        Task<bool> PickAsync(int itemId, int areaId, int bayId, int requestedQuantity);

        #endregion
    }
}
