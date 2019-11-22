namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class AssignedMissionOperationChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public AssignedMissionOperationChangedEventArgs(
            BayNumber bayNumber,
            int missionId,
            int missionOperationId,
            int pendingMissionOperationsCount)
        {
            this.BayNumber = bayNumber;
            this.MissionId = missionId;
            this.MissionOperationId = missionOperationId;
            this.PendingMissionsOperationsCount = pendingMissionOperationsCount;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; }

        public int MissionId { get; }

        public int MissionOperationId { get; }

        public int PendingMissionsOperationsCount { get; }

        #endregion
    }
}
