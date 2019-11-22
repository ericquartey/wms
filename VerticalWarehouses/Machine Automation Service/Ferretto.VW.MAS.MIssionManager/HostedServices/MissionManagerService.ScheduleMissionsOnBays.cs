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
    internal partial class MissionManagerService
    {
        #region Methods

        private async Task ExecuteNextMissionAsync()
        {
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var modeProvider = scope.ServiceProvider.GetRequiredService<IMachineModeProvider>();
                /*
                 *
                  TODO ***************
                if (modeProvider.GetCurrent() != MachineMode.Automatic)
                {
                    return;
                }
                */
                var baysProvider = scope.ServiceProvider.GetRequiredService<IBaysProvider>();

                var idleBays = baysProvider
                    .GetAll()
                    .Where(b => b.Status == BayStatus.Idle);

                if (!idleBays.Any())
                {
                    this.Logger.LogInformation($"There are no free active bays.");
                    return;
                }

                this.Logger.LogInformation($"There are {idleBays.Count()} idle (useable) bays.");

                var machineId = 1; // TODO ***** use serial number instead
                var missions = (await this.machinesDataService.GetMissionsByIdAsync(machineId))
                    .OrderBy(m => m.Priority)
                    .Where(m =>
                        m.BayId.HasValue
                        &&
                        m.Status != WMS.Data.WebAPI.Contracts.MissionStatus.Completed);

                foreach (var bay in idleBays)
                {
                    System.Diagnostics.Debug.Assert(!bay.CurrentMissionId.HasValue);

                    var pendingMissionsOnBay = missions
                        .Where(m => m.BayId.Value == (int)bay.Number);

                    if (pendingMissionsOnBay.Any())
                    {
                        await this.ExecuteNextMissionAsync(
                            bay,
                            pendingMissionsOnBay.First().Id,
                            pendingMissionsOnBay.Count(),
                            baysProvider);
                    }
                    else
                    {
                        this.Logger.LogInformation($"Bay {bay.Number}: there are no pending missions to execute on bay.");
                    }
                }
            }
        }

        private async Task ExecuteNextMissionAsync(Bay bay, int missionId, int pendingMissionsCount, IBaysProvider baysProvider)
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
                var operation = newOperations.OrderBy(o => o.Priority).First();
                baysProvider.AssignMissionOperation(bay.Number, mission.Id, operation.Id);

                this.Logger.LogDebug($"Bay {bay.Number}: moving loading unit id={mission.LoadingUnitId} to bay ...");
                await Task.Delay(10 * 1000);
                this.Logger.LogDebug($"Bay {bay.Number}: loading unit id={mission.LoadingUnitId} is now in bay.");

                await this.missionOperationsDataService.ExecuteAsync(operation.Id);
                this.Logger.LogDebug($"Bay {bay.Number}: busy executing mission operation id='{operation.Id}'.");

                this.NotifyAssignedMissionOperationChanged(bay.Number, mission.Id, operation.Id, pendingMissionsCount);
            }
            else
            {
                this.Logger.LogDebug($"Bay {bay.Number}: all the operations (total={mission.Operations.Count}) for mission id='{mission.Id}' are actioned.");

                baysProvider.AssignMissionOperation(bay.Number, null, null);

                this.NotifyAssignedMissionOperationChanged(bay.Number, null, null, pendingMissionsCount);
            }
        }

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
                    await this.ExecuteNextMissionAsync();
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
