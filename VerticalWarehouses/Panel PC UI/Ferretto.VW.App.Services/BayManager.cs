using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.App.Services
{
    internal class BayManager : IBayManager
    {
        #region Fields

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineMissionOperationsWebService missionOperationsAutomationService;

        private readonly WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationsDataService;

        private readonly WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService;

        private readonly IOperatorHubClient operatorHubClient;

        private WMS.Data.WebAPI.Contracts.MissionOperation currentMissionOperation;

        #endregion

        #region Constructors

        public BayManager(
            IOperatorHubClient operatorHubClient,
            IMachineBaysWebService machineBaysWebService,
            IMachineIdentityWebService machineIdentityWebService,
            IMachineMissionOperationsWebService missionOperationsAutomationWebService,
            WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationsDataService,
            WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService)
        {
            if (operatorHubClient is null)
            {
                throw new ArgumentNullException(nameof(operatorHubClient));
            }

            if (machineBaysWebService is null)
            {
                throw new ArgumentNullException(nameof(machineBaysWebService));
            }

            if (machineIdentityWebService is null)
            {
                throw new ArgumentNullException(nameof(machineIdentityWebService));
            }

            if (missionOperationsDataService is null)
            {
                throw new ArgumentNullException(nameof(missionOperationsDataService));
            }

            if (missionOperationsAutomationWebService is null)
            {
                throw new ArgumentNullException(nameof(missionOperationsAutomationWebService));
            }

            if (missionsDataService is null)
            {
                throw new ArgumentNullException(nameof(missionsDataService));
            }

            this.missionOperationsDataService = missionOperationsDataService;
            this.missionOperationsAutomationService = missionOperationsAutomationWebService;
            this.missionsDataService = missionsDataService;
            this.operatorHubClient = operatorHubClient;
            this.machineBaysWebService = machineBaysWebService;
            this.machineIdentityWebService = machineIdentityWebService;

            this.operatorHubClient.BayStatusChanged += async (sender, e) => await this.OnBayStatusChangedAsync(sender, e);
            this.operatorHubClient.MissionOperationAvailable += async (sender, e) => await this.OnMissionOperationAvailableAsync(sender, e);
        }

        #endregion

        #region Events

        public event EventHandler NewMissionOperationAvailable;

        #endregion

        #region Properties

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

        public IMachineIdentityWebService IdentityService => this.machineIdentityWebService;

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

        public async Task<Bay> GetBayAsync()
        {
            var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();

            return await this.machineBaysWebService.GetByNumberAsync((BayNumber)bayNumber);
        }

        public async Task InitializeAsync()
        {
            this.Identity = await this.IdentityService.GetAsync();
        }

        private async Task OnBayStatusChangedAsync(object sender, BayStatusChangedEventArgs e)
        {
            try
            {
                var bay = await this.GetBayAsync();
                if (bay != null && bay.Number == (BayNumber)e.Index)
                {
                    this.PendingMissionsCount = e.PendingMissionsCount;
                    await this.RetrieveMissionOperation(e.CurrentMissionOperationId);
                }
            }
            catch
            {
                // do nothing
            }
        }

        private async Task OnMissionOperationAvailableAsync(object sender, MissionOperationAvailableEventArgs e)
        {
            //TODO Review Implementation avoid using numbers to identify bays
            var bayNumber = BayNumber.None;
            switch (e.BayNumber)
            {
                case 1:
                    bayNumber = BayNumber.BayOne;
                    break;

                case 2:
                    bayNumber = BayNumber.BayTwo;
                    break;

                case 3:
                    bayNumber = BayNumber.BayThree;
                    break;
            }

            if ((BayNumber)ConfigurationManager.AppSettings.GetBayNumber() == bayNumber)
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
