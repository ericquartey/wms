using System;
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

        public event EventHandler<BayStatusChangedEventArgs> BayStatusChanged;

        public event EventHandler<MissionOperationStartedEventArgs> MissionOperationStarted;

        #endregion

        #region Methods

        protected override void RegisterEvents(HubConnection connection)
        {
            connection.On<ExecuteMissionMessageData>(
                "MissionOperationStarted", this.OnMissionOperationStarted);

            connection.On<BayConnectedMessageData>(
                "BayStatusChanged", this.OnBayStatusChanged);
        }

        private void OnBayStatusChanged(BayConnectedMessageData message)
        {
            this.BayStatusChanged?.Invoke(
                this,
                new BayStatusChangedEventArgs(
                    message.BayId,
                    message.BayType,
                    message.PendingMissionsCount));
        }

        private void OnMissionOperationStarted(ExecuteMissionMessageData message)
        {
            this.MissionOperationStarted?.Invoke(
                this,
                new MissionOperationStartedEventArgs(
                    message.Mission,
                    message.MissionOperation,
                    message.PendingMissionsCount));
        }

        #endregion
    }
}
