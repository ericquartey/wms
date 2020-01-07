using System;

namespace Ferretto.VW.MAS.TimeManagement.Models
{
    internal class SyncStateChangeRequestEventArgs : EventArgs
    {
        #region Constructors

        public SyncStateChangeRequestEventArgs(bool enable)
        {
            this.Enable = enable;
        }

        #endregion

        #region Properties

        public bool Enable { get; }

        #endregion
    }
}
