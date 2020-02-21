using System;
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

        public event EventHandler<AssignedMissionChangedEventArgs> AssignedMissionChanged;

        public event EventHandler<AssignedMissionOperationChangedEventArgs> AssignedMissionOperationChanged;

        public event EventHandler<BayStatusChangedEventArgs> BayStatusChanged;

        public event EventHandler<ErrorStatusChangedEventArgs> ErrorStatusChanged;

        #endregion

        #region Methods

        protected override void RegisterEvents(HubConnection connection)
        {
            connection.On<BayNumber, int?>(
                nameof(AutomationService.Hubs.IOperatorHub.AssignedMissionChanged),
                this.OnAssignedMissionChanged);

            connection.On<BayNumber>(
                nameof(AutomationService.Hubs.IOperatorHub.AssignedMissionOperationChanged),
                this.OnAssignedMissionOperationChanged);

            connection.On<BayNumber, BayStatus>(
                nameof(AutomationService.Hubs.IOperatorHub.BayStatusChanged),
                this.OnBayStatusChanged);

            connection.On<int>(
                nameof(AutomationService.Hubs.IOperatorHub.ErrorStatusChanged),
                this.OnErrorStatusChanged);
        }

        private void OnAssignedMissionChanged(BayNumber bayNumber, int? missionId)
        {
            this.AssignedMissionChanged?.Invoke(
                this,
                new AssignedMissionChangedEventArgs(
                    bayNumber,
                    missionId));
        }

        private void OnAssignedMissionOperationChanged(BayNumber bayNumber)
        {
            this.AssignedMissionOperationChanged?.Invoke(
                this,
                new AssignedMissionOperationChangedEventArgs(
                    bayNumber));
        }

        private void OnBayStatusChanged(BayNumber bayNumber, BayStatus bayStatus)
        {
            this.BayStatusChanged?.Invoke(
                this,
                new BayStatusChangedEventArgs(bayNumber, bayStatus));
        }

        private void OnErrorStatusChanged(int code)
        {
            this.ErrorStatusChanged?.Invoke(this, new ErrorStatusChangedEventArgs(code));
        }

        #endregion
    }
}
