﻿using System;
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
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.MissionMove;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class MissionSchedulingService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private bool dataLayerIsReady;

        #endregion

        #region Constructors

        public MissionSchedulingService(
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
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
                var missionSchedulingProvider = serviceProvider.GetRequiredService<IMissionSchedulingProvider>();

                // send back the loading unit to the cell
                this.Logger.LogInformation("Bay {bayNumber}: mission {missionId} WmsId {wmsId} back to cell.", mission.TargetBay, mission.Id, mission.WmsId);
                missionSchedulingProvider.QueueRecallMission(mission.LoadUnitId, bayNumber, MissionType.IN);
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

        // return false when test is finished
        public bool ScheduleFirstTestMissions(IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var machineProvider = serviceProvider.GetRequiredService<IMachineVolatileDataProvider>();
            var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();

            var loadUnitId = machineProvider.LoadUnitsToTest?.FirstOrDefault();
            if (!loadUnitId.HasValue)
            {
                this.Logger.LogError($"First Test error: Load Unit not defined!");
                errorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, machineProvider.BayTestNumber);
                return false;
            }
            var cellsProvider = serviceProvider.GetRequiredService<ICellsProvider>();
            if (machineProvider.ExecutedCycles == 0)
            {
                // first cycle: init RequiredCycles and cells to test
                var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                var loadUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(loadUnitId.Value);
                if (loadUnitSource != LoadingUnitLocation.NoLocation)
                {
                    var bayNumber = baysDataProvider.GetByLoadingUnitLocation(loadUnitSource)?.Number ?? BayNumber.None;
                    if (bayNumber != machineProvider.BayTestNumber)
                    {
                        this.Logger.LogError($"First Test error: Load Unit not found in Bay!");
                        errorsProvider.RecordNew(MachineErrorCode.LoadUnitNotFound, machineProvider.BayTestNumber);
                        return false;
                    }
                }

                var loadingUnitsDataProvider = serviceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
                if (loadingUnitsDataProvider.CountIntoMachine() > 0)
                {
                    this.Logger.LogError($"First Test error: warehouse is not empty!");
                    errorsProvider.RecordNew(MachineErrorCode.WarehouseNotEmpty, machineProvider.BayTestNumber);
                    return false;
                }
                machineProvider.RequiredCycles = cellsProvider.SetCellsToTest();
                if (machineProvider.RequiredCycles.Value == 0)
                {
                    this.Logger.LogError($"First Test error: no cell to test found!");
                    errorsProvider.RecordNew(MachineErrorCode.FirstTestFailed, machineProvider.BayTestNumber);
                    return false;
                }
                this.Logger.LogInformation($"First test started for Load Unit {loadUnitId.Value} on Bay {machineProvider.BayTestNumber}");
            }

            var returnValue = false;
            if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status != MissionStatus.New && m.Status != MissionStatus.Waiting)
                && machineProvider.ExecutedCycles < machineProvider.RequiredCycles.Value
                )
            {
                var missionSchedulingProvider = serviceProvider.GetRequiredService<IMissionSchedulingProvider>();
                returnValue = missionSchedulingProvider.QueueFirstTestMission(loadUnitId.Value, machineProvider.BayTestNumber, machineProvider.ExecutedCycles, serviceProvider);
            }

            var setupProceduresDataProvider = serviceProvider.GetRequiredService<ISetupProceduresDataProvider>();
            var setupRecord = setupProceduresDataProvider.GetLoadFirstDrawerTest();

            var messageStatus = MessageStatus.OperationExecuting;
            if (!returnValue)
            {
                // testing is finished! Exit from FirstTest mode
                if (machineProvider.ExecutedCycles < machineProvider.RequiredCycles.Value)
                {
                    this.Logger.LogError($"First Test error for Load Unit {loadUnitId} on Bay {machineProvider.BayTestNumber}: Not all cells are tested!");
                    errorsProvider.RecordNew(MachineErrorCode.FirstTestFailed, machineProvider.BayTestNumber);
                    messageStatus = MessageStatus.OperationError;
                }
                else
                {
                    this.Logger.LogInformation($"First test finished successfully for Load Unit {loadUnitId} on Bay {machineProvider.BayTestNumber}");
                    messageStatus = MessageStatus.OperationEnd;
                }
                setupProceduresDataProvider.MarkAsCompleted(setupRecord);
            }
            else
            {
                machineProvider.ExecutedCycles++;
                setupProceduresDataProvider.InProgressProcedure(setupRecord);
            }

            var notificationMessage = new NotificationMessage(
                new MoveTestMessageData(machineProvider.ExecutedCycles,
                    machineProvider.RequiredCycles.Value,
                    machineProvider.LoadUnitsToTest),
                $"First Load Unit Test result",
                MessageActor.Any,
                MessageActor.MissionManager,
                MessageType.MoveTest,
                machineProvider.BayTestNumber,
                machineProvider.BayTestNumber,
                messageStatus);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(notificationMessage);

            return returnValue;
        }

        // return false when test is finished
        public bool ScheduleFullTestMissions(IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var machineProvider = serviceProvider.GetRequiredService<IMachineVolatileDataProvider>();
            var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
            var missionSchedulingProvider = serviceProvider.GetRequiredService<IMissionSchedulingProvider>();

            var loadUnitId = machineProvider.LoadUnitsToTest?.FirstOrDefault();
            if (!loadUnitId.HasValue)
            {
                this.Logger.LogError($"Full Test error: Load Units not defined!");
                errorsProvider.RecordNew(MachineErrorCode.LoadUnitNotFound, machineProvider.BayTestNumber);
                return false;
            }

            // search the next load unit to be tested
            var missions = missionsDataProvider.GetAllActiveMissions();
            var nextLoadUnits = machineProvider.LoadUnitsExecutedCycles.Where(x => x.Value < machineProvider.RequiredCycles.Value);
            if (nextLoadUnits.Any())
            {
                // get the LU with lowest executed cycles value that has no active missions
                loadUnitId = nextLoadUnits
                    .OrderBy(o => o.Value)
                    .FirstOrDefault(lu => !missions.Any(m => m.LoadUnitId == lu.Key)).Key;
            }
            else
            {
                loadUnitId = null;
                // no more load unit to call. Just wait all missions to finish
            }
            if (loadUnitId != null
                && loadUnitId != 0
                && !missions.Any(m => m.Status == MissionStatus.New)
                && machineProvider.ExecutedCycles < machineProvider.RequiredCycles.Value
                )
            {
                missionSchedulingProvider.QueueBayMission(loadUnitId.Value, machineProvider.BayTestNumber, MissionType.FullTestOUT);
                machineProvider.ExecutedCycles = machineProvider.LoadUnitsExecutedCycles[loadUnitId.Value];
                machineProvider.LoadUnitsExecutedCycles[loadUnitId.Value]++;
            }

            // the mission scheduler
            var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();
            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
            foreach (var mission in missions)
            {
                if (mission.Status is MissionStatus.New)
                {
                    var cellsProvider = serviceProvider.GetRequiredService<ICellsProvider>();
                    if (mission.MissionType is MissionType.FullTestOUT)
                    {
                        var sourceCell = cellsProvider.GetByLoadingUnitId(mission.LoadUnitId);
                        if (sourceCell is null)
                        {
                            this.Logger.LogDebug($"Bay {machineProvider.BayTestNumber}: loading unit  {mission.LoadUnitId} cannot be moved to bay because it is not located in a cell.");
                        }
                        else
                        {
                            this.Logger.LogInformation("Bay {bayNumber}: mission {missionId} Activate new.", mission.TargetBay, mission.Id);
                            moveLoadingUnitProvider.ActivateMove(mission.Id, mission.MissionType, mission.LoadUnitId, machineProvider.BayTestNumber, MessageActor.MissionManager);
                        }
                    }
                    else if (mission.MissionType is MissionType.FullTestIN)
                    {
                        this.Logger.LogInformation("Bay {bayNumber}: mission {missionId} Activate IN.", mission.TargetBay, mission.Id);
                        moveLoadingUnitProvider.ActivateMoveToCell(
                            mission.Id,
                            mission.MissionType,
                            mission.LoadUnitId,
                            machineProvider.BayTestNumber,
                            MessageActor.MissionManager);
                    }
                }
                else if (
                    (mission.Status is MissionStatus.Waiting && mission.Step is MissionStep.BayChain)
                    || (mission.Status is MissionStatus.Executing && mission.Step is MissionStep.WaitDeposit)
                    )
                {
                    var loadingUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(mission.LoadUnitId);

                    this.Logger.LogInformation("Bay {bayNumber}: mission {missionId} Resume waiting.", mission.TargetBay, mission.Id);
                    moveLoadingUnitProvider.ResumeMoveLoadUnit(
                        mission.Id,
                        loadingUnitSource,
                        LoadingUnitLocation.Cell,
                        machineProvider.BayTestNumber,
                        null,
                        mission.MissionType,
                        MessageActor.MissionManager);
                }
                else if (mission.Status is MissionStatus.Waiting
                    && mission.Step is MissionStep.WaitPick
                    )
                {
                    try
                    {
                        missionSchedulingProvider.QueueRecallMission(mission.LoadUnitId, machineProvider.BayTestNumber, MissionType.FullTestIN);
                        this.Logger.LogInformation($"Move load unit {mission.LoadUnitId} back from bay {machineProvider.BayTestNumber}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        this.Logger.LogTrace(ex.Message);
                    }
                }
            }

            var setupProceduresDataProvider = serviceProvider.GetRequiredService<ISetupProceduresDataProvider>();
            var setupRecord = setupProceduresDataProvider.GetFullTest();

            var returnValue = false;
            var messageStatus = MessageStatus.OperationExecuting;
            if (loadUnitId is null
                && !missions.Any()
                )
            {
                // testing is finished! Exit from FullTest mode
                machineProvider.ExecutedCycles = machineProvider.LoadUnitsExecutedCycles.Last().Value;
                if (machineProvider.ExecutedCycles < machineProvider.RequiredCycles.Value)
                {
                    this.Logger.LogError($"Full Test error for {machineProvider.LoadUnitsToTest.Count} Load Units on Bay {machineProvider.BayTestNumber}");
                    errorsProvider.RecordNew(MachineErrorCode.FirstTestFailed, machineProvider.BayTestNumber);
                    messageStatus = MessageStatus.OperationError;
                }
                else
                {
                    this.Logger.LogInformation($"Full test finished successfully for {machineProvider.LoadUnitsToTest.Count} Load Units on Bay {machineProvider.BayTestNumber}");
                    messageStatus = MessageStatus.OperationEnd;
                }
                setupProceduresDataProvider.MarkAsCompleted(setupRecord);
            }
            else
            {
                returnValue = true;
                setupProceduresDataProvider.InProgressProcedure(setupRecord);
            }

            var notificationMessage = new NotificationMessage(
                new MoveTestMessageData(machineProvider.ExecutedCycles,
                    machineProvider.RequiredCycles.Value,
                    machineProvider.LoadUnitsToTest),
                $"Full Load Unit Test result",
                MessageActor.Any,
                MessageActor.MissionManager,
                MessageType.MoveTest,
                machineProvider.BayTestNumber,
                machineProvider.BayTestNumber,
                messageStatus);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(notificationMessage);

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
                        m.MissionType is MissionType.WMS
                        ||
                        m.MissionType is MissionType.FullTestIN
                        ||
                        m.MissionType is MissionType.FullTestOUT)
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
                    if (mission.MissionType is MissionType.OUT
                        || mission.MissionType is MissionType.FullTestOUT
                        )
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
                    else if (mission.MissionType is MissionType.IN
                        || mission.MissionType is MissionType.FullTestIN
                        )
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
                    (mission.Status is MissionStatus.Waiting && mission.Step is MissionStep.BayChain)
                    || (mission.Status is MissionStatus.Executing && mission.Step is MissionStep.WaitDeposit)
                    )
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

            missionsDataProvider.PurgeWmsMissions();

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

        private bool GenerateHoming(IBaysDataProvider bayProvider)
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

            if (!generated && !this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay])
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
                case MachineMode.SwitchingToFullTest:
                    {
                        // in this machine mode we generate homing for elevator and bays, but only if there are no missions to restore.
                        // if homing is not possible we switch anyway to automatic mode
                        var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                        var activeMissions = missionsDataProvider.GetAllActiveMissions();

                        if (activeMissions.Any(m => m.IsMissionToRestore() || m.Step >= MissionStep.Error || m.Status == MissionStatus.New))
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
                                else if (activeMissions.Any(m => m.MissionType == MissionType.FullTestIN || m.MissionType == MissionType.FullTestOUT))
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFullTest;
                                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                }
                                else if (activeMissions.Any(m => m.MissionType == MissionType.LoadUnitOperation))
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToLoadUnitOperations;
                                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                }
                            }
                        }
                        if (activeMissions.Any(m => m.IsMissionToRestore() || m.Step >= MissionStep.Error))
                        {
                            await this.ScheduleRestore(serviceProvider, bayProvider, activeMissions);
                        }
                        else if (!activeMissions.Any(m => m.Status == MissionStatus.Executing
                                && m.Step > MissionStep.New)
                            && !this.GenerateHoming(bayProvider)
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
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToFullTest
                                    || activeMissions.Any(m => (m.MissionType == MissionType.FullTestIN || m.MissionType == MissionType.FullTestOUT)
                                        && m.Status == MissionStatus.Executing
                                        )
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.FullTest;
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

                case MachineMode.FullTest:
                    {
                        if (!this.ScheduleFullTestMissions(serviceProvider))
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                            this.Logger.LogInformation($"Full test terminated. Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
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
            var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
            if (sensorProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
            {
                var loadUnit = elevatorDataProvider.GetLoadingUnitOnBoard();
                if (loadUnit is null)
                {
                    errorsProvider.RecordNew(MachineErrorCode.LoadUnitMissingOnElevator);

                    this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    return true;
                }

                if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitId == loadUnit.Id))
                {
                    if (loadUnit.Height == 0)
                    {
                        var loadingUnitProvider = serviceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
                        var machineProvider = serviceProvider.GetRequiredService<IMachineProvider>();
                        var machine = machineProvider.Get();
                        loadingUnitProvider.SetHeight(loadUnit.Id, machine.LoadUnitMaxHeight);
                    }
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
                    foreach (var position in bay.Positions.OrderBy(b => b.Location))
                    {
                        if (sensorProvider.IsLoadingUnitInLocation(position.Location))
                        {
                            if (position.LoadingUnit is null)
                            {
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
            errorsProvider.ResolveAll();
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
            this.Logger.LogTrace("OnDataLayerReady start");
            var loadUnitsDataProvider = serviceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
            loadUnitsDataProvider.UpdateWeightStatistics();
            GetPersistedMissions(serviceProvider, this.EventAggregator);
            this.RestoreFullTest(serviceProvider);
            this.dataLayerIsReady = true;
            await this.InvokeSchedulerAsync(serviceProvider);
            this.Logger.LogTrace("OnDataLayerReady end");
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
                        this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay] = true;
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

        private void RestoreFullTest(IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var mission = missionsDataProvider.GetAllActiveMissions().FirstOrDefault(m => m.MissionType == MissionType.FullTestIN || m.MissionType == MissionType.FullTestOUT);
            if (mission != null)
            {
                // TODO - restore values from saved procedure??
                var loadUnitsProvider = serviceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
                var loadUnits = loadUnitsProvider.GetAll().Where(l => l.Status != LoadingUnitStatus.Undefined).Select(x => x.Id).ToList();
                this.machineVolatileDataProvider.LoadUnitsToTest = loadUnits;
                this.machineVolatileDataProvider.RequiredCycles = 1;
                this.machineVolatileDataProvider.BayTestNumber = mission.TargetBay;
                this.machineVolatileDataProvider.ExecutedCycles = 0;
                this.machineVolatileDataProvider.LoadUnitsExecutedCycles = loadUnits.ToDictionary(key => key, value => 0);
            }
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

            if (!serviceProvider.GetRequiredService<IWmsSettingsProvider>().IsEnabled)
            {
                this.Logger.LogTrace("Skipping scheduling of WMS mission {wmsId}, because WMS is not enabled.", mission.WmsId);
                return;
            }

            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var missionsWmsWebService = serviceProvider.GetRequiredService<WMS.Data.WebAPI.Contracts.IMissionsWmsWebService>();
            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

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
                // wms mission is finished
                baysDataProvider.ClearMission(bayNumber);
                mission.Status = MissionStatus.Completed;
                missionsDataProvider.Update(mission);

                this.Logger.LogInformation("Bay {bayNumber}: WMS mission {missionId} completed.", bayNumber, mission.WmsId.Value);

                //    this.CompleteCurrentMissionInBay(bayNumber, mission, serviceProvider);
            }
        }

        #endregion
    }
}
