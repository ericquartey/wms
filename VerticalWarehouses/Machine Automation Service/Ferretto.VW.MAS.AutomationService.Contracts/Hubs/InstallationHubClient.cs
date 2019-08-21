using System;
using Ferretto.VW.CommonUtils.Messages;
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
            connection.On<NotificationMessageUI<SensorsChangedMessageData>>(
              nameof(IInstallationHub.SensorsChangedNotify), this.OnSensorsChangedNotify);

            connection.On<NotificationMessageUI<CalibrateAxisMessageData>>(
                "CalibrateAxisNotify", this.OnCalibrateAxisNotify);

            connection.On<NotificationMessageUI<SwitchAxisMessageData>>(
                "SwitchAxisNotify", this.OnSwitchAxisNotify);

            connection.On<NotificationMessageUI<ShutterPositioningMessageData>>(
                "ShutterPositioningNotify", this.OnShutterPositioningNotify);

            connection.On<NotificationMessageUI<ShutterTestStatusChangedMessageData>>(
                "ShutterControlNotify", this.OnShutterControlNotify);

            connection.On<NotificationMessageUI<PositioningMessageData>>(
                "PositioningNotify", this.OnPositioningNotify);

            connection.On<NotificationMessageUI<CurrentPositionMessageData>>(
                "CurrentPositionNotify", this.OnCurrentPositionNotify);

            connection.On<NotificationMessageUI<HomingMessageData>>(
                "HomingNotify", this.OnHomingNotify);

            connection.On<NotificationMessageUI<InverterExceptionMessageData>>(
                "ExceptionNotify", this.OnExceptionNotify);

            connection.On<NotificationMessageUI<ResolutionCalibrationMessageData>>(
                "ResolutionCalibrationNotify", this.OnResolutionCalibrationNotify);

            connection.On<NotificationMessageUI<ResetSecurityMessageData>>(
                "ResetSecurityNotify", this.OnResetSecurityNotify);

            connection.On<NotificationMessageUI<InverterStopMessageData>>(
                "InverterStopNotify", this.OnInverterStopNotify);

            connection.On<NotificationMessageUI<PowerEnableMessageData>>(
                "PowerEnableNotify", this.OnPowerEnableNotify);

            connection.On<NotificationMessageUI<InverterStatusWordMessageData>>(
                "InverterStatusWordNotify", this.OnInverterStatusWordNotify);

            connection.On<NotificationMessageUI<MachineStatusActiveMessageData>>(
                "MachineStatusActiveNotify", this.OnMachineStatusActiveNotify);

            connection.On<NotificationMessageUI<MachineStateActiveMessageData>>(
                "MachineStateActiveNotify", this.OnMachineStateActiveNotify);
        }

        /// <summary>
        /// Handler for the CalibrateAxis event.
        /// </summary>
        /// <param name="message"></param>
        private void OnCalibrateAxisNotify(NotificationMessageUI<CalibrateAxisMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        /// <summary>
        /// Handler for Current Positioning event.
        /// </summary>
        /// <param name="message"></param>
        private void OnCurrentPositionNotify(NotificationMessageUI<CurrentPositionMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnExceptionNotify(NotificationMessageUI<InverterExceptionMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnHomingNotify(NotificationMessageUI<HomingMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        /// <summary>
        /// Handler for the InverterStatusWord event.
        /// </summary>
        /// <param name="message"></param>
        private void OnInverterStatusWordNotify(NotificationMessageUI<InverterStatusWordMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        /// <summary>
        /// Handler for the InverterStop event.
        /// </summary>
        /// <param name="message"></param>
        private void OnInverterStopNotify(NotificationMessageUI<InverterStopMessageData> message)
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

        /// <summary>
        /// Handler for Positioning event.
        /// </summary>
        /// <param name="message"></param>
        private void OnPositioningNotify(NotificationMessageUI<PositioningMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        /// <summary>
        /// Handler for the PowerEnable event.
        /// </summary>
        /// <param name="message"></param>
        private void OnPowerEnableNotify(NotificationMessageUI<PowerEnableMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        /// <summary>
        /// Handler for the ResetSecurity event.
        /// </summary>
        /// <param name="message"></param>
        private void OnResetSecurityNotify(NotificationMessageUI<ResetSecurityMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        /// <summary>
        /// Handler for Positioning event.
        /// </summary>
        /// <param name="message"></param>
        private void OnResolutionCalibrationNotify(NotificationMessageUI<ResolutionCalibrationMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        /// <summary>
        /// Handler for the SensorsChanged event.
        /// </summary>
        /// <param name="message"></param>
        private void OnSensorsChangedNotify(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        /// <summary>
        /// Handler for the ShutterControl event.
        /// </summary>
        /// <param name="message"></param>
        private void OnShutterControlNotify(NotificationMessageUI<ShutterTestStatusChangedMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        /// <summary>
        /// Handler for the ShutterPositioning event.
        /// </summary>
        /// <param name="message"></param>
        private void OnShutterPositioningNotify(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        /// <summary>
        /// Handler for the SwitchAxis event.
        /// </summary>
        /// <param name="message"></param>
        private void OnSwitchAxisNotify(NotificationMessageUI<SwitchAxisMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        #endregion
    }
}
