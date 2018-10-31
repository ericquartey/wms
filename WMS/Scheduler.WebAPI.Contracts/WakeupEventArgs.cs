using System;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public class WakeUpEventArgs : EventArgs
    {
        #region Properties

        public string Message { get; set; }
        public string User { get; set; }

        #endregion Properties
    }
}
