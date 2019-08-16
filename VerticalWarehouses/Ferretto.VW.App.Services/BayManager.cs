using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public class BayManager : IBayManager
    {
        #region Fields

        private readonly IMachineIdentityService identityService;

        private readonly IMachineMissionOperationsService missionOperationsAutomationService;

        private readonly IMissionOperationsDataService missionOperationsDataService;

        private readonly IMissionsDataService missionsDataService;

        private readonly IOperatorHubClient operatorHubClient;

        private readonly IStatusMessageService statusMessageService;

        private MissionOperation currentMissionOperation;

        #endregion

        #region Constructors

        public BayManager(
            IOperatorHubClient operatorHubClient,
            IMachineIdentityService identityService,
            IMissionOperationsDataService missionOperationsDataService,
            IMachineMissionOperationsService missionOperationsAutomationService,
            IMissionsDataService missionsDataService,
            IStatusMessageService statusMessageService)
        {
            if (operatorHubClient == null)
            {
                throw new ArgumentNullException(nameof(operatorHubClient));
            }

            if (identityService == null)
            {
                throw new ArgumentNullException(nameof(identityService));
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

            if (statusMessageService == null)
            {
                throw new ArgumentNullException(nameof(statusMessageService));
            }

            this.missionOperationsDataService = missionOperationsDataService;
            this.missionOperationsAutomationService = missionOperationsAutomationService;
            this.missionsDataService = missionsDataService;
            this.statusMessageService = statusMessageService;
            this.operatorHubClient = operatorHubClient;
            this.identityService = identityService;
            this.operatorHubClient.BayStatusChanged += async (sender, e) => await this.OnBayStatusChangedAsync(sender, e);
            this.operatorHubClient.MissionOperationAvailable += async (sender, e) => await this.OnMissionOperationAvailableAsync(sender, e);
        }

        #endregion

        #region Events

        public event EventHandler NewMissionOperationAvailable;

        #endregion

        #region Properties

        public int BayNumber => ConfigurationManager.AppSettings.GetBayNumber();

        public MissionInfo CurrentMission { get; private set; }

        public MissionOperation CurrentMissionOperation
        {
            get => this.currentMissionOperation;
            private set
            {
                if (value == null)
                {
                    this.currentMissionOperation = null;
                }
                else if (this.currentMissionOperation == null)
                {
                    this.currentMissionOperation = value;
                    this.NewMissionOperationAvailable?.Invoke(this, null);
                }
            }
        }

        public MachineIdentity Identity { get; private set; }

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

        public async Task InitializeAsync()
        {
            this.Identity = await this.identityService.GetAsync();
        }

        private async Task OnBayStatusChangedAsync(object sender, BayStatusChangedEventArgs e)
        {
            if (this.BayNumber == e.BayId)
            {
                this.PendingMissionsCount = e.PendingMissionsCount;
                await this.RetrieveMissionOperation(e.CurrentMissionOperationId);
            }
        }

        private async Task OnMissionOperationAvailableAsync(object sender, MissionOperationAvailableEventArgs e)
        {
            if (this.BayNumber == e.BayId)
            {
                this.PendingMissionsCount = e.PendingMissionsCount;
                await this.RetrieveMissionOperation(e.MissionOperationId);
            }
        }

        private async Task RetrieveMissionOperation(int? missionOperationId)
        {
            try
            {
                if (missionOperationId.HasValue
                    &&
                    missionOperationId.Value != this.CurrentMissionOperation?.Id)
                {
                    this.CurrentMissionOperation =
                        await this.missionOperationsDataService.GetByIdAsync(missionOperationId.Value);
                }
            }
            catch (WMS.Data.WebAPI.Contracts.SwaggerException ex)
            {
                this.statusMessageService.Notify(ex);
            }
        }

        #endregion
    }
}
