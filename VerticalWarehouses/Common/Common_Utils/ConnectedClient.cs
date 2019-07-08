namespace Ferretto.VW.Common_Utils
{
    public class ConnectedClient
    {
        #region Constructors

        public ConnectedClient(string connectionID)
        {
            this.ConnectionId = connectionID;
        }

        #endregion

        #region Properties

        public string ConnectionId { get; set; }

        #endregion
    }
}
