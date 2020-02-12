using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
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

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly WMS.Data.WebAPI.Contracts.IMissionOperationsWmsWebService missionOperationsWmsWebService;

        private readonly WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsWmsWebService;

        private bool dataLayerIsReady;

        #endregion

        #region Constructors

        public MissionSchedulingService(
            IConfiguration configuration,
            WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsWmsWebService,
            WMS.Data.WebAPI.Contracts.IMissionOperationsWmsWebService missionOperationsWmsWebService,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.missionsWmsWebService = missionsWmsWebService ?? throw new ArgumentNullException(nameof(missionsWmsWebService));
            this.missionOperationsWmsWebService = missionOperationsWmsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWmsWebService));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
        }

        #endregion

        #region Methods

        public void ScheduleCompactingMissions(IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();

            if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status != MissionStatus.New))
            {
                serviceProvider.GetRequiredService<IMissionSchedulingProvider>().QueueLoadingUnitCompactingMission(serviceProvider);
            }
        }

        public async Task ScheduleMissionsOnBayAsync(BayNumber bayNumber, IServiceProvider serviceProvider, bool restore = false)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();

            var activeMissions = missionsDataProvider.GetAllActiveMissionsByBay(bayNumber);

            var missionToRestore = activeMissions.OrderBy(o => o.Status)
                .ThenBy(t => t.RestoreStep)
                .FirstOrDefault(x => (x.Status == MissionStatus.Executing || x.Status == MissionStatus.Waiting)
                    && x.IsMissionToRestore());
            if (missionToRestore != null)
            {
                if (restore)
                {
                    moveLoadingUnitProvider.ResumeMoveLoadUnit(
                        missionToRestore.Id,
                        LoadingUnitLocation.NoLocation,
                        LoadingUnitLocation.NoLocation,
                        bayNumber,
                        missionToRestore.WmsId,
                        missionToRestore.MissionType,
                        MessageActor.MissionManager);
                }
                else
                {
                    this.Logger.LogDebug($"ScheduleMissionsAsync: waiting for mission to restore {missionToRestore.WmsId}, LoadUnit {missionToRestore.LoadUnitId}; bay {bayNumber}");
                }
                return;
            }

            if (activeMissions.Any(m => m.Status == MissionStatus.Completing))
            {
                // there is a mission being completed: do not process any other missions until this is completed
                return;
            }

            var missions = activeMissions.Where(x => x.Status == MissionStatus.New
                    || x.Status == MissionStatus.Executing
                    || x.Status == MissionStatus.Waiting)
                .OrderBy(o => o.Status);
            if (!missions.Any())
            {
                // no more missions are available for scheduling on this bay
                this.NotifyAssignedMissionOperationChanged(bayNumber, null, null);
                return;
            }
            foreach (var mission in missions)
            {
                if (mission.Status == MissionStatus.New
                    && mission.MissionType != MissionType.IN
                    && mission.MissionType != MissionType.OUT
                    && mission.MissionType != MissionType.WMS
                    )
                {
                    continue;
                }

                var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

                if (mission.WmsId.HasValue)
                {
                    if (!this.configuration.IsWmsEnabled())
                    {
                        this.Logger.LogTrace("Cannot perform mission scheduling, because WMS is not enabled.");
                        continue;
                    }

                    var wmsMission = await this.missionsWmsWebService.GetByIdAsync(mission.WmsId.Value);
                    var newOperations = wmsMission.Operations.Where(o =>
                        o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Completed
                        &&
                        o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Error);
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
                            else
                            {
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
                            moveLoadingUnitProvider.ResumeMoveLoadUnit(
                                mission.Id,
                                loadingUnitSource,
                                LoadingUnitLocation.Cell,
                                bayNumber,
                                null,
                                MissionType.IN,
                                MessageActor.MissionManager);
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
                else if (mission.Status is MissionStatus.Waiting
                    && mission.Step is MissionStep.BayChain
                    )
                {
                    var loadingUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(mission.LoadUnitId);
                    moveLoadingUnitProvider.ResumeMoveLoadUnit(
                        mission.Id,
                        loadingUnitSource,
                        LoadingUnitLocation.Cell,
                        bayNumber,
                        null,
                        mission.MissionType,
                        MessageActor.MissionManager);
                }
            }
        }

        private static void GetPersistedMissions(IServiceProvider serviceProvider, IEventAggregator eventAggregator)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();

            var missions = missionsDataProvider.GetAllMissions().ToList();
            foreach (var mission in missions)
            {
                if (!mission.IsRestoringType())
                {
                    missionsDataProvider.Delete(mission.Id);
                }
                else if (mission.Status != MissionStatus.Completed
                    && mission.Status != MissionStatus.Aborted
                    && mission.Status != MissionStatus.New)
                {
                    if (mission.RestoreStep == MissionStep.NotDefined)
                    {
                        mission.RestoreStep = mission.Step;
                    }
                    IMissionMoveBase newStep;

                    if (mission.RestoreStep == MissionStep.BayChain
                        || mission.RestoreStep == MissionStep.WaitPick
                        )
                    {
                        var bayProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                        var bay = bayProvider.GetByNumber(mission.TargetBay);
                        if (bay != null
                            && bay.Carousel != null)
                        {
                            mission.NeedHomingAxis = Axis.BayChain;
                        }
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
        }

        private bool GenerateHoming(IBaysDataProvider bayProvider, bool isHomingExecuted)
        {
            if (this.machineVolatileDataProvider.IsHomingActive)
            {
                return true;
            }
            var bays = bayProvider.GetAll();
            bool generated = false;
            if (this.machineVolatileDataProvider.IsBayHomingExecuted.Any(x => !x.Value)
                && bays.All(x => x.CurrentMission == null))
            {
                var bayNumber = bays.FirstOrDefault(x => this.machineVolatileDataProvider.IsBayHomingExecuted.ContainsKey(x.Number)
                    && !this.machineVolatileDataProvider.IsBayHomingExecuted[x.Number]
                    && x.Carousel != null
                    && x.CurrentMission == null
                    && x.Positions.All(p => p.LoadingUnit == null))?.Number ?? BayNumber.None;
                if (bayNumber != BayNumber.None)
                {
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
                    this.Logger.LogDebug($"GenerateHoming: bay {bayNumber}");
                    generated = true;
                }
            }
            if (!generated && !isHomingExecuted)
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
                this.Logger.LogDebug($"GenerateHoming: Elevator");
                generated = true;
            }
            this.machineVolatileDataProvider.IsHomingActive = generated;
            return generated;
        }

        /// <summary>
        /// This method processes the Machine Mode changes and calls the mission scheduler
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private async Task InvokeSchedulerAsync(IServiceProvider serviceProvider)
        {
            if (!this.dataLayerIsReady)
            {
                this.Logger.LogTrace("Cannot perform mission scheduling, because data layer is not ready.");
                return;
            }

            var bayProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

            switch (this.machineVolatileDataProvider.Mode)
            {
                case MachineMode.SwitchingToAutomatic:
                    {
                        // in this machine mode we generate homing for elevator and bays, but only if there are no missions to restore.
                        // if homing is not possible we switch anyway to automatic mode
                        var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                        var activeMissions = missionsDataProvider.GetAllActiveMissions();

                        if (activeMissions.Any(m => m.IsMissionToRestore() || m.Step >= MissionStep.Error))
                        {
                            await this.ScheduleRestore(serviceProvider, bayProvider, activeMissions);
                        }
                        else if (!activeMissions.Any(m => m.Status == MissionStatus.Executing
                                    && m.Step > MissionStep.New)
                                && !this.GenerateHoming(bayProvider, this.machineVolatileDataProvider.IsHomingExecuted)
                                )
                        {
                            if (this.IsLoadUnitMissing(serviceProvider))
                            {
                                this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                            }
                            else
                            {
                                this.machineVolatileDataProvider.Mode = MachineMode.Automatic;
                            }

                            this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
                    }
                    break;

                case MachineMode.SwitchingToLoadUnitOperations:
                    {
                        // in this machine mode we generate homing for elevator and bays, but only if there are no missions to restore.
                        // if homing is not possible we switch anyway to automatic mode
                        var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                        var activeMissions = missionsDataProvider.GetAllActiveMissions();

                        if (activeMissions.Any(m => m.IsMissionToRestore() || m.Step >= MissionStep.Error))
                        {
                            await this.ScheduleRestore(serviceProvider, bayProvider, activeMissions);
                        }
                        else if (!activeMissions.Any(m => m.Status == MissionStatus.Executing && m.Step > MissionStep.New) &&
                                 !this.GenerateHoming(bayProvider, this.machineVolatileDataProvider.IsHomingExecuted))
                        {
                            if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations)
                            {
                                this.machineVolatileDataProvider.Mode = MachineMode.LoadUnitOperations;
                            }
                            else
                            {
                                this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                            }

                            this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
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
                                    await this.ScheduleMissionsOnBayAsync(bay.Number, serviceProvider);
                                }
                                else
                                {
                                    this.Logger.LogWarning("Scheduling missions on bay {number} is not allowed: bay is not active.", bay.Number);
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
                        var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();

                        if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status != MissionStatus.New)
                            && !this.GenerateHoming(bayProvider, this.machineVolatileDataProvider.IsHomingExecuted))
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Compact;
                            this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
                    }
                    break;

                case MachineMode.Compact:
                    {
                        this.ScheduleCompactingMissions(serviceProvider);
                    }
                    break;

                case MachineMode.SwitchingToManual:
                    {
                        var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                        if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status == MissionStatus.Executing
                            && m.Step > MissionStep.New)
                            )
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                            this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
                    }
                    break;

                default:
                    {
                        this.Logger.LogDebug("Cannot perform mission scheduling, because machine is not in automatic mode.");
                    }
                    break;
            }
        }

        private bool IsLoadUnitMissing(IServiceProvider serviceProvider)
        {
            var sensorProvider = serviceProvider.GetRequiredService<ISensorsProvider>();
            var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();
            if (sensorProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
            {
                var loadUnit = elevatorDataProvider.GetLoadingUnitOnBoard();
                if (loadUnit is null)
                {
                    var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
                    errorsProvider.RecordNew(MachineErrorCode.LoadUnitMissingOnElevator);
                    return true;
                }
                if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitId == loadUnit.Id))
                {
                    moveLoadingUnitProvider.InsertToCell(MissionType.Manual, LoadingUnitLocation.Elevator, null, loadUnit.Id, BayNumber.BayOne, MessageActor.AutomationService);
                    return true;
                }
            }
            var bayProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
            var bays = bayProvider.GetAll();
            foreach (var bay in bays)
            {
                foreach (var position in bay.Positions)
                {
                    if (sensorProvider.IsLoadingUnitInLocation(position.Location))
                    {
                        if (position.LoadingUnit is null)
                        {
                            var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
                            errorsProvider.RecordNew(MachineErrorCode.LoadUnitMissingOnBay);
                            return true;
                        }
                        if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitId == position.LoadingUnit.Id))
                        {
                            moveLoadingUnitProvider.InsertToCell(MissionType.Manual, position.Location, null, position.LoadingUnit.Id, bay.Number, MessageActor.AutomationService);
                            return true;
                        }
                    }
                }
            }
            return false;
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

        private async Task OnBayOperationalStatusChangedAsync(IServiceProvider serviceProvider)
        {
            await this.InvokeSchedulerAsync(serviceProvider);
        }

        private async Task OnDataLayerReadyAsync(IServiceProvider serviceProvider)
        {
            GetPersistedMissions(serviceProvider, this.EventAggregator);
            this.dataLayerIsReady = true;
            await this.InvokeSchedulerAsync(serviceProvider);
        }

        private async Task OnHoming(NotificationMessage message, IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status != MissionStatus.Waiting && m.Status != MissionStatus.New))
            {
                if (message.Status == MessageStatus.OperationEnd
                    && message.Data is IHomingMessageData data
                    )
                {
                    this.machineVolatileDataProvider.IsHomingActive = false;
                    if (data.AxisToCalibrate == Axis.BayChain)
                    {
                        this.machineVolatileDataProvider.IsBayHomingExecuted[message.RequestingBay] = true;
                    }
                    else
                    {
                        this.machineVolatileDataProvider.IsHomingExecuted = true;
                    }
                    await this.InvokeSchedulerAsync(serviceProvider);
                }
                else if (message.Status == MessageStatus.OperationError)
                {
                    this.machineVolatileDataProvider.IsHomingActive = false;
                    if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic)
                    {
                        this.machineVolatileDataProvider.Mode = MachineMode.Automatic;
                        this.Logger.LogInformation($"Automation Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    }
                }
            }
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
                if ((mission.LoadUnitDestination == LoadingUnitLocation.Elevator
                        || mission.LoadUnitDestination == LoadingUnitLocation.Cell
                        )
                    //&& !mission.WmsId.HasValue
                    )
                {
                    // load unit to elevator or to cell
                    if (mission.LoadUnitSource != LoadingUnitLocation.Cell
                        && mission.LoadUnitSource != LoadingUnitLocation.Elevator
                        )
                    {
                        // load from bay
                        var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                        var bay = baysDataProvider.GetByLoadingUnitLocation(luData.Source);
                        if ((bay.CurrentMission?.Id ?? 0) == mission.Id)
                        {
                            baysDataProvider.ClearMission(bay.Number);
                        }
                    }
                    missionsDataProvider.Complete(mission.Id);
                    this.NotifyAssignedMissionOperationChanged(mission.TargetBay, null, null);
                }
                else if (mission.LoadUnitDestination != LoadingUnitLocation.Cell
                    && mission.LoadUnitDestination != LoadingUnitLocation.Elevator
                    && mission.Status != MissionStatus.Waiting
                    )
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
                        this.NotifyAssignedMissionOperationChanged(mission.TargetBay, null, null);
                    }
                }
                else if (mission.Status != MissionStatus.Waiting)
                // any other mission type
                {
                    missionsDataProvider.Complete(mission.Id);
                    this.NotifyAssignedMissionOperationChanged(mission.TargetBay, null, null);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Failed to process mission: {ex.Message}");
            }

            await this.InvokeSchedulerAsync(serviceProvider);
        }

        private async Task OnMachineModeChangedAsync(IServiceProvider serviceProvider)
        {
            await this.InvokeSchedulerAsync(serviceProvider);
        }

        private async Task OnNewMachineMissionAvailableAsync(IServiceProvider serviceProvider)
        {
            await this.InvokeSchedulerAsync(serviceProvider);
        }

        private async Task OnOperationComplete(MissionOperationCompletedMessageData messageData, IServiceProvider serviceProvider)
        {
            Contract.Requires(messageData != null);

            if (!this.dataLayerIsReady)
            {
                this.Logger.LogTrace("Cannot perform mission scheduling, because data layer is not ready.");
                return;
            }

            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

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
                //baysDataProvider.ClearMission(bay.Number);

                await this.InvokeSchedulerAsync(serviceProvider);
            }
        }

        private async Task ScheduleRestore(IServiceProvider serviceProvider, IBaysDataProvider bayProvider, IEnumerable<Mission> activeMissions)
        {
            if (!activeMissions.Any(m => m.Step >= MissionStep.Error && m.RestoreStep == MissionStep.NotDefined))
            {
                foreach (var bay in bayProvider.GetAll())
                {
                    try
                    {
                        if (bay.IsActive)
                        {
                            await this.ScheduleMissionsOnBayAsync(bay.Number, serviceProvider, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Failed to schedule missions on bay {number}.", bay.Number);
                    }
                }
            }
        }

        #endregion
    }
}
