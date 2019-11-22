using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IBayManager
    {
        #region Events

        event System.EventHandler NewMissionOperationAvailable;

        #endregion

        #region Properties

        double ChainPosition { get; }

        WMS.Data.WebAPI.Contracts.MissionInfo CurrentMission { get; }

        WMS.Data.WebAPI.Contracts.MissionOperation CurrentMissionOperation { get; }

        MachineIdentity Identity { get; }

        int PendingMissionsCount { get; }

        #endregion

        #region Methods

        Task<bool> AbortCurrentMissionOperationAsync();

        void CompleteCurrentMission();

        Task CompleteCurrentMissionOperationAsync(double quantity);

        Task<Bay> GetBayAsync();

        Task InitializeAsync();

        #endregion
    }
}
