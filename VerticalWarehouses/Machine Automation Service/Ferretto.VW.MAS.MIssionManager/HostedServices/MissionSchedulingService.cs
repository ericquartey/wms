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
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.MissionMove;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.MachineManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class MissionSchedulingService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IServicingProvider servicingProvider;

        private bool dataLayerIsReady;

        private LoadingUnitLocation loadUnitSource;

        #endregion

        #region Constructors

        public MissionSchedulingService(
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IServicingProvider servicingProvider,
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.servicingProvider = servicingProvider ?? throw new ArgumentNullException(nameof(servicingProvider));
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
                this.loadUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(loadUnitId.Value);
                if (this.loadUnitSource != LoadingUnitLocation.NoLocation)
                {
                    var bayNumber = baysDataProvider.GetByLoadingUnitLocation(this.loadUnitSource)?.Number ?? BayNumber.None;
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
                this.Logger.LogInformation($"First test started for Load Unit {loadUnitId.Value} on Bay {machineProvider.BayTestNumber} for {machineProvider.RequiredCycles} cells");
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
                // testing is finished!
                var loadingUnitsDataProvider = serviceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
                if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status != MissionStatus.New && m.Status != MissionStatus.Waiting)
                    && loadingUnitsDataProvider.CountIntoMachine() > 0
                    )
                {
                    // move the LU in bay
                    try
                    {
                        this.Logger.LogInformation($"Move to bay {machineProvider.BayTestNumber} First test");
                        var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();
                        moveLoadingUnitProvider.EjectFromCell(MissionType.FirstTest, this.loadUnitSource, loadUnitId.Value, machineProvider.BayTestNumber, MessageActor.MissionManager);
                        returnValue = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // no more testing is possible. Exit from test mode
                        //this.logger.LogError(e, e.Message);
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, ex.Message);
                    }
                }
            }

            if (!returnValue)
            {
                // Exit from FirstTest mode
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
            else if (machineProvider.ExecutedCycles < machineProvider.RequiredCycles.Value)
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
                && !machineProvider.StopTest
                )
            {
                try
                {
                    missionSchedulingProvider.QueueBayMission(loadUnitId.Value, machineProvider.BayTestNumber, MissionType.FullTestOUT);
                    machineProvider.ExecutedCycles = machineProvider.LoadUnitsExecutedCycles[loadUnitId.Value];
                    machineProvider.LoadUnitsExecutedCycles[loadUnitId.Value]++;
                }
                catch (InvalidOperationException ex)
                {
                    errorsProvider.RecordNew(MachineErrorCode.LoadUnitNotFound, machineProvider.BayTestNumber, ex.Message);
                }
            }
            else
            {
                loadUnitId = null;
                // no more load unit to call. Just wait all missions to finish
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
                    || (mission.Status is MissionStatus.Executing && mission.Step is MissionStep.WaitDepositCell)
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
                        this.Logger.LogInformation($"Move load unit {mission.LoadUnitId} back from bay {machineProvider.BayTestNumber}");
                        missionSchedulingProvider.QueueRecallMission(mission.LoadUnitId, machineProvider.BayTestNumber, MissionType.FullTestIN);
                    }
                    catch (InvalidOperationException ex)
                    {
                        this.Logger.LogTrace(ex.Message);
                    }
                }
            }

            var setupProceduresDataProvider = serviceProvider.GetRequiredService<ISetupProceduresDataProvider>();
            var setupRecord = setupProceduresDataProvider.GetFullTest(machineProvider.BayTestNumber);

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
                    errorsProvider.RecordNew(MachineErrorCode.FullTestFailed, machineProvider.BayTestNumber);
                    messageStatus = MessageStatus.OperationError;
                }
                else
                {
                    this.Logger.LogInformation($"Full test finished successfully for {machineProvider.LoadUnitsToTest.Count} Load Units on Bay {machineProvider.BayTestNumber}");
                    messageStatus = MessageStatus.OperationEnd;
                    setupProceduresDataProvider.IncreasePerformedCycles(setupRecord, machineProvider.RequiredCycles.Value);
                }
                setupProceduresDataProvider.MarkAsCompleted(setupRecord);
            }
            else
            {
                returnValue = true;
                if (machineProvider.ExecutedCycles > setupRecord.PerformedCycles)
                {
                    setupProceduresDataProvider.IncreasePerformedCycles(setupRecord, machineProvider.RequiredCycles.Value);
                    this.Logger.LogDebug($"Full test increment cycles on Bay {machineProvider.BayTestNumber}");
                }
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
                if (restore
                    && (missionToRestore.Step != MissionStep.WaitPick || this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay])
                    )
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
                    await this.ScheduleWmsMissionAsync(bayNumber, serviceProvider, mission);
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
                else if (mission.IsMissionWaiting())
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

            missionsDataProvider.PurgeMissions();

            var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
            errorsProvider.PurgeErrors();

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
                        mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                        newStep = new MissionMoveErrorLoadStep(mission, serviceProvider, eventAggregator);
                    }
                    else if (mission.RestoreStep == MissionStep.DepositUnit)
                    {
                        mission.NeedMovingBackward = true;
                        mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                        newStep = new MissionMoveErrorDepositStep(mission, serviceProvider, eventAggregator);
                    }
                    else
                    {
                        mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                        newStep = new MissionMoveErrorStep(mission, serviceProvider, eventAggregator);
                    }
                    newStep.OnEnter(null);
                }
            }

            foreach (var bay in bayProvider.GetAll().Where(w => w.CurrentMission != null))
            {
                if (!missions.Any(m => m.Id == bay.CurrentMission.Id && m.Status != MissionStatus.Completed && m.Status != MissionStatus.Aborted))
                {
                    bayProvider.ClearMission(bay.Number);
                }
            }
        }

        private bool CloseShutter(IBaysDataProvider bayProvider, IMachineResourcesProvider sensorsProvider, IShutterProvider shutterProvider)
        {
            if (this.machineVolatileDataProvider.IsShutterHomingActive.Any(x => x.Value))
            {
                return true;
            }

            var bays = bayProvider.GetAll().Where(x =>
                        x.Shutter != null
                        && x.Shutter.Type != ShutterType.NotSpecified
                        && x.CurrentMission == null
                        && x.Positions.All(p => p.LoadingUnit == null));

            var generated = false;

            if (bays.Any())
            {
                foreach (var bay in bays)
                {
                    var shutterInverter = (bay.Shutter != null) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                    if (sensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Closed
                        && sensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Half
                        )
                    {
                        shutterProvider.Move(ShutterMovementDirection.Down, bypassConditions: false, bay.Number, MessageActor.MachineManager);
                        this.machineVolatileDataProvider.IsShutterHomingActive[bay.Number] = true;
                        generated = true;
                        this.Logger.LogDebug($"Close Shutter: bay {bay.Number}");
                        break;
                    }
                }
            }

            return generated;
        }

        private bool GenerateHoming(IBaysDataProvider bayProvider, IMachineResourcesProvider sensorsProvider)
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
                        && (x.Carousel != null || x.IsExternal)
                        && x.CurrentMission == null
                        && x.Positions.All(p => p.LoadingUnit == null))?.Number ?? BayNumber.None;

                if (bayNumber != BayNumber.None
                    && !sensorsProvider.IsDrawerInBayBottom(bayNumber))
                {
                    IHomingMessageData homingData = new HomingMessageData(Axis.BayChain,
                        Calibration.FindSensor,
                        loadingUnitId: null,
                        showErrors: true);

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
                IHomingMessageData homingData = new HomingMessageData(Axis.HorizontalAndVertical,
                    Calibration.FindSensor,
                    loadingUnitId: null,
                    showErrors: true);

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

            var shutterProvider = serviceProvider.GetRequiredService<IShutterProvider>();

            switch (this.machineVolatileDataProvider.Mode)
            {
                case MachineMode.SwitchingToAutomatic:
                case MachineMode.SwitchingToLoadUnitOperations:
                case MachineMode.SwitchingToCompact:
                case MachineMode.SwitchingToFirstTest:
                case MachineMode.SwitchingToFullTest:
                    {
                        var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
                        if (errorsProvider.GetCurrent() != null)
                        {
                            this.Logger.LogError($"Scheduling Machine status switched to {MachineMode.Manual} from {this.machineVolatileDataProvider.Mode}: no error is allowed!");
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                            break;
                        }
                        // in this machine mode we generate homing for elevator and bays, but only if there are no missions to restore.
                        // if homing is not possible we switch anyway to automatic mode
                        var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                        var activeMissions = missionsDataProvider.GetAllActiveMissions();
                        var machineResourcesProvider = serviceProvider.GetRequiredService<IMachineResourcesProvider>();

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
                            && !this.CloseShutter(bayProvider, machineResourcesProvider, shutterProvider)
                            && !this.GenerateHoming(bayProvider, machineResourcesProvider)
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
                                    this.Logger.LogDebug($"Machine mode: {this.machineVolatileDataProvider.Mode}");
                                    await this.ScheduleMissionsOnBayAsync(bay.Number, serviceProvider);
                                }
                                else if (bay.Number < BayNumber.ElevatorBay)
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
                        var machineResourcesProvider = serviceProvider.GetRequiredService<IMachineResourcesProvider>();
                        if (!this.CloseShutter(bayProvider, machineResourcesProvider, shutterProvider)
                            && !this.GenerateHoming(bayProvider, machineResourcesProvider)
                            && !this.ScheduleFirstTestMissions(serviceProvider)
                            )
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
            this.Logger.LogDebug("OnBayOperationalStatusChangedAsync");
            await this.InvokeSchedulerAsync(serviceProvider);
        }

        private async Task OnDataLayerReadyAsync(IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("OnDataLayerReady start");
            var servicingInfo = serviceProvider.GetRequiredService<IServicingProvider>();
            servicingInfo.CheckServicingInfo();

            var loadUnitsDataProvider = serviceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
            loadUnitsDataProvider.UpdateWeightStatistics();

            GetPersistedMissions(serviceProvider, this.EventAggregator);
            this.RestoreFullTest(serviceProvider);

            this.dataLayerIsReady = true;
            this.Logger.LogDebug("OnDataLayerReadyAsync");
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
                        this.machineVolatileDataProvider.IsHomingExecuted = true;
                    }

                    await this.InvokeSchedulerAsync(serviceProvider);
                }
                else if (message.Status == MessageStatus.OperationError
                    || message.Status == MessageStatus.OperationRunningStop
                    || message.Status == MessageStatus.OperationFaultStop
                    )
                {
                    this.machineVolatileDataProvider.IsHomingActive = false;

                    if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic)
                    {
                        this.machineVolatileDataProvider.Mode = MachineMode.Manual; //MachineMode.Automatic;
                        this.Logger.LogInformation($"Automation Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    }
                    else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations)
                    {
                        this.machineVolatileDataProvider.Mode = MachineMode.Manual; // MachineMode.LoadUnitOperations;
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
            else
            {
                this.Logger.LogTrace("Cannot perform mission scheduling, because data layer is not ready.");
            }
        }

        private async Task OnShutterPositioning(NotificationMessage message, IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status != MissionStatus.New))
            {
                if (message.Status == MessageStatus.OperationEnd
                    && message.Data is ShutterPositioningMessageData data
                    )
                {
                    this.machineVolatileDataProvider.IsShutterHomingActive[message.TargetBay] = false;
                    await this.InvokeSchedulerAsync(serviceProvider);
                }
                else if (message.Status == MessageStatus.OperationError
                    || message.Status == MessageStatus.OperationRunningStop
                    || message.Status == MessageStatus.OperationFaultStop
                    )
                {
                    this.machineVolatileDataProvider.IsShutterHomingActive[message.TargetBay] = false;
                }
            }
        }

        /// <summary>
        /// we get this message every hour
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private void OnTimePeriodElapsed(IServiceProvider serviceProvider)
        {
            // at midnight it is time to do some housework
            if (DateTime.Now.Hour == 0)
            {
                this.Logger.LogInformation($"OnTimePeriodElapsed: clean up missions and errors and schedule homing");
                var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();

                // clean missions
                missionsDataProvider.PurgeMissions();

                // clean errors
                var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
                errorsProvider.PurgeErrors();

                // elevator and bay chain homing every new day
                this.machineVolatileDataProvider.IsHomingExecuted = false;

                var bayDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                foreach (var bay in bayDataProvider.GetAll().Where(b => b.Carousel != null))
                {
                    this.machineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = false;
                }

                // try to fix missions not starting in the morning because of "Bay chain not calibrated"
                if (this.machineVolatileDataProvider.Mode == MachineMode.Automatic
                    && !missionsDataProvider.GetAllActiveMissions().Any()
                    )
                {
                    var machineModeProvider = serviceProvider.GetRequiredService<IMachineModeProvider>();
                    machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Manual);
                }
            }

            this.servicingProvider.UpdateServiceStatus();
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
                        this.Logger.LogDebug("Schedule mission restore on bay {bay.Number}");
                        await this.ScheduleMissionsOnBayAsync(bay.Number, serviceProvider, true);
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Failed to schedule missions on bay {number}.", bay.Number);
                    }
                }
            }
        }

        private async Task ScheduleWmsMissionAsync(
            BayNumber bayNumber,
            IServiceProvider serviceProvider,
            Mission mission)
        {
            System.Diagnostics.Debug.Assert(mission.WmsId.HasValue);

            if (!serviceProvider.GetRequiredService<IWmsSettingsProvider>().IsEnabled)
            {
                this.Logger.LogError("WMS mission {wmsId} can not start because WMS is not enabled.", mission.WmsId);
                return;
            }

            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var missionsWmsWebService = serviceProvider.GetRequiredService<WMS.Data.WebAPI.Contracts.IMissionsWmsWebService>();
            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

            try
            {
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
                    else if (mission.Status is MissionStatus.Waiting && mission.Step != MissionStep.BayChain)
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
                    mission.Status = MissionStatus.Completed;
                    missionsDataProvider.Update(mission);
                    baysDataProvider.ClearMission(bayNumber);

                    this.Logger.LogInformation("Bay {bayNumber}: WMS mission {missionId} completed.", bayNumber, mission.WmsId.Value);

                    //    this.CompleteCurrentMissionInBay(bayNumber, mission, serviceProvider);
                }
            }
            catch (Exception ex)
            {
                var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
                errorsProvider.RecordNew(DataModels.MachineErrorCode.WmsError, BayNumber.None, ex.Message.Replace("\n", " ").Replace("\r", " "));
            }
        }

        #endregion
    }
}
