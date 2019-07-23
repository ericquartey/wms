using System.Threading.Tasks;
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

        MissionOperationInfo CurrentMissionOperation { get; }

        int PendingMissionsCount { get; }

        #endregion

        #region Methods

        void CompleteCurrentMission();

        Task CompleteCurrentMissionOperationAsync(double quantity);

        #endregion
    }
}
