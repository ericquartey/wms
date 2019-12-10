namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class ElevatorPositionChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ElevatorPositionChangedEventArgs(double verticalPosition, double horizontalPosition, int? cellId, int? bayPositionId, bool? bayPositionUpper)
        {
            this.VerticalPosition = verticalPosition;
            this.HorizontalPosition = horizontalPosition;
            this.CellId = cellId;
            this.BayPositionId = bayPositionId;
            this.BayPositionUpper = bayPositionUpper;
        }

        #endregion

        #region Properties

        public int? BayPositionId { get; }

        public bool? BayPositionUpper { get; }

        public int? CellId { get; }

        public double HorizontalPosition { get; }

        public double VerticalPosition { get; }

        #endregion
    }
}
