using System;
using System.Threading.Tasks;

namespace Ferretto.VW.AutomationService.Contracts
{
    public interface IMachineHubClient
    {
        #region Events

        event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        event EventHandler<ElevatorPositionChangedEventArgs> ElevatorPositionChanged;

        event EventHandler<LoadingUnitChangedEventArgs> LoadingUnitInBayChanged;

        event EventHandler<LoadingUnitChangedEventArgs> LoadingUnitInElevatorChanged;

        event EventHandler<MachineStatusReceivedEventArgs> MachineStatusReceived;

        event EventHandler<ModeChangedEventArgs> ModeChanged;

        event EventHandler<UserChangedEventArgs> UserChanged;

        #endregion

        #region Properties

        int MachineId { get; }

        int MaxReconnectTimeoutMilliseconds { get; set; }

        #endregion

        #region Methods

        Task ConnectAsync();

        Task DisconnectAsync();

        Task RequestCurrentStateAsync();

        #endregion
    }
}
