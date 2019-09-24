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

        Bay Bay { get; }

        WMS.Data.WebAPI.Contracts.MissionInfo CurrentMission { get; }

        WMS.Data.WebAPI.Contracts.MissionOperation CurrentMissionOperation { get; }

        MachineIdentity Identity { get; }

        int PendingMissionsCount { get; }

        #endregion

        #region Methods

        void CompleteCurrentMission();

        Task CompleteCurrentMissionOperationAsync(double quantity);

        Task InitializeAsync();

        Task<Bay> UpdateHeightAsync(BayNumber bayNumber, int position, decimal height);

        #endregion
    }
}
