using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public class MissionOperationStartedEventArgs : System.EventArgs
    {
        #region Constructors

        public MissionOperationStartedEventArgs(
            MissionInfo mission,
            MissionOperationInfo missionOperation,
            int pendingMissionsCount)
        {
            this.Mission = mission;
            this.MissionOperation = missionOperation;
            this.PendingMissionsCount = pendingMissionsCount;
        }

        #endregion

        #region Properties

        public MissionInfo Mission { get; }

        public MissionOperationInfo MissionOperation { get; }

        public int PendingMissionsCount { get; }

        #endregion
    }
}
