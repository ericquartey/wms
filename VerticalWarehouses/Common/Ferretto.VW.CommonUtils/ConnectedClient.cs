namespace Ferretto.VW.CommonUtils
{
    public class ConnectedClient
    {
        #region Constructors

        public ConnectedClient(string connectionId)
        {
            this.ConnectionId = connectionId;
        }

        #endregion

        #region Properties

        public string ConnectionId { get; set; }

        #endregion
    }
}
