namespace Ferretto.VW.App.Services
{
    public class StatusMessageEventArgs : System.EventArgs
    {
        #region Constructors

        public StatusMessageEventArgs(string message, StatusMessageLevel level)
        {
            this.Message = message;
            this.Level = level;
        }

        #endregion

        #region Properties

        public StatusMessageLevel Level { get; }

        public string Message { get; }

        #endregion
    }
}
