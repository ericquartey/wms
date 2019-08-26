using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;

namespace Ferretto.VW.App.Services
{
    internal class BayManager : IBayManager
    {
        #region Fields

        private readonly IMachineIdentityService identityService;

        private readonly IMachineBaysService machineBaysService;

        private readonly IMachineMissionOperationsService missionOperationsAutomationService;

        private readonly WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationsDataService;

        private readonly WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService;

        private readonly IOperatorHubClient operatorHubClient;

        private Bay bay;

        private WMS.Data.WebAPI.Contracts.MissionOperation currentMissionOperation;

        #endregion

        #region Constructors

        public BayManager(
            IOperatorHubClient operatorHubClient,
            IMachineBaysService machineBaysService,
            IMachineIdentityService identityService,
            IMachineMissionOperationsService missionOperationsAutomationService,
            WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationsDataService,
            WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService)
        {
            if (operatorHubClient is null)
            {
                throw new ArgumentNullException(nameof(operatorHubClient));
            }

            if (machineBaysService is null)
            {
                throw new ArgumentNullException(nameof(machineBaysService));
            }

            if (identityService is null)
            {
                throw new ArgumentNullException(nameof(identityService));
            }

            if (missionOperationsDataService is null)
            {
                throw new ArgumentNullException(nameof(missionOperationsDataService));
            }

            if (missionOperationsAutomationService is null)
            {
                throw new ArgumentNullException(nameof(missionOperationsAutomationService));
            }

            if (missionsDataService is null)
            {
                throw new ArgumentNullException(nameof(missionsDataService));
            }

            this.missionOperationsDataService = missionOperationsDataService;
            this.missionOperationsAutomationService = missionOperationsAutomationService;
            this.missionsDataService = missionsDataService;
            this.operatorHubClient = operatorHubClient;
            this.machineBaysService = machineBaysService;
            this.identityService = identityService;

            this.operatorHubClient.BayStatusChanged += async (sender, e) => await this.OnBayStatusChangedAsync(sender, e);
            this.operatorHubClient.MissionOperationAvailable += async (sender, e) => await this.OnMissionOperationAvailableAsync(sender, e);
        }

        #endregion

        #region Events

        public event EventHandler NewMissionOperationAvailable;

        #endregion

        #region Properties

        public Bay Bay => this.bay;

        public WMS.Data.WebAPI.Contracts.MissionInfo CurrentMission { get; private set; }

        public WMS.Data.WebAPI.Contracts.MissionOperation CurrentMissionOperation
        {
            get => this.currentMissionOperation;
            private set
            {
                if (value is null)
                {
                    this.currentMissionOperation = null;
                }
                else if (this.currentMissionOperation is null)
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

            var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
            this.bay = await this.machineBaysService.GetByNumberAsync(bayNumber);
        }

        private async Task OnBayStatusChangedAsync(object sender, BayStatusChangedEventArgs e)
        {
            if (this.Bay?.Number == e.Index)
            {
                this.PendingMissionsCount = e.PendingMissionsCount;
                await this.RetrieveMissionOperation(e.CurrentMissionOperationId);
            }
        }

        private async Task OnMissionOperationAvailableAsync(object sender, MissionOperationAvailableEventArgs e)
        {
            if (this.Bay?.Number == e.BayNumber)
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
            catch (WMS.Data.WebAPI.Contracts.SwaggerException)
            {
                // TODO notify error
            }
        }

        #endregion
    }
}
