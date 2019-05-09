namespace Ferretto.VW.AutomationService.Contracts
{
    public class ElevatorPositionChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ElevatorPositionChangedEventArgs(int machineId, int position)
        {
            this.MachineId = machineId;
            this.Position = position;
        }

        #endregion

        #region Properties

        public int MachineId { get; private set; }

        public decimal Position { get; }

        #endregion
    }
}
