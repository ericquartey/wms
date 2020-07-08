namespace Ferretto.VW.Common.Hubs
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

        public bool IsConnected { get; }

        #endregion
    }
}
