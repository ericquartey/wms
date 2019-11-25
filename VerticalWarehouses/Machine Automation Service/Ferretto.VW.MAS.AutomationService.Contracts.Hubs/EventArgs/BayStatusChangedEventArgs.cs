namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class BayStatusChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public BayStatusChangedEventArgs(BayNumber bayNumber, BayStatus bayStatus)
        {
            this.BayNumber = bayNumber;
            this.BayStatus = bayStatus;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; }

        public BayStatus BayStatus { get; }

        #endregion
    }
}
