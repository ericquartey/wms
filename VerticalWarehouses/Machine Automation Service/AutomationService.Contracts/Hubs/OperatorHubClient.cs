using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.MissionsManager;
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

        public event EventHandler<BayStatusChangedEventArgs> BayStatusChanged;

        public event EventHandler<MissionOperationStartedEventArgs> MissionOperationStarted;

        #endregion

        #region Methods

        protected override void RegisterEvents(HubConnection connection)
        {
            connection.On<NotificationMessageUI<ExecuteMissionMessageData>>(
                "MissionOperationStarted", this.OnMissionOperationStarted);

            connection.On<NotificationMessageUI<BayConnectedMessageData>>(
                "BayStatusChanged", this.OnBayStatusChanged);
        }

        private void OnBayStatusChanged(NotificationMessageUI<BayConnectedMessageData> message)
        {
            this.BayStatusChanged?.Invoke(
                this,
                new BayStatusChangedEventArgs(
                    message.Data.BayId,
                    message.Data.BayType,
                    message.Data.PendingMissionsCount));
        }

        private void OnMissionOperationStarted(NotificationMessageUI<ExecuteMissionMessageData> message)
        {
            this.MissionOperationStarted?.Invoke(
                this,
                new MissionOperationStartedEventArgs(
                    message.Data.Mission,
                    message.Data.MissionOperation,
                    message.Data.PendingMissionsCount));
        }

        #endregion
    }
}
