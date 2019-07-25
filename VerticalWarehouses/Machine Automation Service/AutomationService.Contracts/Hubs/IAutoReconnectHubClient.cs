﻿using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public interface IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        #endregion

        #region Properties

        bool IsConnected { get; }

        int MaxReconnectTimeoutMilliseconds { get; set; }

        #endregion

        #region Methods

        Task ConnectAsync();

        Task DisconnectAsync();

        #endregion
    }
}
