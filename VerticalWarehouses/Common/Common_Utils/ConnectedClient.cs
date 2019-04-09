namespace Ferretto.VW.Common_Utils
{
    public class ConnectedClient
    {
        #region Constructors

        public ConnectedClient(string connectionID)
        {
            this.ConnectionID = connectionID;
        }

        #endregion

        #region Properties

        public string ConnectionID { get; set; }

        #endregion
    }
}
