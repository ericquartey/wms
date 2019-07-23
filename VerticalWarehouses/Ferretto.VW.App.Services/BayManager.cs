using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public class BayManager : IBayManager
    {
        #region Fields

        private readonly IMissionOperationsDataService missionOperationsDataService;

        private readonly IOperatorHubClient operatorHubClient;

        #endregion

        #region Constructors

        public BayManager(
            IOperatorHubClient operatorHubClient,
            IMissionOperationsDataService missionOperationsDataService)
        {
            if (operatorHubClient == null)
            {
                throw new ArgumentNullException(nameof(operatorHubClient));
            }

            if (missionOperationsDataService == null)
            {
                throw new ArgumentNullException(nameof(missionOperationsDataService));
            }

            this.operatorHubClient = operatorHubClient;
            this.missionOperationsDataService = missionOperationsDataService;

            this.operatorHubClient.ConnectionStatusChanged += async (sender, e) => await this.OnConnectionStatusChangedAsync(sender, e);
            this.operatorHubClient.BayStatusChanged += async (sender, e) => await this.OnBayStatusChangedAsync(sender, e);
            this.operatorHubClient.MissionOperationStarted += this.OnMissionOperationStarted;
        }

        #endregion

        #region Properties

        public int BayId { get; private set; }

        public MissionInfo CurrentMission { get; set; }

        public MissionOperationInfo CurrentMissionOperation { get; set; }

        public int PendingMissionsCount { get; private set; }

        #endregion

        #region Methods

        public void CompleteCurrentMission()
        {
            // TODO Implement mission completion logic
        }

        private async Task OnBayStatusChangedAsync(object sender, BayStatusChangedEventArgs e)
        {
            if (this.operatorHubClient.IsConnected == true)
            {
                this.BayId = e.BayId;// TODO the bay ID should come from configuration.
                await this.operatorHubClient.RetrieveCurrentMissionOperationAsync();
                this.PendingMissionsCount = e.PendingMissionsCount;
            }
        }

        private async Task OnConnectionStatusChangedAsync(object sender, MAS.AutomationService.Contracts.ConnectionStatusChangedEventArgs e)
        {
            if (e.IsConnected == true)
            {
                await this.operatorHubClient.RetrieveCurrentMissionOperationAsync();
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
