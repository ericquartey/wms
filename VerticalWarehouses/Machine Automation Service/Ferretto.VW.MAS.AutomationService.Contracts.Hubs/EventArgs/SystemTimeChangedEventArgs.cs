using System;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
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
