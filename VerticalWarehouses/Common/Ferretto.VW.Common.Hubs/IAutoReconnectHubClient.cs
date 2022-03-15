using System;
using System.Net;
using System.Threading.Tasks;

namespace Ferretto.VW.Common.Hubs
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

        Task ConnectAsync(bool useMessagePackProtocol = false);

        Task DisconnectAsync();

        Task SetProxy(WebProxy proxy);

        #endregion
    }
}
