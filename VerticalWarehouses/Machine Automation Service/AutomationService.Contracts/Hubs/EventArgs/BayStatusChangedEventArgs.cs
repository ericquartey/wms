namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public class BayStatusChangedEventArgs : System.EventArgs
    {
        #region Constructors

        // TODO change type from int to BayType
        public BayStatusChangedEventArgs(int bayId, int bayType, int pendingMissionsCount)
        {
            this.BayId = bayId;
            this.BayType = bayType;
            this.PendingMissionsCount = pendingMissionsCount;
        }

        #endregion

        #region Properties

        public int BayId { get; set; }

        // TODO change type from int to BayType
        public int BayType { get; set; }

        public int PendingMissionsCount { get; set; }

        #endregion
    }
}
