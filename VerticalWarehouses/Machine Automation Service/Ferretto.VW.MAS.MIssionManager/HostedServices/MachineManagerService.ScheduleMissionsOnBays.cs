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

            bay.CurrentMissionId = missionId;
            this.Logger.LogInformation($"Bay {bay.Number}: new mission id='{bay.CurrentMissionId}' assigned.");

            var mission = await this.missionsDataService.GetByIdAsync(bay.CurrentMissionId.Value);

            var missionOperation = mission.Operations?
                .OrderBy(o => o.Priority)
                .FirstOrDefault(o => o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Completed);

            if (missionOperation != null)
            {
                baysProvider.AssignMissionOperation(bay.Number, mission.Id, missionOperation.Id);

                this.Logger.LogDebug($"Bay {bay.Number}: moving loading unit '{mission.LoadingUnitCode}' to bay.");
                await Task.Delay(3000);
                this.Logger.LogDebug($"Bay {bay.Number}: loading unit '{mission.LoadingUnitCode}' is now in bay.");

                await this.missionOperationsDataService.ExecuteAsync(missionOperation.Id);
                this.Logger.LogDebug($"Bay {bay.Number}: busy executing mission operation id='{bay.CurrentMissionOperationId}'.");

                this.NotifyNewMissionOperationAvailable(bay, pendingMissionsCount);
            }
            else
            {
                baysProvider.AssignMissionOperation(bay.Number, null, null);

                this.Logger.LogDebug($"Bay {bay.Number}: no more operations available for mission id='{mission.Id}'.");
            }
        }

        private void NotifyNewMissionOperationAvailable(Bay bay, int pendingMissionsCount)
        {
            var data = new NewMissionOperationAvailable
            {
                BayNumber = bay.Number,
                MissionId = bay.CurrentMissionId.Value,
                MissionOperationId = bay.CurrentMissionOperationId.Value,
                PendingMissionsCount = pendingMissionsCount,
            };

            var notificationMessage = new NotificationMessage(
                data,
                "New Mission Operation Available",
                MessageActor.Any,
                MessageActor.MachineManager,
                MessageType.NewMissionOperationAvailable,
                bay.Number);

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
