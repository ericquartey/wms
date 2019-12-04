using System;
using System.Diagnostics.Contracts;
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
    internal sealed partial class MissionSchedulingService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly IMachineMissionsProvider machineMissionsProvider;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionOperationsProvider missionOperationsProvider;

        private readonly IMissionsDataService missionsDataService;

        private bool dataLayerIsReady;

        #endregion

        #region Constructors

        public MissionSchedulingService(
            IConfiguration configuration,
            IMachinesDataService machinesDataService,
            IMachineMissionsProvider missionsProvider,
            IMissionOperationsProvider missionOperationsProvider,
            IMissionsDataService missionsDataService,
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
            this.machinesDataService = machinesDataService ?? throw new ArgumentNullException(nameof(machinesDataService));
            this.machineMissionsProvider = missionsProvider ?? throw new ArgumentNullException(nameof(missionsProvider));
            this.missionOperationsProvider = missionOperationsProvider ?? throw new ArgumentNullException(nameof(missionOperationsProvider));
        }

        #endregion

        #region Methods

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
            Contract.Requires(messageData != null);

            if (!this.dataLayerIsReady)
            {
                this.Logger.LogError("DataLayer is not ready");
                return;
            }

            if (!this.configuration.IsWmsEnabled())
            {
                this.Logger.LogError("Wms is not enabled.");
                return;
            }

            if (messageData.MachineMode is MachineMode.Automatic)
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
                            await missionSchedulingProvider.ScheduleMissionsAsync(bay.Number, true);
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
            Contract.Requires(message != null);
            Contract.Requires(message.Data is AssignedMissionOperationChangedMessageData);

            var messageData = message.Data as AssignedMissionOperationChangedMessageData;

            if (messageData.MissionId.HasValue)
            {
                var bayNumber = messageData.BayNumber;
                var missionId = messageData.MissionId.Value;
                var wmsMission = await this.missionsDataService.GetByIdAsync(missionId);
                var newOperations = wmsMission.Operations
                    .Where(o => o.Status == MissionOperationStatus.New);
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
                        var currentOperation = await this.missionOperationsProvider.GetByIdAsync(messageData.MissionOperationId);

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
