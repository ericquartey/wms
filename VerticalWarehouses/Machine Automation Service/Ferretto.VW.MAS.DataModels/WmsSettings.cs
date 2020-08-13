using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class WmsSettings : DataModel
    {
        #region Properties

        public bool IsConnected { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsTimeSyncEnabled { get; set; }

        public DateTimeOffset LastWmsTimeSync { get; set; }

        public Uri ServiceUrl { get; set; }

        public int TimeSyncIntervalMilliseconds { get; set; }

        #endregion
    }
}
