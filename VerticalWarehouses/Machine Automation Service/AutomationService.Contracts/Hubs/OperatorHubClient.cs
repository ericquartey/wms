using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.WMS.Data.WebAPI.Contracts;
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

        public event EventHandler<MissionOperationAvailableEventArgs> MissionOperationAvailable;

        #endregion

        #region Methods

        public async Task RetrieveCurrentMissionOperationAsync()
        {
            await this.SendAsync("RetrieveCurrentMissionOperation");
        }

        protected override void RegisterEvents(HubConnection connection)
        {
            connection.On<MissionOperationInfo>(
                nameof(Hubs.IOperatorHub.NewMissionOperationAvailable), this.OnMissionOperationAvailable);

            connection.On<BayConnectedMessageData>(
                nameof(Hubs.IOperatorHub.BayStatusChanged), this.OnBayStatusChanged);
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

        private void OnMissionOperationAvailable(MissionOperationInfo missionOperation)
        {
            this.MissionOperationAvailable?.Invoke(
                this,
                new MissionOperationAvailableEventArgs(missionOperation));
        }

        #endregion
    }
}
