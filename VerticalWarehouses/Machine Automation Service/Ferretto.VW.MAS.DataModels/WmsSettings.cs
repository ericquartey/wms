using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class WmsSettings : DataModel
    {
        #region Properties

        public bool IsWmsTimeSyncEnabled { get; set; }

        public DateTimeOffset LastWmsTimeSync { get; set; }

        public int TimeSyncIntervalMilliseconds { get; set; }

        #endregion
    }
}
