namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class AssignedMissionChangedEventArgs : System.EventArgs, IBayEventArgs
    {
        #region Constructors

        public AssignedMissionChangedEventArgs(
            BayNumber bayNumber,
            int? missionId)
        {
            this.BayNumber = bayNumber;
            this.MissionId = missionId;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; }

        public int? MissionId { get; }

        #endregion
    }
}
