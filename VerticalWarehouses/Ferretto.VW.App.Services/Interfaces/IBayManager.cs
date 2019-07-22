using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IBayManager
    {
        #region Properties

        int BayId { get; set; }

        Mission CurrentMission { get; set; }

        MissionOperation CurrentMissionOperation { get; }

        int PendingMissionsCount { get; set; }

        #endregion

        #region Methods

        void CompleteCurrentMission();

        #endregion
    }
}
