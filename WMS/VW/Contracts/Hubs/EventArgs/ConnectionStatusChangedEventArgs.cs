namespace Ferretto.VW.AutomationService.Contracts
{
    public sealed class ConnectionStatusChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ConnectionStatusChangedEventArgs(int machineId, bool isConnected)
        {
            this.MachineId = machineId;
            this.IsConnected = isConnected;
        }

        #endregion

        #region Properties

        public bool IsConnected { get; private set; }

        public int MachineId { get; private set; }

        #endregion
    }
}
