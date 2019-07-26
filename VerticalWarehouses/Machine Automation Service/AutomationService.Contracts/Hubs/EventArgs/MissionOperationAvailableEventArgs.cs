namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class MissionOperationAvailableEventArgs : System.EventArgs
    {
        #region Constructors

        public MissionOperationAvailableEventArgs(
            int bayId,
            int missionId,
            int missionOperationId,
            int pendingMissionsCount)
        {
            this.BayId = bayId;
            this.MissionId = missionId;
            this.MissionOperationId = missionOperationId;
            this.PendingMissionsCount = pendingMissionsCount;
        }

        #endregion

        #region Properties

        public int BayId { get; }

        public int MissionId { get; }

        public int MissionOperationId { get; }

        public int PendingMissionsCount { get; }

        #endregion
    }
}
