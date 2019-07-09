using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public class OperatorHubClient : AutoReconnectHubClient, IOperatorHubClient
    {
        #region Constructors

        public OperatorHubClient(Uri uri)
            : base(uri)
        {
        }

        #endregion

        #region Events

        public event EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion

        #region Methods

        protected override void RegisterEvents(HubConnection connection)
        {
            connection.On<NotificationMessageUI<ExecuteMissionMessageData>>(
               "ProvideMissionsToBay", this.OnProvidedMissionsToBay);

            connection.On<NotificationMessageUI<BayConnectedMessageData>>(
                "OnConnectionEstablished", this.OnConnectionEstablished);

            base.RegisterEvents(connection);
        }

        private void OnConnectionEstablished(NotificationMessageUI<BayConnectedMessageData> message)
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
