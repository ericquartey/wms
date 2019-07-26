using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IBayManager
    {
        #region Events

        event System.EventHandler NewMissionOperationAvailable;

        #endregion

        #region Properties

        int BayId { get; }

        MissionInfo CurrentMission { get; }

        MissionOperation CurrentMissionOperation { get; }

        MachineIdentity Identity { get; }

        int PendingMissionsCount { get; }

        #endregion

        #region Methods

        void CompleteCurrentMission();

        Task CompleteCurrentMissionOperationAsync(double quantity);

        Task InitializeAsync();

        #endregion
    }
}
