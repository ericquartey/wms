using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MissionManager
{
    public interface IMissionOperationsProvider
    {
        #region Methods

        Task AbortAsync(int id);

        Task CompleteAsync(int wmsId, double quantity);

        #endregion
    }
}
