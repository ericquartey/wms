using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public class MissionOperationStartedEventArgs : System.EventArgs
    {
        #region Constructors

        public MissionOperationStartedEventArgs(
            Mission mission,
            MissionOperation missionOperation,
            int pendingMissionsCount)
        {
            this.mission = mission;
            this.missionOperation = missionOperation;
            this.pendingMissionsCount = pendingMissionsCount;
        }

        #endregion

        #region Properties

        public Mission Mission { get; }

        public MissionOperation MissionOperation { get; }

        public int PendingMissionsCount { get; }

        #endregion
    }
}
