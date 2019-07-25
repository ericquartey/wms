using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionsManager
{
    public partial class MissionsManagerService
    {
        #region Methods

        private async Task ExecuteNextMissionAsync()
        {
            var connectedIdleBays = this.bays.Where(b => b.Status == BayStatus.Idle);

            System.Diagnostics.Debug.Assert(
                connectedIdleBays.Any(b => b.CurrentMissionOperation != null) == false,
                "Current mission operation on all bays should be null because the bays are idle.");

            this.Logger.LogDebug($"There are {connectedIdleBays.Count()} active bays.");

            foreach (var bay in connectedIdleBays)
            {
                await this.ExecuteNextMissionAsync(bay);
            }
        }

        private async Task ExecuteNextMissionAsync(Bay bay)
        {
            this.Logger.LogDebug($"Bay #{bay.Id}: there are {bay.PendingMissions.Count()} pending missions.");

            if (bay.CurrentMission == null
                &&
                bay.PendingMissions.Any())
            {
                bay.CurrentMission = bay.PendingMissions
                    .OrderBy(m => m.Priority)
                    .First();

                /* TO RESTORE
                System.Diagnostics.Debug.Assert(
                   bay.CurrentMission.Status == MissionStatus.New,
                   "All the pending missions should be in the new state.");
                   */

                // remove the new current mission from the queue
                bay.PendingMissions = bay.PendingMissions.Where(m => m != bay.CurrentMission);

                this.Logger.LogDebug($"Bay #{bay.Id}: new mission id='{bay.CurrentMission.Id}' assigned.");
            }

            if (bay.CurrentMission != null)
            {
                try
                {
                    var mission = await this.missionsDataService.GetByIdAsync(bay.CurrentMission.Id);

                    bay.CurrentMissionOperation = mission.Operations
                        .OrderBy(o => o.Priority)
                        .FirstOrDefault(o => o.Status == MissionOperationStatus.New);

                    System.Diagnostics.Debug.Assert(
                        bay.CurrentMissionOperation != null,
                        "There should be at least one new operation in the mission: that's why we are here for.");

                    bay.Status = BayStatus.Busy;

                    this.Logger.LogDebug($"Bay #{bay.Id}: busy executing mission operation id='{bay.CurrentMissionOperation.Id}'.");

                    this.NotifyMissionExecution(bay);
                }
                catch (SwaggerException ex)
                {
                    this.Logger.LogError(ex, $"Unable to load details for mission id='{bay.CurrentMission.Id}' from WMS.");
                }
            }
        }

        private void NotifyMissionExecution(Bay bay)
        {
            var data = new NewMissionOperationAvailable
            {
                Mission = bay.CurrentMission,
                MissionOperation = bay.CurrentMissionOperation,
                PendingMissionsCount = bay.PendingMissions.Count(),
                BayConnectionId = bay.ConnectionId
            };

            var notificationMessage = new NotificationMessage(
                data,
                "New Mission Operation Available",
                MessageActor.AutomationService,
                MessageActor.MissionsManager,
                MessageType.NewMissionOperationAvailable,
                MessageStatus.NoStatus);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(notificationMessage);
        }

        private async Task RefreshPendingMissionsQueue()
        {
            try
            {
                var machineId = 1; // TODO get machine Id from DataLayer
                var missions = await this.machinesDataService.GetMissionsByIdAsync(machineId);

                foreach (var bay in this.bays)
                {
                    bay.PendingMissions = missions
                        .Where(m =>
                            m.BayId == bay.Id
                            &&
                            m.Status != MissionStatus.Completed
                            &&
                            (bay.CurrentMission == null || bay.CurrentMission.Id != m.Id))
                        .OrderBy(m => m.Priority)
                        .ToArray();
                }
            }
            catch (Exception ex)
            {
                // do nothing
                this.Logger.LogWarning(ex, "Unable to load missions from WMS service.");
            }
        }

        private async Task ScheduleMissionsOnBaysAsync()
        {
            var waitHandles = new WaitHandle[]
            {
                this.bayStatusChangedEvent,
                this.newMissionArrivedResetEvent,
                this.StoppingToken.WaitHandle
            };

            do
            {
                var hasActiveBaysAndPendingMissions =
                    this.bays.Any(bay =>
                        bay.Status == BayStatus.Idle
                        &&
                        bay.PendingMissions.Any());

                if (hasActiveBaysAndPendingMissions)
                {
                    this.Logger.LogDebug("There are active bays with pending missions.");
                    await this.ExecuteNextMissionAsync();
                }
                else
                {
                    this.Logger.LogDebug("No active bays nor pending missions available.");
                    WaitHandle.WaitAny(waitHandles);
                }
            }
            while (!this.StoppingToken.IsCancellationRequested);
        }

        #endregion
    }
}
