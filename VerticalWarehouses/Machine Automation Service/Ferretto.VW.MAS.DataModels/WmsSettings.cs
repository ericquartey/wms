﻿using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class WmsSettings : DataModel
    {
        #region Properties

        public int ConnectionTimeout { get; set; }

        public int DelayTimeout { get; set; }

        public bool AlarmsToWmsOn { get; set; }

        public bool IsConnected { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsTimeSyncEnabled { get; set; }

        public DateTimeOffset LastWmsTimeSync { get; set; }

        public Uri ServiceUrl { get; set; }

        /// <summary>
        /// Enable LF as string terminator in the place of CR
        /// </summary>
        public bool SocketLinkEndOfLine { get; set; }

        public bool SocketLinkIsEnabled { get; set; }

        public int SocketLinkPolling { get; set; }

        public int SocketLinkPort { get; set; }

        public int SocketLinkTimeout { get; set; }

        public int TimeSyncIntervalMilliseconds { get; set; }

        #endregion
    }
}
