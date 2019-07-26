namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs
{
    public class ReceivedMessageEventArgs : System.EventArgs
    {
        #region Constructors

        public ReceivedMessageEventArgs(string message)
        {
            this.Message = message;
        }

        #endregion

        #region Properties

        public string Message { get; }

        #endregion
    }
}
