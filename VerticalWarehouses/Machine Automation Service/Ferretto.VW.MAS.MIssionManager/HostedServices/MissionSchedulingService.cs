using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils;
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

                case MessageType.MachineMode:
                    await this.OnMachineModeChangedAsync(message.Data as MachineModeMessageData);
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

        /// <summary>
        /// try to assign automatic missions to bays when switching to Automatic mode
        /// </summary>
        /// <param name="messageData"></param>
        private async Task OnMachineModeChangedAsync(MachineModeMessageData messageData)
        {
            if (messageData is null)
            {
                this.Logger.LogError($"Message data not correct ");
                return;
            }
            if (!this.dataLayerIsReady)
            {
                this.Logger.LogError($"DataLayer not ready");
                return;
            }
            if (!this.configuration.IsWmsEnabled())
            {
                this.Logger.LogError($"Wms not enabled.");
                return;
            }
            if (messageData.MachineMode == MachineMode.Automatic)
            {
                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    var bayProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();

                    var bays = bayProvider.GetAll();
                    var missionSchedulingProvider = scope.ServiceProvider.GetRequiredService<IMissionSchedulingProvider>();
                    foreach (var bay in bays)
                    {
                        try
                        {
                            await missionSchedulingProvider.ScheduleMissionsAsync(bay.Number);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError($"Failed to Schedule missions to bay {bay.Number}: {ex.Message}");
                        }
                    }
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

                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                    var machineId = machineProvider.GetIdentity();
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
                        // close operation and schedule next
                        bayProvider.AssignWmsMission(bay.Number, currentOperation.MissionId, null);
                        var missionSchedulingProvider = scope.ServiceProvider.GetRequiredService<IMissionSchedulingProvider>();
                        try
                        {
                            await missionSchedulingProvider.ScheduleMissionsAsync(bay.Number);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError($"Failed to Schedule missions to bay {bay.Number}: {ex.Message}");
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
