using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.MachineManager;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class WmsMissionProxyService
    {
        #region Methods

        private void NotifyAssignedMissionOperationChanged(BayNumber bayNumber, int? missionId, int? missionOperationId, int pendingMissionsCount)
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
                MessageActor.Any,
                MessageActor.MachineManager,
                MessageType.AssignedMissionOperationChanged,
                bayNumber);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(notificationMessage);
        }

        private async Task QueueMissionsAsync()
        {
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                /*  TO REMOVE
                 *
                var modeProvider = scope.ServiceProvider.GetRequiredService<IMachineModeProvider>();

                  TODO ***************
                if (modeProvider.GetCurrent() != MachineMode.Automatic)
                {
                    return;
                }
                */

                var machineId = 1; // TODO ***** use serial number instead
                var missions = (await this.machinesDataService.GetMissionsByIdAsync(machineId))
                    .OrderBy(m => m.Priority)
                    .Where(m =>
                        m.BayId.HasValue
                        &&
                        m.Status != WMS.Data.WebAPI.Contracts.MissionStatus.Completed);

                var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();

                foreach (var bay in baysDataProvider.GetAll())
                {
                    var pendingMissionsOnBay = missions.Where(m => m.BayId.Value == (int)bay.Number);

                    if (pendingMissionsOnBay.Any())
                    {
                        var pendingMissionOperationsCount = pendingMissionsOnBay.SelectMany(m => m.Operations).Count();
                        if (bay.Status is BayStatus.Idle)
                        {
                            await this.QueueNextMissionAsync(
                                bay,
                                pendingMissionsOnBay.First().Id,
                                pendingMissionOperationsCount,
                                baysDataProvider);
                        }
                        else
                        {
                            this.Logger.LogInformation(
                                $"Bay {bay.Number}: the bay is not idle (current op. id: {bay.CurrentMissionId}). There are {pendingMissionsOnBay.Count()} pending missions (total ops.: {pendingMissionOperationsCount}).");

                            this.NotifyAssignedMissionOperationChanged(
                                bay.Number,
                                bay.CurrentMissionId,
                                bay.CurrentMissionOperationId,
                                pendingMissionOperationsCount);
                        }
                    }
                }
            }
        }

        private async Task QueueNextMissionAsync(Bay bay, int missionId, int pendingMissionOperationsCount, IBaysDataProvider baysDataProvider)
        {
            System.Diagnostics.Debug.Assert(!bay.CurrentMissionId.HasValue);

            this.Logger.LogInformation($"Bay {bay.Number}: new mission id='{missionId}' assigned.");

            var mission = await this.missionsDataService.GetByIdAsync(missionId);

            if (mission.Operations is null)
            {
                // TODO **************
                // LU mission
                return;
            }

            var newOperations = mission.Operations
                .Where(o => o.Status == WMS.Data.WebAPI.Contracts.MissionOperationStatus.New);

            if (newOperations.Any())
            {
                this.missionSchedulingProvider.QueueBayMission(mission.LoadingUnitId, bay.Number, mission.Id);

                // MOCK: simulate mission
                //var operation = newOperations.OrderBy(o => o.Priority).First();
                //baysDataProvider.AssignMissionOperation(bay.Number, mission.Id, operation.Id);

                //this.Logger.LogDebug($"Bay {bay.Number}: moving loading unit id={mission.LoadingUnitId} to bay ...");
                //await Task.Delay(10 * 1000);
                //this.Logger.LogDebug($"Bay {bay.Number}: loading unit id={mission.LoadingUnitId} is now in bay.");

                //await this.missionOperationsDataService.ExecuteAsync(operation.Id);
                //this.Logger.LogDebug($"Bay {bay.Number}: busy executing mission operation id='{operation.Id}'.");

                //this.NotifyAssignedMissionOperationChanged(bay.Number, mission.Id, operation.Id, pendingMissionOperationsCount);
                // end MOCK
            }
            else
            {
                this.Logger.LogDebug($"Bay {bay.Number}: all the operations (total={mission.Operations.Count}) for mission id='{mission.Id}' are actioned.");

                baysDataProvider.AssignMissionOperation(bay.Number, null, null);

                this.NotifyAssignedMissionOperationChanged(bay.Number, null, null, pendingMissionOperationsCount);
            }
        }

        private async Task ScheduleMissionsOnBaysAsync()
        {
            var waitHandles = new WaitHandle[]
            {
                this.bayStatusChangedEvent,
                this.CancellationToken.WaitHandle,
            };

            do
            {
                try
                {
                    await this.QueueMissionsAsync();
                }
                catch (Exception ex) when (ex is OperationCanceledException)
                {
                    this.Logger.LogDebug("Terminating mission manager scheduler.");
                    return;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error during the scheduling of missions.");
                }
                finally
                {
                    WaitHandle.WaitAny(waitHandles);
                }
            }
            while (!this.CancellationToken.IsCancellationRequested);
        }

        #endregion
    }
}
