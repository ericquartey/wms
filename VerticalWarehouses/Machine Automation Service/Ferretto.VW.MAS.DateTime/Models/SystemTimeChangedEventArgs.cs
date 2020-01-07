using System;

namespace Ferretto.VW.MAS.TimeManagement.Models
{
    public class SystemTimeChangedEventArgs : EventArgs
    {
        #region Constructors

        public SystemTimeChangedEventArgs(DateTime dateTime)
        {
            this.DateTime = dateTime;
        }

        #endregion

        #region Properties

        public DateTime DateTime { get; }

        #endregion
    }
}
