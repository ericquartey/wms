namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class AssignedMissionOperationChangedEventArgs : System.EventArgs, IBayEventArgs
    {
        #region Constructors

        public AssignedMissionOperationChangedEventArgs(
            BayNumber bayNumber,
            int? missionId,
            int? missionOperationId,
            int pendingMissionOperationsCount)
        {
            this.BayNumber = bayNumber;
            this.MissionId = missionId;
            this.MissionOperationId = missionOperationId;
            this.PendingMissionOperationsCount = pendingMissionOperationsCount;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; }

        public int? MissionId { get; }

        public int? MissionOperationId { get; }

        public int PendingMissionOperationsCount { get; }

        #endregion
    }
}
