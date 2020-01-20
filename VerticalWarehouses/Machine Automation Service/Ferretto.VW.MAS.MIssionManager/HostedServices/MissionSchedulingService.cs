using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.MachineManager;
using Ferretto.VW.MAS.MachineManager.MissionMove;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
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

        private readonly WMS.Data.WebAPI.Contracts.IMissionOperationsWmsWebService missionOperationsWmsWebService;

        private readonly WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsWmsWebService;

        private bool dataLayerIsReady;

        #endregion

        #region Constructors

        public MissionSchedulingService(
            IConfiguration configuration,
            WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsWmsWebService,
            WMS.Data.WebAPI.Contracts.IMissionOperationsWmsWebService missionOperationsWmsWebService,
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.missionsWmsWebService = missionsWmsWebService ?? throw new ArgumentNullException(nameof(missionsWmsWebService));
            this.missionOperationsWmsWebService = missionOperationsWmsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWmsWebService));
        }

        #endregion

        #region Methods

        public async Task ScheduleCompactingMissionsAsync(IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();

            if (!missionsDataProvider.GetAllActiveMissions().Any())
            {
                serviceProvider.GetRequiredService<IMissionSchedulingProvider>().QueueLoadingUnitCompactingMission(serviceProvider);
            }
        }

        public async Task ScheduleMissionsOnBayAsync(BayNumber bayNumber, IServiceProvider serviceProvider, bool restore = false)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadingUnitProvider>();

            var activeMissions = missionsDataProvider.GetAllActiveMissionsByBay(bayNumber);

            var missionToRestore = activeMissions.SingleOrDefault(x => (x.Status == MissionStatus.Executing || x.Status == MissionStatus.Waiting)
                && x.IsMissionToRestore());
            if (missionToRestore != null)
            {
                if (restore)
                {
                    moveLoadingUnitProvider.ResumeMoveLoadUnit(missionToRestore.Id, LoadingUnitLocation.NoLocation, LoadingUnitLocation.NoLocation, bayNumber, null, MessageActor.MissionManager);
                }
                else
                {
                    this.Logger.LogTrace($"ScheduleMissionsAsync: waiting for mission to restore {missionToRestore.WmsId}, LoadUnit {missionToRestore.LoadUnitId}; bay {bayNumber}");
                }

                return;
            }

            if (activeMissions.Any(m => m.Status == MissionStatus.Completing))
            {
                // there is a mission being completed: do not process any other missions until this is completed
                return;
            }

            var mission = activeMissions.FirstOrDefault(x => x.Status == MissionStatus.Executing || x.Status == MissionStatus.Waiting);
            if (mission is null)
            {
                mission = activeMissions.FirstOrDefault(x => x.Status == MissionStatus.New
                    && (x.MissionType == MissionType.IN
                        || x.MissionType == MissionType.OUT
                        || x.MissionType == MissionType.WMS
                    ));
                if (mission is null)
                {
                    // no more missions are available for scheduling on this bay
                    this.NotifyAssignedMissionOperationChanged(bayNumber, null, null);
                    return;
                }
            }

            System.Diagnostics.Debug.Assert(mission != null);

            if (mission.WmsId.HasValue)
            {
                if (!this.configuration.IsWmsEnabled())
                {
                    this.Logger.LogTrace("Cannot perform mission scheduling, because WMS is not enabled.");
                    return;
                }

                var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                var wmsMission = await this.missionsWmsWebService.GetByIdAsync(mission.WmsId.Value);
                var newOperations = wmsMission.Operations.Where(o => o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Completed && o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Error);
                if (newOperations.Any())
                {
                    if (mission.Status is MissionStatus.New)
                    {
                        // activate new mission
                        var cellsProvider = serviceProvider.GetRequiredService<ICellsProvider>();
                        var sourceCell = cellsProvider.GetByLoadingUnitId(mission.LoadUnitId);
                        if (sourceCell is null)
                        {
                            this.Logger.LogDebug($"Bay {bayNumber}: WMS mission {mission.WmsId} can not start because LoadUnit {mission.LoadUnitId} is not in a cell.");
                        }
                        if (sourceCell is null)
                        {
                            this.Logger.LogDebug($"Bay {bayNumber}: WMS mission {mission.WmsId} can not start because LoadUnit {mission.LoadUnitId} is not in a cell.");
                        }
                        else
                        {
                            mission.Status = MissionStatus.Executing;
                            missionsDataProvider.Update(mission);

                            moveLoadingUnitProvider.ActivateMove(mission.Id, mission.MissionType, mission.LoadUnitId, bayNumber, MessageActor.MissionManager);
                        }
                    }
                    else if (mission.Status is MissionStatus.Waiting)
                    {
                        var newOperation = newOperations.OrderBy(o => o.Priority).First();
                        this.Logger.LogInformation("Bay {bayNumber}: WMS mission {missionId} has operation {operationId} to execute.", mission.TargetBay, mission.WmsId.Value, newOperation.Id);

                        await this.missionOperationsWmsWebService.ExecuteAsync(newOperation.Id);

                        baysDataProvider.AssignWmsMission(mission.TargetBay, mission, newOperation.Id);
                        this.NotifyAssignedMissionOperationChanged(mission.TargetBay, wmsMission.Id, newOperation.Id);
                    }
                    //else if (mission.Status == MissionStatus.Waiting)
                    //{
                    //    var position = baysDataProvider.GetPositionByLocation(mission.LoadUnitDestination);
                    //    if (!position.IsUpper)
                    //    {
                    //        var loadingUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(mission.LoadUnitId);
                    //        moveLoadingUnitProvider.ResumeMoveLoadUnit(mission.Id, loadingUnitSource, loadingUnitSource, bayNumber, null, MessageActor.MissionManager);
                    //        return;
                    //    }
                    //}
                }
                else if (mission.Status is MissionStatus.Executing || mission.Status is MissionStatus.Waiting)
                {
                    // wms mission is finished => schedule loading unit movement back to cell
                    mission.Status = MissionStatus.Completing;
                    missionsDataProvider.Update(mission);

                    this.Logger.LogInformation("Bay {bayNumber}: WMS mission {missionId} completed.", bayNumber, mission.WmsId.Value);

                    baysDataProvider.ClearMission(bayNumber);
                    this.NotifyAssignedMissionOperationChanged(bayNumber, null, null);

                    // check if there are other missions for this LU in this bay
                    var nextMission = activeMissions.FirstOrDefault(m =>
                        m.LoadUnitId == mission.LoadUnitId
                        &&
                        m.WmsId.HasValue
                        &&
                        m.WmsId != mission.WmsId);

                    var loadingUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(mission.LoadUnitId);

                    if (nextMission is null)
                    {
                        // send back the loading unit to the cell
                        moveLoadingUnitProvider.ResumeMoveLoadUnit(mission.Id, loadingUnitSource, LoadingUnitLocation.Cell, bayNumber, null, MessageActor.MissionManager);
                    }
                    else
                    {
                        // close current mission
                        moveLoadingUnitProvider.StopMove(mission.Id, bayNumber, bayNumber, MessageActor.MissionManager);

                        // activate new mission
                        moveLoadingUnitProvider.ActivateMove(nextMission.Id, nextMission.MissionType, nextMission.LoadUnitId, bayNumber, MessageActor.MissionManager);
                    }
                }
            }
            else if (mission.Status is MissionStatus.New)
            {
                var cellsProvider = serviceProvider.GetRequiredService<ICellsProvider>();
                if (mission.MissionType is MissionType.OUT)
                {
                    var sourceCell = cellsProvider.GetByLoadingUnitId(mission.LoadUnitId);
                    if (sourceCell is null)
                    {
                        this.Logger.LogDebug($"Bay {bayNumber}: loading unit  {mission.LoadUnitId} cannot be moved to bay because it is not located in a cell.");
                    }
                    else
                    {
                        moveLoadingUnitProvider.ActivateMove(mission.Id, mission.MissionType, mission.LoadUnitId, bayNumber, MessageActor.MissionManager);
                    }
                }
                else if (mission.MissionType is MissionType.IN)
                {
                    moveLoadingUnitProvider.ActivateMoveToCell(mission.Id, mission.MissionType, mission.LoadUnitId, bayNumber, MessageActor.MissionManager);
                }
            }
        }

        private static void GetPersistedMissions(IServiceProvider serviceProvider, IEventAggregator eventAggregator)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();

            var missions = missionsDataProvider.GetAllExecutingMissions().ToList();
            foreach (var mission in missions)
            {
                if (mission.RestoreStep == MissionStep.NotDefined)
                {
                    mission.RestoreStep = mission.Step;
                }
                IMissionMoveBase newStep;

                if (mission.RestoreStep == MissionStep.BayChain)
                {
                    mission.NeedHomingAxis = Axis.BayChain;
                    newStep = new MissionMoveErrorStep(mission, serviceProvider, eventAggregator);
                }
                else if (mission.RestoreStep == MissionStep.LoadElevator)
                {
                    mission.NeedMovingBackward = true;
                    mission.NeedHomingAxis = Axis.Horizontal;
                    newStep = new MissionMoveErrorLoadStep(mission, serviceProvider, eventAggregator);
                }
                else if (mission.RestoreStep == MissionStep.DepositUnit)
                {
                    mission.NeedMovingBackward = true;
                    mission.NeedHomingAxis = Axis.Horizontal;
                    newStep = new MissionMoveErrorDepositStep(mission, serviceProvider, eventAggregator);
                }
                else
                {
                    mission.NeedHomingAxis = Axis.Horizontal;
                    newStep = new MissionMoveErrorStep(mission, serviceProvider, eventAggregator);
                }
                newStep.OnEnter(null);
            }
        }

        private void GenerateHoming(IBaysDataProvider bayProvider)
        {
            var bays = bayProvider.GetAll();
            if (bays.Any(x => x.Carousel != null && !x.Carousel.IsHomingExecuted))
            {
                var bayNumber = bays.First(x => x.Carousel != null && !x.Carousel.IsHomingExecuted).Number;
                IHomingMessageData homingData = new HomingMessageData(Axis.BayChain, Calibration.FindSensor, null, false);

                this.EventAggregator
                    .GetEvent<CommandEvent>()
                    .Publish(
                        new CommandMessage(
                            homingData,
                            "Execute Homing Command",
                            MessageActor.DeviceManager,
                            MessageActor.MissionManager,
                            MessageType.Homing,
                            bayNumber));
            }
            else
            {
                IHomingMessageData homingData = new HomingMessageData(Axis.HorizontalAndVertical, Calibration.FindSensor, null, false);

                this.EventAggregator
                    .GetEvent<CommandEvent>()
                    .Publish(
                        new CommandMessage(
                            homingData,
                            "Execute Homing Command",
                            MessageActor.DeviceManager,
                            MessageActor.MissionManager,
                            MessageType.Homing,
                            BayNumber.BayOne));
            }
        }

        private async Task InvokeSchedulerAsync()
        {
            if (!this.dataLayerIsReady)
            {
                this.Logger.LogTrace("Cannot perform mission scheduling, because data layer is not ready.");
                return;
            }

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var modeProvider = scope.ServiceProvider.GetRequiredService<IMachineModeProvider>();
                var bayProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();

                switch (modeProvider.GetCurrent())
                {
                    case MachineMode.SwitchingToAutomatic:
                        {
                            var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                            var machineModeDataProvider = scope.ServiceProvider.GetRequiredService<IMachineModeVolatileDataProvider>();
                            var missionsDataProvider = scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>();

                            if (!machineProvider.IsHomingExecuted
                                && !missionsDataProvider.GetAllActiveMissions().Any(m => m.Step >= MissionStep.Error)
                                )
                            {
                                this.GenerateHoming(bayProvider);
                            }
                            else
                            {
                                machineModeDataProvider.Mode = MachineMode.Automatic;
                                this.Logger.LogInformation($"Machine status switched to {machineModeDataProvider.Mode}");
                            }
                        }
                        break;

                    case MachineMode.Automatic:
                        {
                            foreach (var bay in bayProvider.GetAll())
                            {
                                try
                                {
                                    if (bay.IsActive)
                                    {
                                        await this.ScheduleMissionsOnBayAsync(bay.Number, scope.ServiceProvider, true);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    this.Logger.LogError(ex, "Failed to schedule missions on bay {number}.", bay.Number);
                                }
                            }
                        }
                        break;

                    case MachineMode.SwitchingToCompact:
                        {
                            var missionsDataProvider = scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
                            var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();

                            if (!missionsDataProvider.GetAllActiveMissions().Any())
                            {
                                if (machineProvider.IsHomingExecuted)
                                {
                                    var machineModeDataProvider = scope.ServiceProvider.GetRequiredService<IMachineModeVolatileDataProvider>();
                                    machineModeDataProvider.Mode = MachineMode.Compact;
                                    this.Logger.LogInformation($"Machine status switched to {machineModeDataProvider.Mode}");
                                }
                                else
                                {
                                    this.GenerateHoming(bayProvider);
                                }
                            }
                        }
                        break;

                    case MachineMode.Compact:
                        {
                            await this.ScheduleCompactingMissionsAsync(scope.ServiceProvider);
                        }
                        break;

                    default:
                        {
                            this.Logger.LogDebug("Cannot perform mission scheduling, because machine is not in automatic mode.");
                        }
                        break;
                }
            }
        }

        private void NotifyAssignedMissionOperationChanged(
            BayNumber bayNumber,
            int? missionId,
            int? missionOperationId)
        {
            var data = new AssignedMissionOperationChangedMessageData
            {
                BayNumber = bayNumber,
                MissionId = missionId,
                MissionOperationId = missionOperationId,
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

        private async Task OnBayOperationalStatusChangedAsync()
        {
            await this.InvokeSchedulerAsync();
        }

        private async Task OnDataLayerReadyAsync(IServiceProvider serviceProvider)
        {
            GetPersistedMissions(serviceProvider, this.EventAggregator);
            this.dataLayerIsReady = true;
            await this.InvokeSchedulerAsync();
        }

        private async Task OnLoadingUnitMovedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            Contract.Requires(message != null);
            Contract.Requires(message.Data is MoveLoadingUnitMessageData);

            var luData = message.Data as MoveLoadingUnitMessageData;

            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();

            try
            {
                var mission = missionsDataProvider.GetById(luData.MissionId.Value);
                if (luData.Destination == LoadingUnitLocation.Elevator
                    && !mission.WmsId.HasValue)
                {
                    if (luData.Source != LoadingUnitLocation.Cell)
                    {
                        var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                        var bay = baysDataProvider.GetByLoadingUnitLocation(luData.Source);
                        if (bay.CurrentMission != null)
                        {
                            baysDataProvider.ClearMission(bay.Number);
                            missionsDataProvider.Complete(bay.CurrentMission.Id);
                        }
                    }
                    missionsDataProvider.Complete(mission.Id);
                }
                else if (!luData.DestinationCellId.HasValue)
                // loading unit to bay mission
                {
                    var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                    if (mission.WmsId.HasValue)
                    {
                        var wmsMission = await this.missionsWmsWebService.GetByIdAsync(mission.WmsId.Value);
                        var newOperations = wmsMission.Operations.Where(o => o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Completed && o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Error);
                        if (newOperations.Any())
                        {
                            var newOperation = newOperations.OrderBy(o => o.Priority).First();
                            this.Logger.LogInformation("Bay {bayNumber}: WMS mission {missionId} has operation {operationId} to execute.", mission.TargetBay, mission.WmsId.Value, newOperation.Id);

                            await this.missionOperationsWmsWebService.ExecuteAsync(newOperation.Id);

                            baysDataProvider.AssignWmsMission(mission.TargetBay, mission, newOperation.Id);
                            this.NotifyAssignedMissionOperationChanged(mission.TargetBay, wmsMission.Id, newOperation.Id);
                        }
                    }
                    else
                    {
                        baysDataProvider.AssignWmsMission(mission.TargetBay, mission, null);
                        this.NotifyAssignedMissionOperationChanged(mission.TargetBay, null, null);
                    }
                }
                else
                // any other mission type
                {
                    missionsDataProvider.Complete(mission.Id);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Failed to process mission: {ex.Message}");
            }

            await this.InvokeSchedulerAsync();
        }

        private async Task OnMachineModeChangedAsync()
        {
            await this.InvokeSchedulerAsync();
        }

        private async Task OnNewMachineMissionAvailableAsync()
        {
            await this.InvokeSchedulerAsync();
        }

        private async Task OnOperationComplete(MissionOperationCompletedMessageData messageData)
        {
            Contract.Requires(messageData != null);

            if (!this.dataLayerIsReady)
            {
                this.Logger.LogTrace("Cannot perform mission scheduling, because data layer is not ready.");
                return;
            }

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();

                var bay = baysDataProvider
                    .GetAll()
                    .SingleOrDefault(b => b.CurrentWmsMissionOperationId == messageData.MissionOperationId);

                if (bay is null)
                {
                    this.Logger.LogWarning("Cannot complete operation {operationId}: none of the bays is currently executing it.", messageData.MissionOperationId);
                }
                else
                {
                    // close operation and schedule next
                    baysDataProvider.ClearMission(bay.Number);

                    var currentMode = scope.ServiceProvider
                        .GetRequiredService<IMachineModeProvider>()
                        .GetCurrent();

                    switch (currentMode)
                    {
                        case MachineMode.Automatic:
                            await this.ScheduleMissionsOnBayAsync(bay.Number, scope.ServiceProvider);
                            break;

                        case MachineMode.Compact:
                            await this.ScheduleCompactingMissionsAsync(scope.ServiceProvider);
                            break;
                    }
                }
            }
        }

        #endregion
    }
}
