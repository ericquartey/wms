namespace Ferretto.VW.App.Installation.ServiceUtilities
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
