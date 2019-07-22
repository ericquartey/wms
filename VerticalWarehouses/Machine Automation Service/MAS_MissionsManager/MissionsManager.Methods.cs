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
    public partial class MissionsManager
    {
        #region Methods

        private void ChooseAndExecuteMission()
        {
            var connectedIdleBays = this.baysManager.Bays.Where(b =>
                b.IsConnected
                &&
                b.Status == BayStatus.Idle);

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
                    System.Diagnostics.Debug.Assert(
                        bay.CurrentMissionOperation == null,
                        "Current mission operation should be null because the bay is idle.");

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

        private async Task GetAllPendingMissions()
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
                            m.Status != MissionStatus.Completed)
                        .ToArray();
                }
            }
            catch (Exception ex)
            {
                // do nothing
                this.Logger.LogWarning(ex, "Unable to load missions from WMS service.");
            }
        }

        private void NotifyMissionExecution(MAS_Utils.Utilities.Bay bay)
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

        private async Task SetupBays()
        {
            this.baysManager.Bays = new List<MAS_Utils.Utilities.Bay>();

            var ip1 = await this.networkConfiguration.PPC1MasterIPAddress;
            var ip2 = await this.networkConfiguration.PPC2SlaveIPAddress;
            var ip3 = await this.networkConfiguration.PPC3SlaveIPAddress;
            var ipAddresses = new string[] { ip1.ToString(), ip2.ToString(), ip3.ToString() };
            var bayTypes = new int[] { await this.generalInfoConfiguration.Bay1Type, await this.generalInfoConfiguration.Bay2Type, await this.generalInfoConfiguration.Bay3Type };
            var baysQuantity = await this.generalInfoConfiguration.BaysQuantity;

            for (var i = 0; i < baysQuantity; i++)
            {
                this.baysManager.Bays.Add(new MAS_Utils.Utilities.Bay
                {
                    Id = i == 0 ? 2 : 3,
                    IsConnected = false,
                    Status = BayStatus.Unavailable,
                    IpAddress = ipAddresses[i],
                    Type = (BayType)bayTypes[i]
                });
            }
        }

        #endregion
    }
}
