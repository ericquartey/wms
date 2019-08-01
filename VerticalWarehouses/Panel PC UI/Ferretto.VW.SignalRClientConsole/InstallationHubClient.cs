using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.SignalRClientConsole
{
    public class InstallationHubClient : IInstallationHubClient
    {
        #region Fields

        private readonly HubConnection hubConnection;

        #endregion

        #region Constructors

        public InstallationHubClient(string url, string installationHubPath)
        {
            this.hubConnection = new HubConnectionBuilder()
              .WithUrl(new Uri(new Uri(url), installationHubPath).AbsoluteUri)
              .Build();

            this.hubConnection.On<NotificationMessageUI<SensorsChangedMessageData>>(
                "SensorsChangedNotify", this.OnSensorsChangedNotify);

            this.hubConnection.On<NotificationMessageUI<CalibrateAxisMessageData>>(
                "CalibrateAxisNotify", this.OnCalibrateAxisNotify);

            this.hubConnection.On<NotificationMessageUI<SwitchAxisMessageData>>(
                "SwitchAxisNotify", this.OnSwitchAxisNotify);

            this.hubConnection.On<NotificationMessageUI<ShutterPositioningMessageData>>(
                "ShutterPositioningNotify", this.OnShutterPositioningNotify);

            this.hubConnection.On<NotificationMessageUI<ShutterControlMessageData>>(
                "ShutterControlNotify", this.OnShutterControlNotify);

            this.hubConnection.On<NotificationMessageUI<PositioningMessageData>>(
                "PositioningNotify", this.OnPositioningNotify);

            this.hubConnection.On<NotificationMessageUI<HomingMessageData>>(
                "HomingNotify", this.OnHomingNotify);

            this.hubConnection.On<NotificationMessageUI<ResolutionCalibrationMessageData>>(
                "ResolutionCalibrationNotify", this.OnResolutionCalibrationNotify);

            // -
            // Add here the registration of handlers related to the notification events
            // -
            this.hubConnection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await this.hubConnection.StartAsync();
            };
        }

        #endregion

        #region Events

        public event EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion

        #region Methods

        public async Task ConnectAsync()
        {
            await this.hubConnection.StartAsync();
        }

        public async Task DisconnectAsync()
        {
            await this.hubConnection.DisposeAsync();
        }

        /// <summary>
        /// Handler for the CalibrateAxis event.
        /// </summary>
        /// <param name="message"></param>
        private void OnCalibrateAxisNotify(NotificationMessageUI<CalibrateAxisMessageData> message)
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
