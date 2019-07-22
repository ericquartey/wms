using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    public class BayManager : IBayManager
    {
        #region Fields

        private readonly IOperatorHubClient operatorHubClient;

        #endregion

        #region Constructors

        public BayManager(IOperatorHubClient operatorHubClient)
        {
            if (operatorHubClient == null)
            {
                throw new System.ArgumentNullException(nameof(operatorHubClient));
            }

            this.operatorHubClient = operatorHubClient;

            this.operatorHubClient.ConnectionStatusChanged += this.OnConnectionStatusChanged;
            this.operatorHubClient.BayStatusChanged += this.OnBayStatusChanged;
            this.operatorHubClient.MissionOperationStarted += this.OnMissionOperationStarted;
        }

        #endregion

        #region Properties

        public int BayId { get; set; }

        public Mission CurrentMission { get; set; }

        public MissionOperation CurrentMissionOperation { get; set; }

        public int PendingMissionsCount { get; set; }

        #endregion

        #region Methods

        public void CompleteCurrentMission()
        {
            // TODO Implement mission completion logic
        }

        private void OnBayStatusChanged(object sender, BayStatusChangedEventArgs e)
        {
            this.BayId = e.BayId;
            this.PendingMissionsCount = e.PendingMissionsCount;
        }

        private void OnConnectionStatusChanged(object sender, MAS.AutomationService.Contracts.ConnectionStatusChangedEventArgs e)
        {
            if (e.IsConnected == true)
            {
                // TODO ask for the next mission
            }
        }

        private void OnMissionOperationStarted(object sender, MissionOperationStartedEventArgs e)
        {
            if (e.MissionOperation == null)
            {
                this.CurrentMission = e.Mission;
                this.CurrentMissionOperation = e.MissionOperation;
                this.PendingMissionsCount = e.PendingMissionsCount;
            }
        }

        #endregion
    }
}
