using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed class MissionSchedulingService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly IMachineMissionsProvider machineMissionsProvider;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionOperationsDataService missionOperationsDataService;

        private readonly IMissionsDataService missionsDataService;

        #endregion

        #region Constructors

        public MissionSchedulingService(
            IMachinesDataService machinesDataService,
            IMachineMissionsProvider missionsProvider,
            IMissionsDataService missionsDataService,
            IMissionOperationsDataService missionOperationsDataService,
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machinesDataService = machinesDataService ?? throw new ArgumentNullException(nameof(machinesDataService));
            this.machineMissionsProvider = missionsProvider ?? throw new ArgumentNullException(nameof(missionsProvider));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
            this.missionOperationsDataService = missionOperationsDataService ?? throw new ArgumentNullException(nameof(missionOperationsDataService));
        }

        #endregion

        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return
                command.Destination is CommonUtils.Messages.Enumerations.MessageActor.Any
                ||
                command.Destination is CommonUtils.Messages.Enumerations.MessageActor.MissionManager;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return
                notification.Destination is CommonUtils.Messages.Enumerations.MessageActor.Any
                ||
                notification.Destination is CommonUtils.Messages.Enumerations.MessageActor.MissionManager;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command, IServiceProvider serviceProvider)
        {
            switch (command.Type)
            {
                case MessageType.MoveLoadingUnit:
                    this.OnMoveLoadingUnitAsync(command);
                    break;
            }

            return Task.CompletedTask;
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            switch (message.Type)
            {
                case MessageType.MissionOperationCompleted:
                    this.OnOperationComplete(message);
                    break;

                case MessageType.AssignedMissionOperationChanged:
                    this.OnOperationChangedAsync(message);
                    break;
            }
            return Task.CompletedTask;
        }

        private async Task OnMoveLoadingUnitAsync(CommandMessage command)
        {
            if (command is null)
            {
                return;
            }

            if (command.Data is MoveLoadingUnitMessageData messageData)
            {
                switch (messageData.CommandAction)
                {
                    case CommandAction.Start:
                        try
                        {
                            if (this.machineMissionsProvider.TryCreateWmsMission(FSMType.MoveLoadingUnit, messageData, out var missionId))
                            {
                                try
                                {
                                    this.machineMissionsProvider.StartMachineMission(missionId, command);

                                    if (messageData.WmsId.HasValue)
                                    {
                                        var wmsMission = await this.missionsDataService.GetByIdAsync(messageData.WmsId.Value);
                                        var newOperations = wmsMission.Operations
                                            .Where(o => o.Status == WMS.Data.WebAPI.Contracts.MissionOperationStatus.New);
                                        var operation = newOperations.OrderBy(o => o.Priority).First();
                                        using (var scope = this.ServiceScopeFactory.CreateScope())
                                        {
                                            var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                                            baysDataProvider.AssignMissionOperation(messageData.TargetBay, wmsMission.Id, operation.Id);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    this.Logger.LogDebug($"Failed to start Move Loading Unit State machine mission {missionId}: {ex.Message}");
                                    this.machineMissionsProvider.StopMachineMission(missionId, StopRequestReason.Stop);
                                }
                            }
                            else
                            {
                                this.Logger.LogDebug($"Conditions not verified for creating Wms mission {messageData.WmsId ?? 0} on Bay {messageData.TargetBay}; LU {messageData.LoadingUnitId}");
                                // TODO try to send LU back to cell or another bay
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError($"Failed to start Move Loading Unit Wms mission {messageData.WmsId ?? 0}: {ex.Message}");
                        }
                        break;
                }
            }
        }

        private async Task OnOperationChangedAsync(NotificationMessage message)
        {
            if (message is null)
            {
                return;
            }

            if (message.Data is AssignedMissionOperationChangedMessageData messageData
                && messageData.MissionId.HasValue)
            {
                var bayNumber = messageData.BayNumber;
                var missionId = messageData.MissionId.Value;
                var wmsMission = await this.missionsDataService.GetByIdAsync(missionId);
                var newOperations = wmsMission.Operations
                    .Where(o => o.Status == WMS.Data.WebAPI.Contracts.MissionOperationStatus.New);
                var operation = newOperations.OrderBy(o => o.Priority).First();
                var machineId = 1; // TODO ***** use serial number instead
                var pendingMissionsOnBay = (await this.machinesDataService.GetMissionsByIdAsync(machineId))
                    .Where(m => m.BayId.Value == (int)bayNumber
                        && m.Status != WMS.Data.WebAPI.Contracts.MissionStatus.Completed);

                var pendingMissionsCount = 0;
                if (pendingMissionsOnBay.Any())
                {
                    pendingMissionsCount = pendingMissionsOnBay.SelectMany(m => m.Operations).Count();
                }
                var data = new AssignedMissionOperationChangedMessageData
                {
                    BayNumber = bayNumber,
                    MissionId = missionId,
                    MissionOperationId = operation?.Id ?? 0,
                    PendingMissionsCount = pendingMissionsCount,
                };

                var notificationMessage = new NotificationMessage(
                    data,
                    $"Mission operation assigned to bay {bayNumber} has changed.",
                    MessageActor.WebApi,
                    MessageActor.MachineManager,
                    MessageType.AssignedMissionOperationChanged,
                    bayNumber);

                this.EventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(notificationMessage);
            }
        }

        private async Task OnOperationComplete(NotificationMessage message)
        {
            if (message is null)
            {
                return;
            }

            if (message.Data is MissionOperationCompletedMessageData messageData)
            {
                try
                {
                    var operation = await this.missionOperationsDataService.GetByIdAsync(messageData.MissionOperationId);
                    var wmsMission = await this.missionsDataService.GetByIdAsync(operation.MissionId);
                    var newOperations = wmsMission.Operations
                        .Where(o => o.Status == WMS.Data.WebAPI.Contracts.MissionOperationStatus.New);
                    if (newOperations.Any())
                    {
                        // wait next operation
                        var newOperation = newOperations.OrderBy(o => o.Priority).First();
                        using (var scope = this.ServiceScopeFactory.CreateScope())
                        {
                            var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                            baysDataProvider.AssignMissionOperation(message.RequestingBay, wmsMission.Id, newOperation.Id);
                        }
                    }
                    else
                    {
                        // are there other missions for this LU in this bay?
                        {
                            // update WmsId in the current machine mission
                        }
                        // else are there other missions for this LU and another bay?
                        {
                            // update WmsId in the current machine mission and move to another bay
                        }
                        // else send back the LU
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError($"Failed to continue Wms operation {messageData.MissionOperationId}: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
