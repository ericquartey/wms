using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
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

        public event EventHandler<MissionOperationAvailableEventArgs> MissionOperationAvailable;

        #endregion

        #region Methods

        protected override void RegisterEvents(HubConnection connection)
        {
            connection.On<MissionOperationInfo>(
                nameof(IOperatorHub.NewMissionOperationAvailable), this.OnMissionOperationAvailable);

            connection.On<BayOperationalStatusChangedMessageData>(
                nameof(IOperatorHub.BayStatusChanged), this.OnBayStatusChanged);
        }

        private void OnBayStatusChanged(BayOperationalStatusChangedMessageData message)
        {
            this.BayStatusChanged?.Invoke(
                this,
                new BayStatusChangedEventArgs(
                    message.BayId,
                    message.BayType,
                    message.BayStatus,
                    message.PendingMissionsCount,
                    message.CurrentMissionOperation));
        }

        private void OnMissionOperationAvailable(MissionOperationInfo missionOperation)
        {
            this.MissionOperationAvailable?.Invoke(
                this,
                new MissionOperationAvailableEventArgs(missionOperation));
        }

        #endregion
    }
}
