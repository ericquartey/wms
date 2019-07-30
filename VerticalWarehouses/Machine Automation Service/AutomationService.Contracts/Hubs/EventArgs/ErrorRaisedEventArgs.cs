namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs
{
    public class ErrorRaisedEventArgs : System.EventArgs
    {
        #region Constructors

        public ErrorRaisedEventArgs(int code)
        {
            this.Code = code;
        }

        #endregion

        #region Properties

        public int Code { get; }

        #endregion
    }
}
