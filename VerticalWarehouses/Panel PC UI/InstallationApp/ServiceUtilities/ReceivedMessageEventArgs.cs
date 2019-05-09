namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    public class ReceivedMessageEventArgs
    {
        #region Constructors

        public ReceivedMessageEventArgs(string message)
        {
            this.Message = message;
        }

        #endregion

        #region Properties

        public string Message { get; set; }

        #endregion
    }
}
