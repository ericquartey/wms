using System;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IWmsSettingsProvider
    {
        #region Properties

        int ConnectionTimeout { get; set; }

        bool IsConnected { get; set; }

        bool IsEnabled { get; set; }

        bool IsTimeSyncEnabled { get; set; }

        DateTimeOffset LastWmsSyncTime { get; set; }

        Uri ServiceUrl { get; set; }

        bool SocketLinkIsEnabled { get; set; }

        int SocketLinkPolling { get; set; }

        int SocketLinkPort { get; set; }

        int SocketLinkTimeout { get; set; }

        int TimeSyncIntervalMilliseconds { get; }

        #endregion

        #region Methods

        WmsSettings GetAll();

        void TimeSyncIntervalMillisecondsUpdate(int seconds);

        #endregion
    }
}
