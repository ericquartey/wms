namespace Ferretto.VW.MAS.NordDriver
{
    public sealed class ConnectionStatusChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ConnectionStatusChangedEventArgs(bool isConnected, bool isConnectedUdp)
        {
            this.IsConnected = isConnected;
            this.IsConnectedUdp = isConnectedUdp;
        }

        #endregion

        #region Properties

        public bool IsConnected { get; }

        public bool IsConnectedUdp { get; }

        #endregion
    }
}
