using System;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
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

        public event EventHandler<ErrorStatusChangedEventArgs> ErrorStatusChanged;

        public event EventHandler<MissionOperationAvailableEventArgs> MissionOperationAvailable;

        #endregion

        #region Methods

        protected override void RegisterEvents(HubConnection connection)
        {
            connection.On<INewMissionOperationAvailable>(
                nameof(AutomationService.Hubs.IOperatorHub.NewMissionOperationAvailable), this.OnMissionOperationAvailable);

            connection.On<IBayOperationalStatusChangedMessageData>(
                nameof(AutomationService.Hubs.IOperatorHub.BayStatusChanged), this.OnBayStatusChanged);

            connection.On<int>(
                nameof(AutomationService.Hubs.IOperatorHub.ErrorStatusChanged), this.OnErrorStatusChanged);
        }

        private void OnBayStatusChanged(IBayOperationalStatusChangedMessageData e)
        {
            this.BayStatusChanged?.Invoke(
                this,
                new BayStatusChangedEventArgs(
                    e.Index,
                    e.BayStatus,
                    e.PendingMissionsCount,
                    e.CurrentMissionOperationId));
        }

        private void OnErrorStatusChanged(int code)
        {
            this.ErrorStatusChanged?.Invoke(this, new ErrorStatusChangedEventArgs(code));
        }

        private void OnMissionOperationAvailable(INewMissionOperationAvailable e)
        {
            this.MissionOperationAvailable?.Invoke(
                this,
                new MissionOperationAvailableEventArgs(
                    e.BayId,
                    e.MissionId,
                    e.MissionOperationId,
                    e.PendingMissionsCount));
        }

        #endregion
    }
}
