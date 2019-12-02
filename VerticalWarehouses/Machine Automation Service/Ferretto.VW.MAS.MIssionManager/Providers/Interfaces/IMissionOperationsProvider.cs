using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.MissionManager
{
    public interface IMissionOperationsProvider
    {
        #region Methods

        Task AbortAsync(int id);

        Task CompleteAsync(int wmsId, double quantity);

        Task<MissionOperation> GetByIdAsync(int wmsId);

        #endregion
    }
}
