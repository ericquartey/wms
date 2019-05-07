namespace Ferretto.VW.AutomationService.Contracts
{
    public class ElevatorPositionChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ElevatorPositionChangedEventArgs(int position)
        {
            this.Position = position;
        }

        #endregion

        #region Properties

        public int Position { get; }

        #endregion
    }
}
