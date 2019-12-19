using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.MissionManager
{
    public interface IMissionOperationsProvider
    {
        #region Methods

        Task AbortAsync(int id);

        Task CompleteAsync(int wmsId, double quantity);

        Task<MissionOperation> GetByIdAsync(int wmsId);

        int GetCountByBay(BayNumber bayNumber);

        Task PartiallyCompleteAsync(int wmsId, double quantity);

        #endregion
    }
}
