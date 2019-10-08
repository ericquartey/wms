using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class InstallationHubClient : AutoReconnectHubClient, IInstallationHubClient
    {
        #region Constructors

        public InstallationHubClient(string url, string installationHubPath)
            : base(new Uri(new Uri(url), installationHubPath))
        {
        }

        #endregion

        #region Events

        public event EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion

        #region Methods

        protected override void RegisterEvents(HubConnection connection)
        {
            connection.On<NotificationMessageUI<CommonUtils.Messages.Data.SensorsChangedMessageData>>(
                nameof(IInstallationHub.SensorsChanged), this.OnSensorsChanged);

            connection.On<NotificationMessageUI<CalibrateAxisMessageData>>(
                nameof(IInstallationHub.CalibrateAxisNotify), this.OnCalibrateAxisNotify);

            connection.On<NotificationMessageUI<SwitchAxisMessageData>>(
                 nameof(IInstallationHub.SwitchAxisNotify), this.OnSwitchAxisNotify);

            connection.On<NotificationMessageUI<CommonUtils.Messages.Data.ShutterPositioningMessageData>>(
                 nameof(IInstallationHub.ShutterPositioningNotify), this.OnShutterPositioningNotify);

            connection.On<NotificationMessageUI<PositioningMessageData>>(
                 nameof(IInstallationHub.PositioningNotify), this.OnPositioningNotify);

            connection.On<NotificationMessageUI<CurrentPositionMessageData>>(
                 nameof(IInstallationHub.CurrentPositionChanged), this.OnCurrentPositionChanged);

            connection.On<NotificationMessageUI<HomingMessageData>>(
                 nameof(IInstallationHub.HomingProcedureStatusChanged), this.OnHomingProcedureStatusChanged);

            connection.On<NotificationMessageUI<ResolutionCalibrationMessageData>>(
                 nameof(IInstallationHub.ResolutionCalibrationNotify), this.OnResolutionCalibrationNotify);

            connection.On<NotificationMessageUI<InverterStatusWordMessageData>>(
                 nameof(IInstallationHub.InverterStatusWordChanged), this.OnInverterStatusWordChanged);

            connection.On<NotificationMessageUI<MachineStatusActiveMessageData>>(
                 nameof(IInstallationHub.MachineStatusActiveNotify), this.OnMachineStatusActiveNotify);

            connection.On<NotificationMessageUI<MachineStateActiveMessageData>>(
                 nameof(IInstallationHub.MachineStateActiveNotify), this.OnMachineStateActiveNotify);

            connection.On<NotificationMessageUI<PowerEnableMessageData>>(
                 nameof(IInstallationHub.PowerEnableNotify), this.OnPowerEnableNotify);

            connection.On<NotificationMessageUI<ElevatorWeightCheckMessageData>>(
                 nameof(IInstallationHub.ElevatorWeightCheck), this.OnElavtorWeightCheck);

            connection.On<NotificationMessageUI<ChangeRunningStateMessageData>>(
                nameof(IInstallationHub.ChangeRunningState), this.OnChangeRunningState);
        }

        private void OnCalibrateAxisNotify(NotificationMessageUI<CalibrateAxisMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnChangeRunningState(NotificationMessageUI<ChangeRunningStateMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnCurrentPositionChanged(NotificationMessageUI<CurrentPositionMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnElavtorWeightCheck(NotificationMessageUI<ElevatorWeightCheckMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnHomingProcedureStatusChanged(NotificationMessageUI<HomingMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnInverterStatusWordChanged(NotificationMessageUI<InverterStatusWordMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnMachineStateActiveNotify(NotificationMessageUI<MachineStateActiveMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnMachineStatusActiveNotify(NotificationMessageUI<MachineStatusActiveMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnPositioningNotify(NotificationMessageUI<PositioningMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnPowerEnableNotify(NotificationMessageUI<PowerEnableMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnResolutionCalibrationNotify(NotificationMessageUI<ResolutionCalibrationMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnSensorsChanged(NotificationMessageUI<CommonUtils.Messages.Data.SensorsChangedMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnShutterPositioningNotify(NotificationMessageUI<CommonUtils.Messages.Data.ShutterPositioningMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnSwitchAxisNotify(NotificationMessageUI<SwitchAxisMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        #endregion
    }
}
