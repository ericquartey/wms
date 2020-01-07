using System;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IWmsSettingsProvider
    {
        #region Properties

        bool IsWmsTimeSyncEnabled { get; set; }

        DateTimeOffset LastWmsSyncTime { get; set; }

        int TimeSyncIntervalMilliseconds { get; }

        #endregion
    }
}
