using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Logging;
using Prism.Events;
using MissionStatus = Ferretto.VW.CommonUtils.Messages.Enumerations.MissionStatus;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed class MissionSchedulingProvider : IMissionSchedulingProvider
    {
        #region Fields

        private readonly CommandEvent commandEvent;
        private readonly IBaysDataProvider bayProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<MissionSchedulingProvider> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly IMissionsDataService missionsDataService;

        private readonly IMoveLoadingUnitProvider moveLoadingUnitProvider;

        #endregion

        #region Constructors

        public MissionSchedulingProvider(
            IBaysDataProvider bayProvider,
            IMissionsDataProvider missionsDataProvider,
            IMissionsDataService missionsDataService,
            IMoveLoadingUnitProvider moveLoadingUnitProvider,
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingService> logger)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.commandEvent = eventAggregator.GetEvent<CommandEvent>();
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.bayProvider = bayProvider ?? throw new ArgumentNullException(nameof(bayProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
            this.moveLoadingUnitProvider = moveLoadingUnitProvider ?? throw new ArgumentNullException(nameof(moveLoadingUnitProvider));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Methods

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber)
        {
            throw new NotImplementedException();
        }

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, int wmsMissionId, int wmsMissionPriority)
        {
            this.missionsDataProvider.CreateBayMission(loadingUnitId, targetBayNumber, wmsMissionId, wmsMissionPriority);

            this.logger.LogDebug($"New mission for loading unit {loadingUnitId} to bay {targetBayNumber}.");

            return Task.CompletedTask;
        }

        public void QueueCellMission(int loadingUnitId, int targetCellId)
        {
            throw new NotImplementedException();
        }

        public void QueueLoadingUnitCompactingMission()
        {
            throw new NotImplementedException();
        }

        public async Task ScheduleMissionsAsync(BayNumber bayNumber)
        {
            var activeMissions = this.missionsDataProvider.GetAllActiveMissionsByBay(bayNumber);

            // first try to continue executing mission
            var executingMission = activeMissions.SingleOrDefault(x => x.Status == MissionStatus.Executing);
            if (!(executingMission is null)
                && executingMission.WmsId.HasValue
                )
            {
                var currentWmsMission = await this.missionsDataService.GetByIdAsync(executingMission.WmsId.Value);
                var newOperations = currentWmsMission.Operations
                    .Where(o => o.Status == WMS.Data.WebAPI.Contracts.MissionOperationStatus.New);
                if (newOperations.Any())
                {
                    // there are more operations for the same wms mission
                    var newOperation = newOperations.OrderBy(o => o.Priority).First();

                    this.bayProvider.AssignWmsMission(bayNumber, currentWmsMission.Id, newOperation.Id);

                    this.NotifyAssignedMissionOperationChanged(bayNumber, currentWmsMission.Id, newOperation.Id, activeMissions.Count());
                }
                else
                {
                    // wms mission is finished
                    this.logger.LogInformation($"Bay {bayNumber}: mission {executingMission.WmsId.Value} completed.");

                    this.bayProvider.ClearMission(bayNumber);

                    var loadingUnitSource = this.bayProvider.GetLoadingUnitLocationByLoadingUnit(executingMission.LoadingUnitId);

                    // check if there are other missions for this LU in this bay
                    var nextMission = activeMissions.FirstOrDefault(x => x.WmsId.HasValue
                        && x.LoadingUnitId == executingMission.LoadingUnitId
                        && x.WmsId.HasValue
                        && x.WmsId != executingMission.WmsId);
                    if (nextMission != null)
                    {
                        // close current mission
                        this.moveLoadingUnitProvider.StopMove(executingMission.FsmId, bayNumber, bayNumber, MessageActor.MissionManager);

                        // activate new mission
                        this.moveLoadingUnitProvider.ActivateMove(nextMission.FsmId, loadingUnitSource, bayNumber, bayNumber, MessageActor.MissionManager);
                    }
                    // else are there other missions for this LU and another bay?
                    //{
                    // update WmsId in the current machine mission and move to another bay
                    //}
                    else
                    {
                        // send back the LU
                        this.moveLoadingUnitProvider.ResumeMoveLoadUnit(executingMission.FsmId, loadingUnitSource, LoadingUnitLocation.Cell, bayNumber, null, MessageActor.MissionManager);
                    }
                }
            }
        }

        private void NotifyAssignedMissionOperationChanged(
                    BayNumber bayNumber,
                    int? missionId,
                    int? missionOperationId,
                    int pendingMissionsCount)
        {
            var data = new AssignedMissionOperationChangedMessageData
            {
                BayNumber = bayNumber,
                MissionId = missionId,
                MissionOperationId = missionOperationId,
                PendingMissionsCount = pendingMissionsCount,
            };

            var notificationMessage = new NotificationMessage(
                data,
                $"Mission operation assigned to bay {bayNumber} has changed.",
                MessageActor.WebApi,
                MessageActor.MachineManager,
                MessageType.AssignedMissionOperationChanged,
                bayNumber);

            this.eventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(notificationMessage);
        }

        #endregion
    }
}
