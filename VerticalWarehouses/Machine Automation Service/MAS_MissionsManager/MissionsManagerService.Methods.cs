using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionsManager
{
    public partial class MissionsManagerService
    {
        #region Methods

        private void ChooseAndExecuteMission()
        {
            var connectedIdleBays = this.baysManager.Bays.Where(b => b.Status == BayStatus.Idle);

            System.Diagnostics.Debug.Assert(
                connectedIdleBays.Any(b => b.CurrentMissionOperation != null) == false,
                "Current mission operation on all bays should be null because the bays are idle.");

            foreach (var bay in connectedIdleBays)
            {
                if (bay.CurrentMission == null
                    &&
                    bay.PendingMissions.Any())
                {
                    bay.CurrentMission = bay.PendingMissions
                        .OrderBy(m => m.Priority)
                        .First();

                    System.Diagnostics.Debug.Assert(
                       bay.CurrentMission.Status == MissionStatus.New,
                       "All the pending missions should be in the new state.");

                    // remove the new current mission from the queue
                    bay.PendingMissions = bay.PendingMissions.Where(m => m != bay.CurrentMission);
                }

                if (bay.CurrentMission != null)
                {
                    bay.CurrentMissionOperation = bay.CurrentMission.Operations
                        .OrderBy(o => o.Priority)
                        .FirstOrDefault(o => o.Status == MissionOperationStatus.New);

                    System.Diagnostics.Debug.Assert(
                        bay.CurrentMissionOperation != null,
                        "There should be at least one new operation in the mission: that's why we are here for.");

                    bay.Status = BayStatus.Busy;

                    this.NotifyMissionExecution(bay);

                    this.Logger.LogDebug(
                        $"MM MissionManagementCycle: Iteration #{this.logCounterMissionManagement++}: Bay {bay.Id} status set to Busy, chose mission {bay.CurrentMission.Id} / operation {bay.CurrentMissionOperation.Id}");
                }
            }
        }

        private void NotifyMissionExecution(Bay bay)
        {
            var data = new ExecuteMissionMessageData
            {
                Mission = bay.CurrentMission,
                MissionOperation = bay.CurrentMissionOperation,
                PendingMissionsCount = bay.PendingMissions.Count(),
                BayConnectionId = bay.ConnectionId
            };

            var notificationMessage = new NotificationMessage(
                data,
                "Execute Mission",
                MessageActor.AutomationService,
                MessageActor.MissionsManager,
                MessageType.ExecuteMission,
                MessageStatus.NoStatus);

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
        }

        private async Task RefreshPendingMissionsQueue()
        {
            try
            {
                // TODO get machine Id from DataLayer
                var machineId = 1;
                var missions = await this.machinesDataService.GetMissionsByIdAsync(machineId);

                foreach (var bay in this.baysManager.Bays)
                {
                    bay.PendingMissions = missions
                        .Where(m =>
                            m.BayId == bay.Id
                            &&
                            m.Status != MissionStatus.Completed
                            &&
                            bay.CurrentMission == null || bay.CurrentMission.Id != m.Id)
                        .ToArray();

                    // refresh current mission
                    bay.CurrentMission = bay.CurrentMission == null
                        ? null
                        : missions.SingleOrDefault(b => b.Id == bay.CurrentMission.Id);
                }
            }
            catch (Exception ex)
            {
                // do nothing
                this.Logger.LogWarning(ex, "Unable to load missions from WMS service.");
            }
        }

        #endregion
    }
}
