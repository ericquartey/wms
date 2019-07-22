namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public class BayStatusChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public BayStatusChangedEventArgs(int bayId, BayType bayType, int pendingMissionsCount)
        {
            this.BayId = bayId;
            this.BayType = bayType;
            this.PendingMissionsCount = pendingMissionsCount;
        }

        #endregion

        #region Properties

        public int BayId { get; set; }

        public BayType BayType { get; set; }

        public int PendingMissionsCount { get; set; }

        #endregion
    }
}
