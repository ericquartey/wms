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

        private readonly WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationsDataService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

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
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
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

        public async Task CompleteCurrentAsync(double quantity)
        {
            await this.missionOperationsWebService.CompleteAsync(
                this.CurrentMissionOperation.Id,
                quantity);
        }

        public async Task PartiallyCompleteCurrentAsync(double quantity)
        {
            await this.missionOperationsWebService.PartiallyCompleteAsync(this.CurrentMissionOperation.Id, quantity);
        }

        private async Task OnAssignedMissionOperationChangedAsync(object sender, AssignedMissionOperationChangedEventArgs e)
        {
            this.PendingMissionOperationsCount = e.PendingMissionOperationsCount;

            var currentMissionOperation = this.CurrentMissionOperation;

            if (e.MissionId.HasValue && e.MissionOperationId.HasValue)
            {
                try
                {
                    this.CurrentMission = await this.missionsDataService.GetByIdAsync(e.MissionId.Value);
                    this.CurrentMissionOperation =
                        await this.missionOperationsDataService.GetByIdAsync(e.MissionOperationId.Value);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                this.CurrentMission = null;
                this.CurrentMissionOperation = null;
            }

            if (currentMissionOperation?.Id != this.CurrentMissionOperation?.Id
                ||
                currentMissionOperation?.RequestedQuantity != this.CurrentMissionOperation?.RequestedQuantity)
            {
                this.eventAggregator
                       .GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>()
                       .Publish(e);
            }
        }

        #endregion
    }
}
