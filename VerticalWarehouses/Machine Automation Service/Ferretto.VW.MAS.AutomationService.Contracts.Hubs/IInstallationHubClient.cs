using System;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public interface IInstallationHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler<BayChainPositionChangedEventArgs> BayChainPositionChanged;

        event EventHandler<ElevatorPositionChangedEventArgs> ElevatorPositionChanged;

        event EventHandler<MachineModeChangedEventArgs> MachineModeChanged;

        event EventHandler<MachinePowerChangedEventArgs> MachinePowerChanged;

        event EventHandler<MessageNotifiedEventArgs> MessageReceived;

        event EventHandler<SystemTimeChangedEventArgs> SystemTimeChanged;

        #endregion
    }
}
