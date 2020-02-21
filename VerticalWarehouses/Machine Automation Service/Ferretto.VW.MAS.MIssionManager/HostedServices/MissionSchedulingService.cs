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

        private bool dataLayerIsReady;

        #endregion

        #region Constructors

        public MissionSchedulingService(
            IConfiguration configuration,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
        }

        #endregion

        #region Methods

        public static bool ScheduleCompactingMissions(IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();

            if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status != MissionStatus.New && m.Status != MissionStatus.Waiting))
            {
                return serviceProvider.GetRequiredService<IMissionSchedulingProvider>().QueueLoadingUnitCompactingMission(serviceProvider);
            }
            // no more compacting is possible. Exit from compact mode
            return false;
        }

        public void CompleteCurrentMissionInBay(BayNumber bayNumber, Mission mission, IServiceProvider serviceProvider)
        {
            var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

            baysDataProvider.ClearMission(bayNumber);
            this.NotifyAssignedMissionChanged(bayNumber, null);

            // check if there are other missions for this LU in this bay
            var nextMission = missionsDataProvider
                .GetAllActiveMissionsByBay(bayNumber)
                .FirstOrDefault(m =>
                    m.LoadUnitId == mission.LoadUnitId
                    &&
                    m.WmsId.HasValue
                    &&
                    m.WmsId != mission.WmsId);

            if (nextMission is null)
            {
                var loadingUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(mission.LoadUnitId);

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
                this.Logger.LogInformation("Bay {bayNumber}: mission {missionId} Stop.", mission.TargetBay, mission.Id);
                moveLoadingUnitProvider.StopMove(mission.Id, bayNumber, bayNumber, MessageActor.MissionManager);

                // activate new mission
                this.Logger.LogInformation("Bay {bayNumber}: mission {missionId} Activate.", mission.TargetBay, nextMission.Id);
                moveLoadingUnitProvider.ActivateMove(nextMission.Id, nextMission.MissionType, nextMission.LoadUnitId, bayNumber, MessageActor.MissionManager);
            }
        }

        public bool ScheduleFirstTestMissions(IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var machinemachineVolatileDataProvider = serviceProvider.GetRequiredService<IMachineVolatileDataProvider>();
            var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();

            var loadUnitId = machinemachineVolatileDataProvider.LoadUnitsToTest?.FirstOrDefault();
            if (!loadUnitId.HasValue)
            {
                this.Logger.LogError($"First Test error: Load Unit not defined!");
                errorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, machinemachineVolatileDataProvider.BayTestNumber);
                return false;
            }
            var cellsProvider = serviceProvider.GetRequiredService<ICellsProvider>();
            if (machinemachineVolatileDataProvider.CyclesTested == 0)
            {
                var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                var loadUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(loadUnitId.Value);
                if (loadUnitSource != LoadingUnitLocation.NoLocation)
                {
                    this.Logger.LogError($"First Test error: Load Unit not found in Bay!");
                    errorsProvider.RecordNew(MachineErrorCode.LoadUnitNotFound, machinemachineVolatileDataProvider.BayTestNumber);
                    return false;
                }
                cellsProvider.SetCellsToTest();
                this.Logger.LogInformation($"First test started for Load Unit {loadUnitId.Value} on Bay {machinemachineVolatileDataProvider.BayTestNumber}");
            }

            var returnValue = false;
            if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status != MissionStatus.New && m.Status != MissionStatus.Waiting))
            {
                var missionSchedulingProvider = serviceProvider.GetRequiredService<IMissionSchedulingProvider>();
                returnValue = missionSchedulingProvider.QueueFirstTestMission(loadUnitId.Value, machinemachineVolatileDataProvider.BayTestNumber, machinemachineVolatileDataProvider.CyclesTested, serviceProvider);
            }
            if (!returnValue)
            {
                // testing is finished! Exit from FirstTest mode
                if (cellsProvider.IsCellToTest())
                {
                    this.Logger.LogError($"First Test error for Load Unit {loadUnitId} on Bay {machinemachineVolatileDataProvider.BayTestNumber}: Not all cells are tested!");
                    errorsProvider.RecordNew(MachineErrorCode.FirstTestFailed, machinemachineVolatileDataProvider.BayTestNumber);
                }
                else
                {
                    this.Logger.LogInformation($"First test finished successfully for Load Unit {loadUnitId} on Bay {machinemachineVolatileDataProvider.BayTestNumber}");
                }
                var setupProceduresDataProvider = serviceProvider.GetRequiredService<ISetupProceduresDataProvider>();
                setupProceduresDataProvider.MarkAsCompleted(setupProceduresDataProvider.GetBayFirstLoadingUnit(machinemachineVolatileDataProvider.BayTestNumber));
            }
            else
            {
                machinemachineVolatileDataProvider.CyclesTested++;
            }
            return returnValue;
        }

        public async Task ScheduleMissionsOnBayAsync(BayNumber bayNumber, IServiceProvider serviceProvider, bool restore = false)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();

            var activeMissions = missionsDataProvider.GetAllActiveMissionsByBay(bayNumber);

            var missionToRestore = activeMissions
                .OrderBy(o => o.Status)
                .ThenBy(t => t.RestoreStep)
                .FirstOrDefault(x => (x.Status == MissionStatus.Executing || x.Status == MissionStatus.Waiting)
                    && x.IsMissionToRestore());
            if (missionToRestore != null)
            {
                if (restore)
                {
                    this.Logger.LogInformation("Bay {bayNumber}: mission {missionId} Restore.", missionToRestore.TargetBay, missionToRestore.Id);
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

            var missions = activeMissions
                .Where(m =>
                    (
                        m.Status is MissionStatus.New
                        &&
                        (m.MissionType is MissionType.IN
                        ||
                        m.MissionType is MissionType.OUT
                        ||
                        m.MissionType is MissionType.WMS)
                    )
                    ||
                    m.Status is MissionStatus.Executing
                    ||
                    m.Status is MissionStatus.Waiting)
                .OrderBy(o => o.Status);

            if (!missions.Any())
            {
                // no more missions are available for scheduling on this bay
                this.NotifyAssignedMissionChanged(bayNumber, null);
                return;
            }

            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

            foreach (var mission in missions)
            {
                if (mission.WmsId.HasValue)
                {
                    await this.ScheduleWmsMissionAsync(bayNumber, serviceProvider, activeMissions, mission);
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
                            this.Logger.LogInformation("Bay {bayNumber}: mission {missionId} Activate new.", mission.TargetBay, mission.Id);
                            moveLoadingUnitProvider.ActivateMove(mission.Id, mission.MissionType, mission.LoadUnitId, bayNumber, MessageActor.MissionManager);
                        }
                    }
                    else if (mission.MissionType is MissionType.IN)
                    {
                        this.Logger.LogInformation("Bay {bayNumber}: mission {missionId} Activate IN.", mission.TargetBay, mission.Id);
                        moveLoadingUnitProvider.ActivateMoveToCell(
                            mission.Id,
                            mission.MissionType,
                            mission.LoadUnitId,
                            bayNumber,
                            MessageActor.MissionManager);
                    }
                }
                else if (
                    mission.Status is MissionStatus.Waiting
                    &&
                    mission.Step is MissionStep.BayChain)
                {
                    var loadingUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(mission.LoadUnitId);

                    this.Logger.LogInformation("Bay {bayNumber}: mission {missionId} Resume waiting.", mission.TargetBay, mission.Id);
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
            var bayProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

            var missions = missionsDataProvider.GetAllMissions().ToList();
            foreach (var mission in missions)
            {
                if (!mission.IsRestoringType())
                {
                    if (bayProvider.IsMissionInBay(mission))
                    {
                        bayProvider.ClearMission(mission.TargetBay);
                    }
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
            var generated = false;
            if (this.machineVolatileDataProvider.IsBayHomingExecuted.Any(x => !x.Value)
                &&
                bays.All(x => x.CurrentMission == null))
            {
                var bayNumber = bays.FirstOrDefault(x =>
                this.machineVolatileDataProvider.IsBayHomingExecuted.ContainsKey(x.Number)
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
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (!this.dataLayerIsReady)
            {
                this.Logger.LogTrace("Mission scheduling is not allowed: data layer is not ready.");
                return;
            }

            var sensorsProvider = serviceProvider.GetRequiredService<ISensorsProvider>();
            if (!sensorsProvider.IsMachineSecurityRunning)
            {
                this.Logger.LogWarning("Mission scheduling is not allowed: machine is not in running state.");
                return;
            }

            var bayProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

            switch (this.machineVolatileDataProvider.Mode)
            {
                case MachineMode.SwitchingToAutomatic:
                case MachineMode.SwitchingToLoadUnitOperations:
                case MachineMode.SwitchingToCompact:
                case MachineMode.SwitchingToFirstTest:
                    {
                        // in this machine mode we generate homing for elevator and bays, but only if there are no missions to restore.
                        // if homing is not possible we switch anyway to automatic mode
                        var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                        var activeMissions = missionsDataProvider.GetAllActiveMissions();

                        if (activeMissions.Any(m => m.IsMissionToRestore() || m.Step >= MissionStep.Error))
                        {
                            if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic)
                            {
                                if (activeMissions.Any(m => m.MissionType == MissionType.Compact))
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToCompact;
                                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                }
                                else if (activeMissions.Any(m => m.MissionType == MissionType.FirstTest))
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFirstTest;
                                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                }
                                else if (activeMissions.Any(m => m.MissionType == MissionType.LoadUnitOperation))
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToLoadUnitOperations;
                                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                }
                            }
                            await this.ScheduleRestore(serviceProvider, bayProvider, activeMissions);
                        }
                        else if (!activeMissions.Any(m => m.Status == MissionStatus.Executing
                                    && m.Step > MissionStep.New)
                                && !this.GenerateHoming(bayProvider, this.machineVolatileDataProvider.IsHomingExecuted)
                                )
                        {
                            if (!this.IsLoadUnitMissing(serviceProvider))
                            {
                                if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToCompact
                                    || activeMissions.Any(m => m.MissionType == MissionType.Compact && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.Compact;
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToFirstTest
                                    || activeMissions.Any(m => m.MissionType == MissionType.FirstTest && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.FirstTest;
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations
                                    || activeMissions.Any(m => m.MissionType == MissionType.LoadUnitOperation && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.LoadUnitOperations;
                                }
                                else
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.Automatic;
                                }

                                this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                            }
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

                case MachineMode.Compact:
                    {
                        if (!ScheduleCompactingMissions(serviceProvider))
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                            this.Logger.LogInformation($"Compacting terminated. Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
                    }
                    break;

                case MachineMode.FirstTest:
                    {
                        if (!this.ScheduleFirstTestMissions(serviceProvider))
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                            this.Logger.LogInformation($"First test terminated. Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
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
                        this.Logger.LogDebug("Mission scheduling is not allowed: machine is not in automatic mode.");
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

                    this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    return true;
                }

                if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitId == loadUnit.Id))
                {
                    this.Logger.LogInformation($"Insert load unit {loadUnit.Id} from {LoadingUnitLocation.Elevator} to cell");
                    var missionType = (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic) ? MissionType.IN : MissionType.LoadUnitOperation;
                    moveLoadingUnitProvider.InsertToCell(missionType, LoadingUnitLocation.Elevator, null, loadUnit.Id, BayNumber.BayOne, MessageActor.AutomationService);
                    return true;
                }
            }

            if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic)
            {
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

                                this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                                this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                return true;
                            }

                            if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitId == position.LoadingUnit.Id))
                            {
                                this.Logger.LogInformation($"Insert load unit {position.LoadingUnit.Id} from {position.Location} to cell");
                                var missionType = (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic) ? MissionType.IN : MissionType.LoadUnitOperation;
                                moveLoadingUnitProvider.InsertToCell(missionType, position.Location, null, position.LoadingUnit.Id, bay.Number, MessageActor.AutomationService);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void NotifyAssignedMissionChanged(
            BayNumber bayNumber,
            int? missionId)
        {
            var data = new AssignedMissionChangedMessageData
            {
                BayNumber = bayNumber,
                MissionId = missionId,
            };

            var notificationMessage = new NotificationMessage(
                data,
                $"Mission assigned to bay {bayNumber} has changed.",
                MessageActor.WebApi,
                MessageActor.MachineManager,
                MessageType.AssignedMissionChanged,
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
            var loadUnitsDataProvider = serviceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
            loadUnitsDataProvider.UpdateWeightStatistics();
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
                    else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations)
                    {
                        this.machineVolatileDataProvider.Mode = MachineMode.LoadUnitOperations;
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
            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

            try
            {
                var mission = missionsDataProvider.GetById(luData.MissionId.Value);
                if (mission.LoadUnitDestination is LoadingUnitLocation.Elevator
                    ||
                    mission.LoadUnitDestination is LoadingUnitLocation.Cell)
                // load unit to elevator or to cell
                {
                    if (mission.LoadUnitSource != LoadingUnitLocation.Cell
                        &&
                        mission.LoadUnitSource != LoadingUnitLocation.Elevator)
                    // load from bay
                    {
                        var bay = baysDataProvider.GetByLoadingUnitLocation(luData.Source);
                        if ((bay.CurrentMission?.Id ?? 0) == mission.Id)
                        {
                            baysDataProvider.ClearMission(bay.Number);
                        }
                    }

                    missionsDataProvider.Complete(mission.Id);
                    this.NotifyAssignedMissionChanged(mission.TargetBay, null);
                }
                else if (mission.LoadUnitDestination != LoadingUnitLocation.Cell
                    &&
                    mission.LoadUnitDestination != LoadingUnitLocation.Elevator
                    &&
                    mission.Status != MissionStatus.Waiting)
                // loading unit to bay mission
                {
                    baysDataProvider.AssignMission(mission.TargetBay, mission);
                    this.NotifyAssignedMissionChanged(mission.TargetBay, mission.WmsId);
                }
                else if (mission.Status != MissionStatus.Waiting)
                // any other mission type
                {
                    missionsDataProvider.Complete(mission.Id);
                    this.NotifyAssignedMissionChanged(mission.TargetBay, null);
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
            if (this.dataLayerIsReady)
            {
                await this.InvokeSchedulerAsync(serviceProvider);
            }

            this.Logger.LogTrace("Cannot perform mission scheduling, because data layer is not ready.");
        }

        private async Task ScheduleRestore(IServiceProvider serviceProvider, IBaysDataProvider bayProvider, IEnumerable<Mission> activeMissions)
        {
            if (!activeMissions.Any(m => m.Step >= MissionStep.Error && m.RestoreStep == MissionStep.NotDefined))
            {
                foreach (var bay in bayProvider.GetAll().Where(b => b.IsActive))
                {
                    try
                    {
                        await this.ScheduleMissionsOnBayAsync(bay.Number, serviceProvider, true);
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Failed to schedule missions on bay {number}.", bay.Number);
                    }
                }
            }
        }

        private async Task ScheduleWmsMissionAsync(BayNumber bayNumber, IServiceProvider serviceProvider, IEnumerable<Mission> activeMissions, Mission mission)
        {
            System.Diagnostics.Debug.Assert(mission.WmsId.HasValue);

            if (!this.configuration.IsWmsEnabled())
            {
                this.Logger.LogTrace("Skipping scheduling of WMS mission {wmsId}, because WMS is not enabled.", mission.WmsId);
                return;
            }

            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var missionsWmsWebService = serviceProvider.GetRequiredService<WMS.Data.WebAPI.Contracts.IMissionsWmsWebService>();

            var wmsMission = await missionsWmsWebService.GetByIdAsync(mission.WmsId.Value);
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
                        this.Logger.LogDebug($"Bay {bayNumber}: WMS mission {mission.WmsId} cannot start because LoadUnit {mission.LoadUnitId} is not in a cell.");
                    }
                    else
                    {
                        this.Logger.LogInformation("Bay {bayNumber}: WMS mission {missionId} Activate.", mission.TargetBay, mission.WmsId.Value);
                        serviceProvider
                            .GetRequiredService<IMoveLoadUnitProvider>()
                            .ActivateMove(mission.Id, mission.MissionType, mission.LoadUnitId, bayNumber, MessageActor.MissionManager);
                    }
                }
                else if (mission.Status is MissionStatus.Waiting)
                {
                    var newOperation = newOperations.OrderBy(o => o.Priority).First();
                    this.Logger.LogInformation("Bay {bayNumber}: WMS mission {missionId} has operation {operationId} to execute.", mission.TargetBay, mission.WmsId.Value, newOperation.Id);

                    serviceProvider
                        .GetRequiredService<IBaysDataProvider>()
                        .AssignMission(mission.TargetBay, mission);

                    this.NotifyAssignedMissionChanged(mission.TargetBay, wmsMission.Id);
                }
            }
            else if (mission.Status is MissionStatus.Executing || mission.Status is MissionStatus.Waiting)
            {
                // wms mission is finished => schedule loading unit movement back to cell
                mission.Status = MissionStatus.Completing;
                missionsDataProvider.Update(mission);

                this.Logger.LogInformation("Bay {bayNumber}: WMS mission {missionId} completed.", bayNumber, mission.WmsId.Value);

                //    this.CompleteCurrentMissionInBay(bayNumber, mission, serviceProvider);
            }
        }

        #endregion
    }
}
