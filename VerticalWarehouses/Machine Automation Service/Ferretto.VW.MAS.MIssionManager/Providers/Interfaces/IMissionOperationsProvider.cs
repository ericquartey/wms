using System.Threading.Tasks;

namespace Ferretto.VW.MAS.MissionManager
{
    public interface IMissionOperationsProvider
    {
        #region Methods

        Task AbortAsync(int id);

        Task CompleteAsync(int id, double quantity);

        #endregion
    }
}
