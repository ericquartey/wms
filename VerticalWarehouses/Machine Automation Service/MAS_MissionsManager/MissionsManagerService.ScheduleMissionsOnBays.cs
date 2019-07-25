using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionsManager
{
    public partial class MissionsManagerService
    {
        #region Methods

        private async Task ExecuteNextMissionAsync()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var bayProvider = scope.ServiceProvider.GetRequiredService<IBaysProvider>();

                var idleBays = bayProvider
                    .GetAll()
                    .Where(b => b.Status == BayStatus.Idle);

                this.Logger.LogDebug($"There are {idleBays.Count()} idle (useable) bays.");

                try
                {
                    var machineId = 1; // TODO get machine Id from DataLayer
                    var missions = await this.machinesDataService.GetMissionsByIdAsync(machineId);

                    foreach (var bay in idleBays)
                    {
                        var pendingMissionsOnBay = missions
                            .Where(m =>
                                m.BayId.HasValue
                                &&
                                m.BayId.Value == bay.ExternalId
                                &&
                                m.Status != WMS.Data.WebAPI.Contracts.MissionStatus.Completed
                                &&
                                (!bay.CurrentMissionId.HasValue || m.Id != bay.CurrentMissionId.Value))
                            .ToArray();

                        await this.ExecuteNextMissionAsync(bay, pendingMissionsOnBay);
                    }
                }
                catch (Exception ex)
                {
                    // do nothing
                    this.Logger.LogWarning(ex, "Unable to load missions from WMS service.");
                }
            }
        }

        private async Task ExecuteNextMissionAsync(
            Bay bay,
            IEnumerable<WMS.Data.WebAPI.Contracts.MissionInfo> pendingMissions)
        {
            this.Logger.LogDebug($"Bay #{bay.Id}: there are {pendingMissions.Count()} pending missions.");

            if (!bay.CurrentMissionId.HasValue
                &&
                pendingMissions.Any())
            {
                bay.CurrentMissionId = pendingMissions
                    .OrderBy(m => m.Priority)
                    .First()
                    .Id;

                /* TO RESTORE
                System.Diagnostics.Debug.Assert(
                   bay.CurrentMission.Status == MissionStatus.New,
                   "All the pending missions should be in the new state.");
                   */

                this.Logger.LogDebug($"Bay #{bay.Id}: new mission id='{bay.CurrentMissionId}' assigned.");
            }

            if (bay.CurrentMissionId.HasValue)
            {
                try
                {
                    var mission = await this.missionsDataService.GetByIdAsync(bay.CurrentMissionId.Value);

                    var missionOperation = mission.Operations?
                        .OrderBy(o => o.Priority)
                        .FirstOrDefault(o => o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Completed);

                    using (var scope = this.serviceScopeFactory.CreateScope())
                    {
                        var bayProvider = scope.ServiceProvider.GetRequiredService<IBaysProvider>();

                        if (missionOperation != null)
                        {
                            bayProvider.AssignMissionOperation(bay.Id, mission.Id, missionOperation.Id);

                            this.Logger.LogDebug($"Bay #{bay.Id}: busy executing mission operation id='{bay.CurrentMissionOperationId}'.");

                            this.NotifyNewMissionOperationAvailable(bay, pendingMissions.Count());
                        }
                        else
                        {
                            bayProvider.AssignMissionOperation(bay.Id, null, null);

                            this.Logger.LogDebug($"Bay #{bay.Id}: no more operations available for mission id='{mission.Id}'.");
                        }
                    }
                }
                catch (WMS.Data.WebAPI.Contracts.SwaggerException ex)
                {
                    this.Logger.LogError(ex, $"Unable to load details for mission id='{bay.CurrentMissionId}' from WMS.");
                }
            }
        }

        private void NotifyNewMissionOperationAvailable(Bay bay, int pendingMissionsCount)
        {
            var data = new NewMissionOperationAvailable
            {
                MissionId = bay.CurrentMissionId.Value,
                MissionOperationId = bay.CurrentMissionOperationId.Value,
                PendingMissionsCount = pendingMissionsCount
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
                await this.ExecuteNextMissionAsync();

                WaitHandle.WaitAny(waitHandles);
            }
            while (!this.StoppingToken.IsCancellationRequested);
        }

        #endregion
    }
}
