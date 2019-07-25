using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
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
             "SensorsChangedNotify", this.OnSensorsChangedNotify);

            connection.On<NotificationMessageUI<CalibrateAxisMessageData>>(
                "CalibrateAxisNotify", this.OnCalibrateAxisNotify);

            connection.On<NotificationMessageUI<SwitchAxisMessageData>>(
                "SwitchAxisNotify", this.OnSwitchAxisNotify);

            connection.On<NotificationMessageUI<ShutterPositioningMessageData>>(
                "ShutterPositioningNotify", this.OnShutterPositioningNotify);

            connection.On<NotificationMessageUI<ShutterControlMessageData>>(
                "ShutterControlNotify", this.OnShutterControlNotify);

            connection.On<NotificationMessageUI<PositioningMessageData>>(
                "PositioningNotify", this.OnPositioningNotify);

            connection.On<NotificationMessageUI<HomingMessageData>>(
                "HomingNotify", this.OnHomingNotify);

            connection.On<NotificationMessageUI<InverterExceptionMessageData>>(
                "ExceptionNotify", this.OnExceptionNotify);

            connection.On<NotificationMessageUI<ResolutionCalibrationMessageData>>(
                "ResolutionCalibrationNotify", this.OnResolutionCalibrationNotify);
        }

        /// <summary>
        /// Handler for the CalibrateAxis event.
        /// </summary>
        /// <param name="message"></param>
        private void OnCalibrateAxisNotify(NotificationMessageUI<CalibrateAxisMessageData> message)
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
        /// Handler for Positioning event.
        /// </summary>
        /// <param name="message"></param>
        private void OnPositioningNotify(NotificationMessageUI<PositioningMessageData> message)
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
        private void OnShutterControlNotify(NotificationMessageUI<ShutterControlMessageData> message)
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
