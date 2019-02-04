using System;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public class WakeUpEventArgs : EventArgs
    {
        #region Constructors

        public WakeUpEventArgs(string user, string message)
        {
            this.User = user;
            this.Message = message;
        }

        #endregion

        #region Properties

        public string Message { get; private set; }

        public string User { get; private set; }

        #endregion
    }
}
