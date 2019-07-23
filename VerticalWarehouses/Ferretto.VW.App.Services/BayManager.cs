using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public class BayManager : IBayManager
    {
        #region Fields

        private readonly IMissionOperationsService missionOperationsAutomationService;

        private readonly IMissionOperationsDataService missionOperationsDataService;

        private readonly IMissionsDataService missionsDataService;

        private readonly IOperatorHubClient operatorHubClient;

        #endregion

        #region Constructors

        public BayManager(
            IOperatorHubClient operatorHubClient,
            IMissionOperationsDataService missionOperationsDataService,
            IMissionOperationsService missionOperationsAutomationService,
            IMissionsDataService missionsDataService)
        {
            if (operatorHubClient == null)
            {
                throw new ArgumentNullException(nameof(operatorHubClient));
            }

            if (missionOperationsDataService == null)
            {
                throw new ArgumentNullException(nameof(missionOperationsDataService));
            }

            if (missionOperationsAutomationService == null)
            {
                throw new ArgumentNullException(nameof(missionOperationsAutomationService));
            }

            if (missionsDataService == null)
            {
                throw new ArgumentNullException(nameof(missionsDataService));
            }

            this.missionOperationsDataService = missionOperationsDataService;
            this.missionOperationsAutomationService = missionOperationsAutomationService;
            this.missionsDataService = missionsDataService;
            this.operatorHubClient = operatorHubClient;

            this.operatorHubClient.ConnectionStatusChanged += async (sender, e) => await this.OnConnectionStatusChangedAsync(sender, e);
            this.operatorHubClient.BayStatusChanged += async (sender, e) => await this.OnBayStatusChangedAsync(sender, e);
            this.operatorHubClient.MissionOperationAvailable += this.OnMissionOperationAvailable;
        }

        #endregion

        #region Events

        public event System.EventHandler NewMissionOperationAvailable;

        #endregion

        #region Properties

        public int BayId { get; private set; }

        public MissionInfo CurrentMission { get; private set; }

        public MissionOperationInfo CurrentMissionOperation { get; private set; }

        public int PendingMissionsCount { get; private set; }

        #endregion

        #region Methods

        public void CompleteCurrentMission()
        {
            // TODO Implement mission completion logic
        }

        public async Task CompleteCurrentMissionOperationAsync(double quantity)
        {
            await this.missionOperationsAutomationService.CompleteAsync(this.CurrentMissionOperation.Id, quantity);

            this.CurrentMissionOperation = null;
        }

        private async Task OnBayStatusChangedAsync(object sender, BayStatusChangedEventArgs e)
        {
            if (this.operatorHubClient.IsConnected == true)
            {
                this.BayId = e.BayId;// TODO the bay ID should come from configuration.
                this.PendingMissionsCount = e.PendingMissionsCount;

                await this.operatorHubClient.RetrieveCurrentMissionOperationAsync();
            }
        }

        private async Task OnConnectionStatusChangedAsync(object sender, MAS.AutomationService.Contracts.ConnectionStatusChangedEventArgs e)
        {
            if (e.IsConnected == true)
            {
                await this.operatorHubClient.RetrieveCurrentMissionOperationAsync();
            }
        }

        private void OnMissionOperationAvailable(object sender, MissionOperationAvailableEventArgs e)
        {
            if (this.CurrentMissionOperation == null)
            // no ongoing operations are present
            {
                this.CurrentMissionOperation = e.MissionOperation;

                this.NewMissionOperationAvailable?.Invoke(this, null);
            }
        }

        #endregion
    }
}
