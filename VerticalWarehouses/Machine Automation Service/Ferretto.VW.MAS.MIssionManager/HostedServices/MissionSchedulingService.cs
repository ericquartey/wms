using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
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
//using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class MissionSchedulingService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private const int CleanupTimeout = 60 * 60 * 1000;  // one hour expressed in milliseconds

        private readonly Timer CleanupTimer;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly Timer DelayAfterPut1;
        private readonly Timer DelayAfterPut2;
        private readonly Timer DelayAfterPut3;

        private readonly IServicingProvider servicingProvider;

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        private bool dataLayerIsReady;

        private int DelayTimeout = 3000;

        private bool firstCleanupExecuted;

        private readonly Dictionary<BayNumber, bool> isDelayFinish;

        private LoadingUnitLocation loadUnitSource;

        #endregion

        #region Constructors

        public MissionSchedulingService(
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IServicingProvider servicingProvider,
            IEventAggregator eventAggregator,
            IWmsSettingsProvider wmsSettingsProvider,
            ILogger<MissionSchedulingService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.servicingProvider = servicingProvider ?? throw new ArgumentNullException(nameof(servicingProvider));
            this.wmsSettingsProvider = wmsSettingsProvider ?? throw new ArgumentNullException(nameof(wmsSettingsProvider));
            this.CleanupTimer = new Timer(this.OnTimePeriodElapsed, null, Timeout.Infinite, Timeout.Infinite);
            this.DelayAfterPut1 = new Timer(this.OnTimePeriodElapsed2, BayNumber.BayOne, Timeout.Infinite, Timeout.Infinite);
            this.DelayAfterPut2 = new Timer(this.OnTimePeriodElapsed2, BayNumber.BayTwo, Timeout.Infinite, Timeout.Infinite);
            this.DelayAfterPut3 = new Timer(this.OnTimePeriodElapsed2, BayNumber.BayThree, Timeout.Infinite, Timeout.Infinite);

            this.isDelayFinish = new Dictionary<BayNumber, bool>();
            this.isDelayFinish.Add(BayNumber.BayOne, false);
            this.isDelayFinish.Add(BayNumber.BayTwo, false);
            this.isDelayFinish.Add(BayNumber.BayThree, false);
        }

        #endregion

        #region Methods

        public bool ScheduleCompactingMissions(IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();

            if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status != MissionStatus.New && m.Status != MissionStatus.Waiting))
            {
                return serviceProvider.GetRequiredService<IMissionSchedulingProvider>().QueueLoadingUnitCompactingMission(serviceProvider);
            }
            // no more compacting is possible. Exit from compact mode
            var machineProvider = serviceProvider.GetRequiredService<IMachineProvider>();
            this.machineVolatileDataProvider.IsOptimizeRotationClass = machineProvider.GetMinMaxHeight().IsRotationClass;
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
            var loadingUnit = serviceProvider.GetRequiredService<ILoadingUnitsDataProvider>().GetById(loadUnitId.Value);
            if (loadingUnit.Tare == 0.00)
            {
                this.Logger.LogError($"First Test error: Load Unit tare is zero!");
                errorsProvider.RecordNew(MachineErrorCode.LoadUnitTareError, machineProvider.BayTestNumber);
                return false;
            }

            var cellsProvider = serviceProvider.GetRequiredService<ICellsProvider>();
            if (machineProvider.ExecutedCycles == 0)
            {
                // first cycle: init RequiredCycles and cells to test
                var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                this.loadUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(loadUnitId.Value);
                var bayNumber = baysDataProvider.GetByLoadingUnitLocation(this.loadUnitSource)?.Number ?? BayNumber.None;
                if (this.loadUnitSource != LoadingUnitLocation.NoLocation)
                {
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
                var height = loadingUnit.Height;
                if (height == 0)
                {
                    var machine = serviceProvider.GetRequiredService<IMachineProvider>();
                    height = machine.GetMinMaxHeight().LoadUnitMinHeight;
                }
                machineProvider.RequiredCycles = cellsProvider.SetCellsToTest(bayNumber, height);
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
                this.machineVolatileDataProvider.RandomCells = false;
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
                    || (mission.Status is MissionStatus.Waiting && mission.Step is MissionStep.WaitDepositExternalBay)
                    || (mission.Status is MissionStatus.Waiting && mission.Step is MissionStep.WaitDepositInternalBay)
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
                this.machineVolatileDataProvider.RandomCells = false;
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
                    && (missionToRestore.Step != MissionStep.WaitPick ||
                        this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay])
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
                this.Logger.LogDebug($"Do not process any other missions until this is completed...");

                // there is a mission being completed: do not process any other missions until this is completed
                return;
            }

            this.Logger.LogDebug($"ScheduleMissionsOnBayAsync on Bay {bayNumber} | Get the active missions...");

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
                this.Logger.LogDebug($"No more missions are available for scheduling on bay {bayNumber}");

                // no more missions are available for scheduling on this bay
                this.NotifyAssignedMissionChanged(bayNumber, null);
                return;
            }

            // -------------
            //var allMissions = missionsDataProvider.GetAllMissions();
            //this.Logger.LogDebug($"all Missions: Count:{allMissions.Count()}");
            //foreach (var m in allMissions)
            //{
            //    this.Logger.LogDebug($"Mission Id:{m.Id}");
            //}
            // --------------

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
                        //this.NotifyAssignedMissionChanged(bayNumber, mission.Id);
                    }
                }
                else if (mission.IsMissionWaiting())
                {
                    var loadingUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(mission.LoadUnitId);

                    this.Logger.LogInformation("Bay {bayNumber}: mission {missionId} Resume waiting.", bayNumber, mission.Id);
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

        private static bool FullTestCheckBay(IServiceProvider serviceProvider)
        {
            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
            var missionSchedulingProvider = serviceProvider.GetRequiredService<IMissionSchedulingProvider>();
            var machineProvider = serviceProvider.GetRequiredService<IMachineVolatileDataProvider>();
            var sensorsProvider = serviceProvider.GetRequiredService<ISensorsProvider>();
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();

            var bay = baysDataProvider.GetByNumberPositions(machineProvider.BayTestNumber);
            if (bay != null)
            {
                foreach (var bayPosition in bay.Positions)
                {
                    if (bayPosition != null &&
                        bayPosition.LoadingUnit != null &&
                        sensorsProvider.IsLoadingUnitInLocation(bayPosition.Location))
                    {
                        var missions = missionsDataProvider.GetAllActiveMissionsByBay(bay.Number);
                        if (!missions.Any(s => s.LoadUnitId == bayPosition.LoadingUnit.Id))
                        {
                            missionSchedulingProvider.QueueRecallMission(bayPosition.LoadingUnit.Id, bay.Number, MissionType.FullTestIN);

                            missions = missionsDataProvider.GetAllActiveMissionsByBay(bay.Number);
                            var mission = missions.Single(s => s.LoadUnitId == bayPosition.LoadingUnit.Id);

                            if (mission != null &&
                                mission.Status == MissionStatus.New &&
                                mission.MissionType == MissionType.FullTestIN)
                            {
                                moveLoadingUnitProvider.ActivateMoveToCell(
                                 mission.Id,
                                 mission.MissionType,
                                 mission.LoadUnitId,
                                 bay.Number,
                                 MessageActor.MissionManager);

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
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
                        var bay = bayProvider.GetByNumberCarousel(mission.TargetBay);
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
                    var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                    if (shutterInverter != InverterDriver.Contracts.InverterIndex.None
                        && sensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Closed
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
            foreach (var bay in bays.Where(x =>
                this.machineVolatileDataProvider.IsBayHomingExecuted.ContainsKey(x.Number)
                    //&& !this.machineVolatileDataProvider.IsBayHomingExecuted[x.Number]
                    && (x.Carousel != null || x.IsExternal)
                    && x.CurrentMission == null
                    && x.Positions.All(p => p.LoadingUnit == null)))
            {
                if ((bay.Carousel != null
                        && !sensorsProvider.IsDrawerInBayBottom(bay.Number))
                        && !this.machineVolatileDataProvider.IsBayHomingExecuted[bay.Number]
                    || (bay.IsExternal
                            && !sensorsProvider.IsDrawerInBayExternalPosition(bay.Number, bay.IsDouble)
                            && (!this.machineVolatileDataProvider.IsBayHomingExecuted[bay.Number]
                                || (!bay.IsDouble && !sensorsProvider.IsSensorZeroOnBay(bay.Number)))
                                )
                    )
                {
                    IHomingMessageData homingData = new HomingMessageData(Axis.BayChain,
                        Calibration.FindSensor,
                        loadingUnitId: null,
                        showErrors: true,
                        turnBack: false);

                    this.EventAggregator
                        .GetEvent<CommandEvent>()
                        .Publish(
                            new CommandMessage(
                                homingData,
                                "Execute Homing Command",
                                MessageActor.DeviceManager,
                                MessageActor.MissionManager,
                                MessageType.Homing,
                                bay.Number));
                    this.Logger.LogDebug($"GenerateHoming: bay {bay.Number}");
                    generated = true;
                    break;
                }
            }

            if (!generated && !this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay])
            {
                IHomingMessageData homingData = new HomingMessageData(Axis.HorizontalAndVertical,
                    Calibration.FindSensor,
                    loadingUnitId: null,
                    showErrors: true,
                    turnBack: false);

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
                if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToShutdown)
                {
                    this.machineVolatileDataProvider.Mode = MachineMode.Shutdown;
                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    return;
                }
                this.Logger.LogWarning("Mission scheduling is not allowed: machine is not in running state.");
                return;
            }

            var bayProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

            var shutterProvider = serviceProvider.GetRequiredService<IShutterProvider>();

            switch (this.machineVolatileDataProvider.Mode)
            {
                case MachineMode.SwitchingToAutomatic:
                case MachineMode.SwitchingToLoadUnitOperations:
                case MachineMode.SwitchingToLoadUnitOperations2:
                case MachineMode.SwitchingToLoadUnitOperations3:
                case MachineMode.SwitchingToCompact:
                case MachineMode.SwitchingToFirstTest:
                case MachineMode.SwitchingToFullTest:
                case MachineMode.SwitchingToCompact2:
                case MachineMode.SwitchingToFirstTest2:
                case MachineMode.SwitchingToFullTest2:
                case MachineMode.SwitchingToCompact3:
                case MachineMode.SwitchingToFirstTest3:
                case MachineMode.SwitchingToFullTest3:
                    {
                        var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
                        var machineResourcesProvider = serviceProvider.GetRequiredService<IMachineResourcesProvider>();

                        if (machineResourcesProvider.PreFireAlarm)
                        {
                            this.Logger.LogError("FireAlarm Active");
                            errorsProvider.RecordNew(MachineErrorCode.PreFireAlarm, BayNumber.All);
                        }

                        if (errorsProvider.GetCurrent() != null)
                        {
                            //this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                            //this.Logger.LogError($"Scheduling Machine status switched to {MachineMode.Manual} from {this.machineVolatileDataProvider.Mode}: no error is allowed!");
                            this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(errorsProvider.GetCurrent().BayNumber);
                            this.Logger.LogError($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode} from {this.machineVolatileDataProvider.Mode}: no error is allowed!");
                            break;
                        }
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
                                    switch (activeMissions.FirstOrDefault(m => m.MissionType == MissionType.Compact).TargetBay)
                                    {
                                        case BayNumber.BayOne:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToCompact;
                                            break;

                                        case BayNumber.BayTwo:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToCompact2;
                                            break;

                                        case BayNumber.BayThree:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToCompact3;
                                            break;

                                        default:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToCompact;
                                            break;
                                    }

                                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                }
                                else if (activeMissions.Any(m => m.MissionType == MissionType.FirstTest))
                                {
                                    switch (activeMissions.FirstOrDefault(m => m.MissionType == MissionType.FirstTest).TargetBay)
                                    {
                                        case BayNumber.BayOne:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFirstTest;
                                            break;

                                        case BayNumber.BayTwo:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFirstTest2;
                                            break;

                                        case BayNumber.BayThree:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFirstTest3;
                                            break;

                                        default:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFirstTest;
                                            break;
                                    }

                                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                }
                                else if (activeMissions.Any(m => m.MissionType == MissionType.FullTestIN || m.MissionType == MissionType.FullTestOUT))
                                {
                                    switch (activeMissions.FirstOrDefault(m => m.MissionType == MissionType.FullTestIN || m.MissionType == MissionType.FullTestOUT).TargetBay)
                                    {
                                        case BayNumber.BayOne:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFullTest;
                                            break;

                                        case BayNumber.BayTwo:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFullTest2;
                                            break;

                                        case BayNumber.BayThree:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFullTest3;
                                            break;

                                        default:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFullTest;
                                            break;
                                    }

                                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                }
                                else if (activeMissions.Any(m => m.MissionType == MissionType.LoadUnitOperation))
                                {
                                    switch (activeMissions.FirstOrDefault(m => m.MissionType == MissionType.LoadUnitOperation).TargetBay)
                                    {
                                        case BayNumber.BayOne:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToLoadUnitOperations;
                                            break;

                                        case BayNumber.BayTwo:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToLoadUnitOperations2;
                                            break;

                                        case BayNumber.BayThree:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToLoadUnitOperations3;
                                            break;

                                        default:
                                            this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFullTest;
                                            break;
                                    }

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
                            if (!this.IsLoadUnitMissing(serviceProvider, machineResourcesProvider))
                            {
                                if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToCompact
                                    || activeMissions.Any(m => m.MissionType == MissionType.Compact && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.Compact;
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToCompact2
                                    || activeMissions.Any(m => m.MissionType == MissionType.Compact && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.Compact2;
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToCompact3
                                    || activeMissions.Any(m => m.MissionType == MissionType.Compact && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.Compact3;
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToFirstTest
                                    || activeMissions.Any(m => m.MissionType == MissionType.FirstTest && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.FirstTest;
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToFirstTest2
                                    || activeMissions.Any(m => m.MissionType == MissionType.FirstTest && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.FirstTest2;
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToFirstTest3
                                    || activeMissions.Any(m => m.MissionType == MissionType.FirstTest && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.FirstTest3;
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToFullTest
                                        || activeMissions.Any(m => (m.MissionType == MissionType.FullTestIN || m.MissionType == MissionType.FullTestOUT)
                                        && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    if (!FullTestCheckBay(serviceProvider))
                                    {
                                        this.machineVolatileDataProvider.Mode = MachineMode.FullTest;
                                    }
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToFullTest2
                                    || activeMissions.Any(m => (m.MissionType == MissionType.FullTestIN || m.MissionType == MissionType.FullTestOUT)
                                        && m.Status == MissionStatus.Executing
                                        )
                                    )
                                {
                                    if (!FullTestCheckBay(serviceProvider))
                                    {
                                        this.machineVolatileDataProvider.Mode = MachineMode.FullTest2;
                                    }
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToFullTest3
                                    || activeMissions.Any(m => (m.MissionType == MissionType.FullTestIN || m.MissionType == MissionType.FullTestOUT)
                                        && m.Status == MissionStatus.Executing
                                        )
                                    )
                                {
                                    if (!FullTestCheckBay(serviceProvider))
                                    {
                                        this.machineVolatileDataProvider.Mode = MachineMode.FullTest3;
                                    }
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations
                                    || activeMissions.Any(m => m.MissionType == MissionType.LoadUnitOperation && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.LoadUnitOperations;
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations2
                                    || activeMissions.Any(m => m.MissionType == MissionType.LoadUnitOperation && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.LoadUnitOperations2;
                                }
                                else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations3
                                    || activeMissions.Any(m => m.MissionType == MissionType.LoadUnitOperation && m.Status == MissionStatus.Executing)
                                    )
                                {
                                    this.machineVolatileDataProvider.Mode = MachineMode.LoadUnitOperations3;
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
                        if (!this.ScheduleCompactingMissions(serviceProvider))
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                            this.Logger.LogInformation($"Compacting terminated. Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
                    }
                    break;

                case MachineMode.Compact2:
                    {
                        if (!this.ScheduleCompactingMissions(serviceProvider))
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual2;
                            this.Logger.LogInformation($"Compacting terminated. Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
                    }
                    break;

                case MachineMode.Compact3:
                    {
                        if (!this.ScheduleCompactingMissions(serviceProvider))
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual3;
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

                case MachineMode.FirstTest2:
                    {
                        var machineResourcesProvider = serviceProvider.GetRequiredService<IMachineResourcesProvider>();
                        if (!this.CloseShutter(bayProvider, machineResourcesProvider, shutterProvider)
                            && !this.GenerateHoming(bayProvider, machineResourcesProvider)
                            && !this.ScheduleFirstTestMissions(serviceProvider)
                            )
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual2;
                            this.Logger.LogInformation($"First test terminated. Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
                    }
                    break;

                case MachineMode.FirstTest3:
                    {
                        var machineResourcesProvider = serviceProvider.GetRequiredService<IMachineResourcesProvider>();
                        if (!this.CloseShutter(bayProvider, machineResourcesProvider, shutterProvider)
                            && !this.GenerateHoming(bayProvider, machineResourcesProvider)
                            && !this.ScheduleFirstTestMissions(serviceProvider)
                            )
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual3;
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

                case MachineMode.FullTest2:
                    {
                        if (!this.ScheduleFullTestMissions(serviceProvider))
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual2;
                            this.Logger.LogInformation($"Full test terminated. Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
                    }
                    break;

                case MachineMode.FullTest3:
                    {
                        if (!this.ScheduleFullTestMissions(serviceProvider))
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual3;
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

                case MachineMode.SwitchingToManual2:
                    {
                        var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                        if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status == MissionStatus.Executing
                            && m.Step > MissionStep.New)
                            )
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual2;
                            this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
                    }
                    break;

                case MachineMode.SwitchingToManual3:
                    {
                        var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                        if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status == MissionStatus.Executing
                            && m.Step > MissionStep.New)
                            )
                        {
                            this.machineVolatileDataProvider.Mode = MachineMode.Manual3;
                            this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
                    }
                    break;

                case MachineMode.SwitchingToShutdown:
                    {
                        var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                        if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.Status == MissionStatus.Executing
                                && m.Step > MissionStep.New
                                && m.Step < MissionStep.Error)
                            && !this.machineVolatileDataProvider.IsHomingActive
                            )
                        {
                            var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
                            if (errorsProvider.GetCurrent() == null
                                && !missionsDataProvider.GetAllActiveMissions().Any(m => m.Status == MissionStatus.Executing
                                    && m.Step >= MissionStep.Error)
                                )
                            {
                                IHomingMessageData homingData = new HomingMessageData(Axis.HorizontalAndVertical,
                                    Calibration.FindSensor,
                                    loadingUnitId: null,
                                    showErrors: true,
                                    turnBack: false);

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
                                this.machineVolatileDataProvider.IsHomingActive = true;
                            }
                            this.machineVolatileDataProvider.Mode = MachineMode.Shutdown;
                            this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        }
                    }
                    break;

                case MachineMode.Shutdown:
                    if (!this.machineVolatileDataProvider.IsHomingActive)
                    {
                        var runningStateProvider = serviceProvider.GetRequiredService<IRunningStateProvider>();
                        if (runningStateProvider.MachinePowerState == MachinePowerState.Powered)
                        {
                            runningStateProvider.SetRunningState(false, BayNumber.BayOne, MessageActor.MissionManager);
                        }
                    }
                    // wait for ppc app to shutdown MAS and system
                    break;

                default:
                    {
                        this.Logger.LogDebug("Mission scheduling is not allowed: machine is not in automatic mode.");
                    }
                    break;
            }
        }

        private bool IsLoadUnitMissing(IServiceProvider serviceProvider, IMachineResourcesProvider machineResourcesProvider)
        {
            var sensorProvider = serviceProvider.GetRequiredService<ISensorsProvider>();
            var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();
            var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
            var bayProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
            var loadUnitMovementProvider = serviceProvider.GetRequiredService<ILoadingUnitMovementProvider>();
            if (sensorProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
            {
                var loadUnit = elevatorDataProvider.GetLoadingUnitOnBoard();
                if (loadUnit is null)
                {
                    errorsProvider.RecordNew(MachineErrorCode.LoadUnitMissingOnElevator);

                    //this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                    this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(errorsProvider.GetCurrent().BayNumber);
                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    return true;
                }

                if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitId == loadUnit.Id))
                {
                    if (loadUnit.Height == 0)
                    {
                        var loadingUnitProvider = serviceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
                        var machineProvider = serviceProvider.GetRequiredService<IMachineProvider>();
                        var machine = machineProvider.GetMinMaxHeight();
                        loadingUnitProvider.SetHeight(loadUnit.Id, machine.LoadUnitMaxHeight);
                    }
                    var bays = bayProvider.GetAll();
                    MissionType missionType;
                    var isFullTestMission = this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToFullTest || this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToFullTest2 || this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToFullTest3;
                    foreach (var bay in bays)
                    {
                        foreach (var position in bay.Positions.OrderBy(b => b.Location))
                        {
                            if (!sensorProvider.IsLoadingUnitInLocation(position.Location)
                                && !position.IsBlocked
                                && (!bay.IsExternal
                                    || (!bay.IsDouble && !loadUnitMovementProvider.IsInternalPositionOccupied(bay))
                                    || (bay.IsDouble && !loadUnitMovementProvider.IsInternalPositionOccupied(bay.Number, position.Location))
                                    )
                                )
                            {
                                this.Logger.LogInformation($"Eject load unit {loadUnit.Id} from {LoadingUnitLocation.Elevator} to bay");
                                missionType = (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic) ? MissionType.OUT : isFullTestMission ? MissionType.FullTestOUT : MissionType.LoadUnitOperation;
                                moveLoadingUnitProvider.EjectFromCell(missionType, position.Location, loadUnit.Id, bay.Number, MessageActor.AutomationService);
                                return true;
                            }
                        }
                    }
                    this.Logger.LogInformation($"Insert load unit {loadUnit.Id} from {LoadingUnitLocation.Elevator} to cell");
                    missionType = (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic) ? MissionType.IN : isFullTestMission ? MissionType.FullTestIN : MissionType.LoadUnitOperation;
                    moveLoadingUnitProvider.InsertToCell(missionType, LoadingUnitLocation.Elevator, null, loadUnit.Id, BayNumber.BayOne, MessageActor.AutomationService);
                    return true;
                }
            }

            if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic)
            {
                var bays = bayProvider.GetAll();
                var loadUnit = elevatorDataProvider.GetLoadingUnitOnBoard();
                if (loadUnit is null
                    && !machineResourcesProvider.IsSensorZeroOnCradle
                    && machineResourcesProvider.IsDrawerCompletelyOffCradle)
                {
                    errorsProvider.RecordNew(MachineErrorCode.MissingZeroSensorWithEmptyElevator);

                    this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(errorsProvider.GetCurrent().BayNumber);
                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    return true;
                }
                if (loadUnit is null
                    && machineResourcesProvider.IsDrawerPartiallyOnCradle)
                {
                    errorsProvider.RecordNew(MachineErrorCode.InvalidPresenceSensors);

                    this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(errorsProvider.GetCurrent().BayNumber);
                    this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    return true;
                }

                foreach (var bay in bays)
                {
                    if (bay.Carousel != null
                        && !machineResourcesProvider.IsSensorZeroOnBay(bay.Number))
                    {
                        errorsProvider.RecordNew(MachineErrorCode.SensorZeroBayNotActiveAtStart);
                        this.machineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = false;
                        if (bay.CurrentMission != null)
                        {
                            moveLoadingUnitProvider.StopMove(bay.CurrentMission.Id, bay.Number, bay.Number, MessageActor.MissionManager);
                        }

                        //this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                        this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(bay.Number);
                        this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                        return true;
                    }

                    foreach (var position in bay.Positions.OrderBy(b => b.Location))
                    {
                        if ((sensorProvider.IsLoadingUnitInLocation(position.Location) && !position.IsBlocked)
                            || (!bay.IsDouble && bay.IsExternal && loadUnitMovementProvider.IsInternalPositionOccupied(bay))
                            || (bay.IsDouble && bay.IsExternal && loadUnitMovementProvider.IsInternalPositionOccupied(bay.Number, position.Location)))
                        {
                            if (position.LoadingUnit is null)
                            {
                                errorsProvider.RecordNew(MachineErrorCode.LoadUnitMissingOnBay, bay.Number);

                                //this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                                this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(bay.Number);
                                this.Logger.LogInformation($"Scheduling Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                return true;
                            }

                            if (!missionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitId == position.LoadingUnit.Id
                                    && m.TargetBay == bay.Number
                                    && (m.Status != MissionStatus.New || m.MissionType == MissionType.IN)
                                    )
                                )
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
            this.Logger.LogDebug("InvokeSchedulerAsync");
            await this.InvokeSchedulerAsync(serviceProvider);
        }

        private async Task OnChangeRunningState(IServiceProvider serviceProvider)
        {
            if (this.machineVolatileDataProvider.Mode == MachineMode.Shutdown)
            {
                this.Logger.LogDebug("InvokeSchedulerAsync");
                await this.InvokeSchedulerAsync(serviceProvider);
            }
        }

        private async Task OnDataLayerReadyAsync(IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("OnDataLayerReady start");
            //var servicingInfo = serviceProvider.GetRequiredService<IServicingProvider>();
            //servicingInfo.CheckServicingInfo();

            //var loadUnitsDataProvider = serviceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
            //loadUnitsDataProvider.UpdateWeightStatistics();

            this.OnTimePeriodElapsed(null);
            GetPersistedMissions(serviceProvider, this.EventAggregator);
            this.RestoreFullTest(serviceProvider);

            this.dataLayerIsReady = true;

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(
                    new NotificationMessage
                    {
                        Data = new MachineModeMessageData(MachineMode.NotSpecified),
                        Destination = MessageActor.Any,
                        Source = MessageActor.DataLayer,
                        Type = MessageType.MachineMode,
                    });

            this.Logger.LogDebug("InvokeSchedulerAsync");
            await this.InvokeSchedulerAsync(serviceProvider);

            this.CleanupTimer.Change(CleanupTimeout, CleanupTimeout);

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
                        var bayProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                        var bay = bayProvider.GetByNumber(message.TargetBay);
                        if (bay.CurrentMission is null)
                        {
                            this.machineVolatileDataProvider.IsBayHomingExecuted[message.RequestingBay] = true;
                        }
                    }
                    else if (data.AxisToCalibrate == Axis.Vertical || data.AxisToCalibrate == Axis.HorizontalAndVertical)
                    {
                        this.machineVolatileDataProvider.IsHomingExecuted = true;
                    }

                    this.Logger.LogDebug("InvokeSchedulerAsync");
                    await this.InvokeSchedulerAsync(serviceProvider);
                }
                else if (message.Status == MessageStatus.OperationError
                    || message.Status == MessageStatus.OperationRunningStop
                    || message.Status == MessageStatus.OperationFaultStop
                    )
                {
                    this.machineVolatileDataProvider.IsHomingActive = false;

                    if (message.Data is IHomingMessageData dataHoming
                        && dataHoming.AxisToCalibrate == Axis.BayChain)
                    {
                        this.machineVolatileDataProvider.IsBayHomingExecuted[message.RequestingBay] = false;
                    }
                    else
                    {
                        this.machineVolatileDataProvider.IsHomingExecuted = false;
                    }
                    this.Logger.LogDebug("Homing failed");

                    if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic)
                    {
                        //this.machineVolatileDataProvider.Mode = MachineMode.Manual; //MachineMode.Automatic;
                        this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(message.TargetBay); //MachineMode.Automatic;
                        this.Logger.LogInformation($"Automation Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    }
                    else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations)
                    {
                        this.machineVolatileDataProvider.Mode = MachineMode.Manual; // MachineMode.LoadUnitOperations;
                        this.Logger.LogInformation($"Automation Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    }
                    else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations2)
                    {
                        this.machineVolatileDataProvider.Mode = MachineMode.Manual2; // MachineMode.LoadUnitOperations;
                        this.Logger.LogInformation($"Automation Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    }
                    else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations3)
                    {
                        this.machineVolatileDataProvider.Mode = MachineMode.Manual3; // MachineMode.LoadUnitOperations;
                        this.Logger.LogInformation($"Automation Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    }
                    else if (this.machineVolatileDataProvider.Mode == MachineMode.SwitchingToShutdown)
                    {
                        this.machineVolatileDataProvider.Mode = MachineMode.Shutdown;
                        this.Logger.LogInformation($"Automation Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    }
                    else if (this.machineVolatileDataProvider.Mode == MachineMode.Shutdown)
                    {
                        this.Logger.LogDebug("InvokeSchedulerAsync");
                        await this.InvokeSchedulerAsync(serviceProvider);
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
                    //this.NotifyAssignedMissionChanged(mission.TargetBay, null);
                }
                else if (mission.LoadUnitDestination != LoadingUnitLocation.Cell
                    &&
                    mission.LoadUnitDestination != LoadingUnitLocation.Elevator
                    &&
                    mission.Status != MissionStatus.Waiting
                    &&
                    mission.Status != MissionStatus.Completed
                    )
                // loading unit to bay mission
                {
                    baysDataProvider.AssignMission(mission.TargetBay, mission);
                    this.NotifyAssignedMissionChanged(mission.TargetBay, mission.Id);
                }
                else if (mission.Status != MissionStatus.Waiting)
                // any other mission type
                {
                    missionsDataProvider.Complete(mission.Id);
                    //this.NotifyAssignedMissionChanged(mission.TargetBay, null);

                    //var bCreateAMissionForExceptionalCase = false;
                    //if (mission.Status == MissionStatus.Completed)
                    //{
                    //    // Analyze the given mission.
                    //    // If mission has an error code of MachineErrorCode.LoadUnitWeightExceeded and in MissionStep.Completed then
                    //    // it is prompted the creation of a new mission to handle the showing of error condition for the
                    //    // LoadUnitWeightExceeded and the following return of drawer in the warehouse
                    //    if (mission.ErrorCode == MachineErrorCode.LoadUnitWeightExceeded)
                    //    {
                    //        var bay = baysDataProvider.GetByLoadingUnitLocation(mission.LoadUnitSource);
                    //        if (!(bay is null))
                    //        {
                    //            // Only applied for internal double bay
                    //            if (bay.IsDouble && bay.Carousel == null && !bay.IsExternal)
                    //            {
                    //                bCreateAMissionForExceptionalCase = true;
                    //            }
                    //        }
                    //    }
                    //}

                    //if (!bCreateAMissionForExceptionalCase)
                    //{
                    //    // Set the given mission to MissionStatus.Completed.
                    //    // The given mission is also deleted from the collection of active missions
                    //    missionsDataProvider.Complete(mission.Id);
                    //    this.NotifyAssignedMissionChanged(mission.TargetBay, null);
                    //}
                    //else
                    //{
                    //    // Create a new mission one. It is handled the condition described above
                    //    this.NotifyAssignedMissionChanged(mission.TargetBay, null);
                    //    var missionNew = missionsDataProvider.CreateBayMission(mission.LoadUnitId, mission.TargetBay, MissionType.IN);
                    //    this.Logger.LogDebug($"Handle condition on double internal bay. Create a new mission {missionNew.Id} to recall loading unit into warehouse");
                    //}
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Failed to process mission: {ex.Message}");
            }

            this.Logger.LogDebug("InvokeSchedulerAsync");
            await this.InvokeSchedulerAsync(serviceProvider);
        }

        private async Task OnMachineModeChangedAsync(IServiceProvider serviceProvider)
        {
            this.Logger.LogDebug("InvokeSchedulerAsync");
            await this.InvokeSchedulerAsync(serviceProvider);
        }

        private async Task OnNewMachineMissionAvailableAsync(IServiceProvider serviceProvider)
        {
            this.Logger.LogDebug($"OnNewMachineMissionAvailableAsync | A new machine mission is available: InvokeSchedulerAsync");
            await this.InvokeSchedulerAsync(serviceProvider);
        }

        private async Task OnOperationComplete(MissionOperationCompletedMessageData messageData, IServiceProvider serviceProvider)
        {
            if (this.dataLayerIsReady)
            {
                this.Logger.LogDebug("InvokeSchedulerAsync");
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
                    this.Logger.LogDebug("InvokeSchedulerAsync");
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
        /// <returns></returns>
        private void OnTimePeriodElapsed(object state)
        {
            var scope = this.ServiceScopeFactory.CreateScope();

            // at midnight it is time to do some housework
            if (DateTime.Now.Hour == 4
                || !this.firstCleanupExecuted
                )
            {
                this.Logger.LogInformation($"OnTimePeriodElapsed: clean up missions and errors and schedule homing");
                var missionsDataProvider = scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>();

                // clean missions
                missionsDataProvider.PurgeMissions();

                // clean errors
                var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
                errorsProvider.PurgeErrors();

                // elevator and bay chain homing every new day?
                //this.machineVolatileDataProvider.IsHomingExecuted = false;

                //var bayDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                //foreach (var bay in bayDataProvider.GetAll().Where(b => b.Carousel != null || b.IsExternal))
                //{
                //    this.machineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = false;
                //}

                // try to fix missions not starting in the morning because of "Bay chain not calibrated"
                //if (this.machineVolatileDataProvider.Mode == MachineMode.Automatic
                //    && !missionsDataProvider.GetAllActiveMissions().Any()
                //    )
                //{
                //    var machineModeProvider = scope.ServiceProvider.GetRequiredService<IMachineModeProvider>();
                //    machineModeProvider.RequestChange(MachineMode.Manual);
                //}

                var rotationClassScheduleProvider = scope.ServiceProvider.GetRequiredService<IRotationClassScheduleProvider>();
                rotationClassScheduleProvider.SetRotationClass();
                this.firstCleanupExecuted = true;
            }

            // actions executed every hour
            this.servicingProvider.UpdateServiceStatus();
        }

        private void OnTimePeriodElapsed2(object state)
        {
            switch((BayNumber)state)
            {
                case BayNumber.BayOne:
                    this.DelayAfterPut1.Change(-1, -1);
                    break;
                case BayNumber.BayTwo:
                    this.DelayAfterPut2.Change(-1, -1);
                    break;
                case BayNumber.BayThree:
                    this.DelayAfterPut3.Change(-1, -1);
                    break;
            }
            this.isDelayFinish[(BayNumber)state] = true;
            this.Logger.LogDebug($"delay timer {(BayNumber)state}");

            var notificationMessage = new NotificationMessage(
                null,
                $"New machine mission available for bay {BayNumber.BayOne}.",
                MessageActor.MissionManager,
                MessageActor.MissionManager,
                MessageType.NewMachineMissionAvailable,
                BayNumber.BayOne);

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
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
                        this.Logger.LogDebug($"Schedule mission restore on bay {bay.Number}");
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
            var machineProvider = serviceProvider.GetRequiredService<IMachineProvider>();

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
                        this.isDelayFinish[bayNumber] = false;

                        serviceProvider
                            .GetRequiredService<IBaysDataProvider>()
                            .AssignMission(mission.TargetBay, mission);

                        this.NotifyAssignedMissionChanged(mission.TargetBay, mission.Id);
                    }
                    else if (mission.Status is MissionStatus.Waiting && mission.Step == MissionStep.BayChain)
                    {
                        var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();
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
                else if (mission.Status is MissionStatus.New || mission.Status is MissionStatus.Waiting)
                {
                    if (!this.isDelayFinish[bayNumber]
                        && wmsMission.Operations.Any(o => o.Type == WMS.Data.WebAPI.Contracts.MissionOperationType.Put))
                    {
                        this.DelayTimeout = this.wmsSettingsProvider.DelayTimeout;
                        this.Logger.LogDebug($"Mission closed for load unit {mission.LoadUnitId}: wait {this.DelayTimeout}ms for WMS to send an update after put operation in bay {bayNumber}");
                        switch (bayNumber)
                        {
                            case BayNumber.BayOne:
                                this.DelayAfterPut1.Change(this.DelayTimeout, -1);
                                break;
                            case BayNumber.BayTwo:
                                this.DelayAfterPut2.Change(this.DelayTimeout, -1);
                                break;
                            case BayNumber.BayThree:
                                this.DelayAfterPut3.Change(this.DelayTimeout, -1);
                                break;
                        }
                    }
                    else
                    {
                        // wms mission is finished
                        mission.Status = MissionStatus.Completed;
                        mission.StepTime = DateTime.UtcNow;
                        missionsDataProvider.Update(mission);
                        machineProvider.UpdateMissionTime(mission.MissionTime);
                        baysDataProvider.ClearMission(bayNumber);

                        this.Logger.LogInformation("Bay {bayNumber}: WMS mission {missionId} completed and move back from bay load unit {LoadUnitId}.", bayNumber, mission.WmsId.Value, mission.LoadUnitId);
                        var missionSchedulingProvider = serviceProvider.GetRequiredService<IMissionSchedulingProvider>();
                        missionSchedulingProvider.QueueRecallMission(mission.LoadUnitId, bayNumber, MissionType.IN);

                        this.isDelayFinish[bayNumber] = false;
                    }
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
