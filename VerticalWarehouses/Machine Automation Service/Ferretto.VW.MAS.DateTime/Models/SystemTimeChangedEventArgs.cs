using System;

namespace Ferretto.VW.MAS.TimeManagement.Models
{
    public class SystemTimeChangedEventArgs : EventArgs
    {
        #region Constructors

        public SystemTimeChangedEventArgs(DateTimeOffset dateTime)
        {
            this.DateTime = dateTime;
        }

        #endregion

        #region Properties

        public DateTimeOffset DateTime { get; }

        #endregion
    }
}
