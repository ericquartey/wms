using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Events;

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

        private readonly IOperatorHubClient operatorHubClient;

        private WMS.Data.WebAPI.Contracts.MissionOperation currentMissionOperation;

        #endregion

        #region Constructors

        public BayManager(
            IEventAggregator eventAggregator,
            IOperatorHubClient operatorHubClient,
            IMachineBaysWebService machineBaysWebService,
            IMachineIdentityWebService machineIdentityWebService,
            IMachineMissionOperationsWebService missionOperationsAutomationWebService,
            WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationsDataService)
        {
            this.missionOperationsDataService = missionOperationsDataService ?? throw new ArgumentNullException(nameof(missionOperationsDataService));
            this.missionOperationsAutomationService = missionOperationsAutomationWebService ?? throw new ArgumentNullException(nameof(missionOperationsAutomationWebService));
            this.operatorHubClient = operatorHubClient ?? throw new ArgumentNullException(nameof(operatorHubClient));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));

            this.operatorHubClient.AssignedMissionOperationChanged += async (sender, e) => await this.OnAssignedMissionOperationChangedAsync(sender, e);

            var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();

            eventAggregator
                .GetEvent<PubSubEvent<BayChainPositionChangedEventArgs>>()
                .Subscribe(
                    this.OnBayChainPositionChanged,
                    ThreadOption.UIThread,
                    false);
        }

        #endregion

        #region Events

        public event EventHandler NewMissionOperationAvailable;

        #endregion

        #region Properties

        public double ChainPosition { get; private set; }

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

        public int PendingMissionOperationsCount { get; private set; }

        #endregion

        #region Methods

        public async Task<bool> AbortCurrentMissionOperationAsync()
        {
            try
            {
                await this.missionOperationsAutomationService.AbortAsync(this.CurrentMissionOperation.Id);
            }
            catch
            {
                this.CurrentMissionOperation = null;
                return false;
            }
            return true;
        }

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

            return await this.machineBaysWebService.GetByNumberAsync(bayNumber);
        }

        public async Task InitializeAsync()
        {
            this.Identity = await this.IdentityService.GetAsync();
        }

        private async Task OnAssignedMissionOperationChangedAsync(object sender, AssignedMissionOperationChangedEventArgs e)
        {
            var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();

            if (e.BayNumber == bayNumber)
            {
                this.PendingMissionOperationsCount = e.PendingMissionOperationsCount;

                await this.RetrieveMissionOperation(e.MissionOperationId);
            }
        }

        private void OnBayChainPositionChanged(BayChainPositionChangedEventArgs e)
        {
            this.ChainPosition = e.Position;
        }

        private async Task RetrieveMissionOperation(int? missionOperationId)
        {
            if (missionOperationId.HasValue)
            {
                if (missionOperationId.Value != this.CurrentMissionOperation?.Id)
                {
                    try
                    {
                        this.CurrentMissionOperation =
                            await this.missionOperationsDataService.GetByIdAsync(missionOperationId.Value);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        // TODO notify error
                    }
                }
                else
                {
                    this.CurrentMissionOperation = null;
                }
            }
        }

        #endregion
    }
}
