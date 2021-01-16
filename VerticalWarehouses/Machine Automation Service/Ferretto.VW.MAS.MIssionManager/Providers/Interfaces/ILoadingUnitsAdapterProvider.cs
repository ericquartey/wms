using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.MAS.MissionManager
{
    public interface ILoadingUnitsAdapterProvider
    {
        #region Methods

        Task<LoadingUnitSchedulerRequest> WithdrawAsync(int id, int bayId);

        #endregion
    }
}
