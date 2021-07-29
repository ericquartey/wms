using System;
using Ferretto.VW.Common.Hubs;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public interface IInstallationHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<BayChainPositionChangedEventArgs> BayChainPositionChanged;

        event EventHandler<BayLightChangedEventArgs> BayLightChanged;

        event EventHandler<ElevatorPositionChangedEventArgs> ElevatorPositionChanged;

        event EventHandler<EndMissionRobotChangedEventArgs> EndMissionRobotChanged;

        event EventHandler<MachineModeChangedEventArgs> MachineModeChanged;

        event EventHandler<MachinePowerChangedEventArgs> MachinePowerChanged;

        event EventHandler<MessageNotifiedEventArgs> MessageReceived;

        event EventHandler<ReadyWarehouseRobotChangedEventArgs> ReadyWarehouseRobotChanged;

        event EventHandler<SystemTimeChangedEventArgs> SystemTimeChanged;

        #endregion
    }
}
