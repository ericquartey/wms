namespace Ferretto.WMS.Data.WebAPI.Contracts
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
