using System;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public interface IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        #endregion

        #region Properties

        int MaxReconnectTimeoutMilliseconds { get; set; }

        #endregion

        #region Methods

        Task ConnectAsync();

        Task DisconnectAsync();

        #endregion
    }
}
