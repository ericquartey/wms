namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
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
