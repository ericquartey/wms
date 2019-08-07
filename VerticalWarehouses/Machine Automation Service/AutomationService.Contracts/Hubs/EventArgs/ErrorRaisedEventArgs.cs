namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs
{
    public class ErrorStatusChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ErrorStatusChangedEventArgs(int? code)
        {
            this.Code = code;
        }

        #endregion

        #region Properties

        public int? Code { get; }

        #endregion
    }
}
