using System;
using System.Threading.Tasks;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public interface IDataHubClient
    {
        #region Events

        event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        event EventHandler<EntityChangedEventArgs> EntityChanged;

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
