namespace Ferretto.VW.AutomationService.Contracts
{
    public sealed class ConnectionStatusChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ConnectionStatusChangedEventArgs(bool isConnected)
        {
            this.IsConnected = isConnected;
        }

        #endregion

        #region Properties

        public bool IsConnected { get; private set; }

        #endregion
    }
}
