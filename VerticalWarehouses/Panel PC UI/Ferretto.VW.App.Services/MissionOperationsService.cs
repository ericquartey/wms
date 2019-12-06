using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class MissionOperationsService : IMissionOperationsService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineMissionOperationsWebService missionOperationsAutomationService;

        private readonly WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationsDataService;

        private readonly WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService;

        private readonly IOperatorHubClient operatorHubClient;

        #endregion

        #region Constructors

        public MissionOperationsService(
            IEventAggregator eventAggregator,
            IMachineMissionOperationsWebService missionOperationsWebService,
            WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationsDataService,
            WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService,
            IOperatorHubClient operatorHubClient)
        {
            this.eventAggregator = eventAggregator;
            this.missionOperationsDataService = missionOperationsDataService ?? throw new ArgumentNullException(nameof(missionOperationsDataService));
            this.missionOperationsAutomationService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));

            this.operatorHubClient = operatorHubClient ?? throw new ArgumentNullException(nameof(operatorHubClient));
            this.operatorHubClient.AssignedMissionOperationChanged += async (sender, e) => await this.OnAssignedMissionOperationChangedAsync(sender, e);
        }

        #endregion

        #region Properties

        public WMS.Data.WebAPI.Contracts.MissionInfo CurrentMission { get; private set; }

        public WMS.Data.WebAPI.Contracts.MissionOperation CurrentMissionOperation { get; private set; }

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

        public async Task CompleteCurrentMissionOperationAsync(double quantity)
        {
            await this.missionOperationsAutomationService.CompleteAsync(this.CurrentMissionOperation.Id, quantity);

            this.CurrentMissionOperation = null;
        }

        private async Task OnAssignedMissionOperationChangedAsync(object sender, AssignedMissionOperationChangedEventArgs e)
        {
            this.PendingMissionOperationsCount = e.PendingMissionOperationsCount;
            if (e.MissionId.HasValue && e.MissionOperationId.HasValue)
            {
                await this.RetrieveMissionOperation(e.MissionOperationId.Value, e.MissionId.Value, e);
            }
            else
            {
                this.CurrentMission = null;
                this.CurrentMissionOperation = null;

                this.eventAggregator
                  .GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>()
                  .Publish(e);
            }
        }

        private async Task RetrieveMissionOperation(int missionOperationId, int missionId, AssignedMissionOperationChangedEventArgs e)
        {
            if (missionOperationId != this.CurrentMissionOperation?.Id)
            {
                try
                {
                    this.CurrentMissionOperation =
                        await this.missionOperationsDataService.GetByIdAsync(missionOperationId);
                    this.CurrentMission = await this.missionsDataService.GetByIdAsync(missionId);

                    this.eventAggregator
                        .GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>()
                        .Publish(e);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        #endregion
    }
}
