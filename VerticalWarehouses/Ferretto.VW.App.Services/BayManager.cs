using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
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

        private MissionOperationInfo currentMissionOperation;

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

            this.operatorHubClient.BayStatusChanged += this.OnBayStatusChanged;
            this.operatorHubClient.MissionOperationAvailable += this.OnMissionOperationAvailable;
        }

        #endregion

        #region Events

        public event System.EventHandler NewMissionOperationAvailable;

        #endregion

        #region Properties

        public int BayId { get; private set; }

        public MissionInfo CurrentMission { get; private set; }

        public MissionOperationInfo CurrentMissionOperation
        {
            get => this.currentMissionOperation;
            private set
            {
                if (value == null)
                {
                    this.currentMissionOperation = value;
                }

                if (this.currentMissionOperation == null)
                {
                    this.CurrentMissionOperation = value;
                    this.NewMissionOperationAvailable?.Invoke(this, null);
                }
            }
        }

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

        private void OnBayStatusChanged(object sender, BayStatusChangedEventArgs e)
        {
            this.BayId = e.BayId;// TODO the bay ID should come from configuration.
            this.PendingMissionsCount = e.PendingMissionsCount;
            this.CurrentMissionOperation = e.CurrentMissionOperation;
        }

        private void OnMissionOperationAvailable(object sender, MissionOperationAvailableEventArgs e)
        {
            this.CurrentMissionOperation = e.MissionOperation;
        }

        #endregion
    }
}
