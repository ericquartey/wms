using Ferretto.VW.App.Services.Interfaces;

namespace Ferretto.VW.App.Services.Models
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
