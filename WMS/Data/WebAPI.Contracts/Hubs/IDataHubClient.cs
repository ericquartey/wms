using System;
using System.Threading.Tasks;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public interface IDataHubClient
    {
        #region Events

        event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        event EventHandler<EntityChangedEventArgs> EntityChanged;

        event EventHandler<MachineStatusUpdatedEventArgs> MachineStatusUpdated;

        #endregion

        #region Properties

        int MaxReconnectTimeoutMilliseconds { get; set; }

        bool IsConnected { get; }

        #endregion

        #region Methods

        Task ConnectAsync();

        Task DisconnectAsync();

        #endregion
    }
}
