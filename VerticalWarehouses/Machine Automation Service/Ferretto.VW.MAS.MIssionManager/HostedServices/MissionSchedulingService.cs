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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed class MissionSchedulingService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly IMachineMissionsProvider machineMissionsProvider;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionOperationsDataService missionOperationsDataService;

        private readonly IMissionsDataService missionsDataService;

        private bool dataLayerIsReady;

        #endregion

        #region Constructors

        public MissionSchedulingService(
            IConfiguration configuration,
            IMachinesDataService machinesDataService,
            IMachineMissionsProvider missionsProvider,
            IMissionsDataService missionsDataService,
            IMissionOperationsDataService missionOperationsDataService,
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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
            // do nothing
            return Task.CompletedTask;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            switch (message.Type)
            {
                case MessageType.MissionOperationCompleted:
                    await this.OnOperationComplete(message.Data as MissionOperationCompletedMessageData);
                    break;

                case MessageType.AssignedMissionOperationChanged:
                    await this.OnOperationChangedAsync(message);
                    break;

                case MessageType.DataLayerReady:
                    this.dataLayerIsReady = true;
                    break;
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

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(notificationMessage);
        }

        private async Task OnMoveLoadingUnitAsync(CommandMessage command)
        {
            if (!this.configuration.IsWmsEnabled() || !this.dataLayerIsReady)
            {
                return;
            }

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
                            if (this.machineMissionsProvider.TryCreateWmsMission(FsmType.MoveLoadingUnit, messageData, out var missionId))
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
                                            baysDataProvider.AssignWmsMission(messageData.TargetBay, wmsMission.Id, operation.Id);
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
                this.NotifyAssignedMissionOperationChanged(bayNumber, missionId, operation?.Id ?? 0, pendingMissionsCount);
            }
        }

        private async Task OnOperationComplete(MissionOperationCompletedMessageData messageData)
        {
            if (messageData is null)
            {
                this.Logger.LogError($"Message data not correct ");
                return;
            }
            if (!this.dataLayerIsReady)
            {
                this.Logger.LogError($"DataLayer not ready for operation id={messageData.MissionOperationId}.");
                return;
            }
            if (!this.configuration.IsWmsEnabled())
            {
                this.Logger.LogError($"Wms not enabled for operation id={messageData.MissionOperationId}.");
                return;
            }

            try
            {
                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    var bayProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();

                    var bay = bayProvider
                        .GetAll()
                        .Where(b => b.CurrentWmsMissionOperationId.HasValue && b.CurrentMissionId.HasValue)
                        .SingleOrDefault(b => b.CurrentWmsMissionOperationId == messageData.MissionOperationId);

                    if (bay is null)
                    {
                        this.Logger.LogWarning($"None of the bays is currently executing operation id={messageData.MissionOperationId}.");
                    }
                    else
                    {
                        // check what is the next operation for this bay
                        var currentOperation = await this.missionOperationsDataService.GetByIdAsync(messageData.MissionOperationId);
                        var currentWmsMission = await this.missionsDataService.GetByIdAsync(currentOperation.MissionId);
                        var newOperations = currentWmsMission.Operations
                            .Where(o => o.Status == WMS.Data.WebAPI.Contracts.MissionOperationStatus.New);
                        if (newOperations.Any())
                        {
                            // there are more operations for the same wms mission
                            var newOperation = newOperations.OrderBy(o => o.Priority).First();

                            bayProvider.AssignWmsMission(bay.Number, currentWmsMission.Id, newOperation.Id);

                            var missionsDataProvider = scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
                            var activeMissions = missionsDataProvider.GetAllActiveMissionsByBay(bay.Number);
                            this.NotifyAssignedMissionOperationChanged(bay.Number, currentWmsMission.Id, newOperation.Id, activeMissions.Count());
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
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Failed to continue Wms operation {messageData.MissionOperationId}: {ex.Message}");
            }
        }

        #endregion
    }
}
