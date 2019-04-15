using System;
using System.Threading.Tasks;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    public class InstallationHubClient : IContainerInstallationHubClient
    {
        #region Fields

        private readonly HubConnection hubConnection;

        #endregion

        #region Constructors

        public InstallationHubClient(string url, string sensorStatePath)
        {
            this.hubConnection = new HubConnectionBuilder()
              .WithUrl(new Uri(new Uri(url), sensorStatePath).AbsoluteUri)
              .Build();

            this.hubConnection.On<NotificationMessageUI<SensorsChangedMessageData>>(
                "SensorsChangedNotify", this.OnSensorsChangedNotify);

            this.hubConnection.On<NotificationMessageUI<CalibrateAxisMessageData>>(
                "CalibrateAxisNotify", this.OnCalibrateAxisNotify);

            this.hubConnection.On<NotificationMessageUI<SwitchAxisMessageData>>(
                "SwitchAxisNotify", this.OnSwitchAxisNotify);

            this.hubConnection.On<NotificationMessageUI<UpDownRepetitiveMessageData>>(
                "UpDownRepetitiveNotify", this.OnUpDownRepetitiveNotify);

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

        /// <summary>
        /// Handler for the SensorsChanged event.
        /// </summary>
        /// <param name="message"></param>
        private void OnSensorsChangedNotify(NotificationMessageUI<SensorsChangedMessageData> message)
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

        /// <summary>
        /// Handler for the UpDownRepetitive event.
        /// </summary>
        /// <param name="message"></param>
        private void OnUpDownRepetitiveNotify(NotificationMessageUI<UpDownRepetitiveMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        #endregion
    }
}
