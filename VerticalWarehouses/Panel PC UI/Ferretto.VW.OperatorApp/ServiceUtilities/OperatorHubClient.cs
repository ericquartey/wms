using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.OperatorApp.ServiceUtilities
{
    internal class OperatorHubClient : IOperatorHubClient
    {
        #region Fields

        private readonly HubConnection hubConnection;

        #endregion

        #region Constructors

        public OperatorHubClient(string url, string installationHubPath)
        {
            this.hubConnection = new HubConnectionBuilder()
              .WithUrl(new Uri(new Uri(url), installationHubPath).AbsoluteUri)
              .Build();

            this.hubConnection.On<NotificationMessageUI<ExecuteMissionMessageData>>(
                "SetBayDrawerOperationToPick", this.OnDrawerOperationPickNotify);

            this.hubConnection.On<NotificationMessageUI<ExecuteMissionMessageData>>(
                "ProvideMissionsToBay", this.OnProvidedMissionsToBay);

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

        private void OnDrawerOperationPickNotify(NotificationMessageUI<ExecuteMissionMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnProvidedMissionsToBay(NotificationMessageUI<ExecuteMissionMessageData> message)
        {
            this.MessageNotified?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        #endregion
    }
}
