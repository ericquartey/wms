namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class BayChainPositionChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public BayChainPositionChangedEventArgs(double position, BayNumber bayNumber)
        {
            this.Position = position;
            this.BayNumber = bayNumber;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; }

        public double Position { get; }

        #endregion
    }
}
