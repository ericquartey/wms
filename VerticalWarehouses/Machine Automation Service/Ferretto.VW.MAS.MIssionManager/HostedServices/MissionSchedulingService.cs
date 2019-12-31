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
using Ferretto.VW.MAS.MachineManager;
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

        private readonly WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsWmsWebService;

        private bool dataLayerIsReady;

        #endregion

        #region Constructors

        public MissionSchedulingService(
            IConfiguration configuration,
            WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsWmsWebService,
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.missionsWmsWebService = missionsWmsWebService ?? throw new ArgumentNullException(nameof(missionsWmsWebService));
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
                    moveLoadingUnitProvider.ResumeMoveLoadUnit(missionToRestore.FsmId, LoadingUnitLocation.NoLocation, LoadingUnitLocation.NoLocation, bayNumber, null, MessageActor.MissionManager);
                }
                else
                {
                    this.Logger.LogTrace($"ScheduleMissionsAsync: waiting for mission to restore {missionToRestore.WmsId}, LoadUnit {missionToRestore.LoadingUnitId}; bay {bayNumber}");
                }

                return;
            }

            var mission = activeMissions.FirstOrDefault(x => x.Status == MissionStatus.Executing || x.Status == MissionStatus.Waiting);
            if (mission is null)
            {
                mission = activeMissions.FirstOrDefault(x => x.Status == MissionStatus.New);
                if (mission is null)
                {
                    // no more missions are available for scheduling on this bay
                    this.NotifyAssignedMissionOperationChanged(bayNumber, null, null, activeMissions.Count());
                    return;
                }
            }

            if (!this.configuration.IsWmsEnabled())
            {
                this.Logger.LogTrace("Cannot perform mission scheduling, because WMS is not enabled.");
                return;
            }

            System.Diagnostics.Debug.Assert(mission != null);

            if (!mission.WmsId.HasValue)
            {
                // TODO: we do not handle non-WMS missions for now
                return;
            }

            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
            var wmsMission = await this.missionsWmsWebService.GetByIdAsync(mission.WmsId.Value);
            var newOperations = wmsMission.Operations.Where(o => o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Completed && o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Error);
            if (newOperations.Any())
            {
                if (mission.Status == MissionStatus.New)
                {
                    // activate new mission
                    var cellsProvider = serviceProvider.GetRequiredService<ICellsProvider>();
                    var sourceCell = cellsProvider.GetByLoadingUnitId(mission.LoadingUnitId);
                    if (sourceCell is null)
                    {
                        this.Logger.LogDebug($"Bay {bayNumber}: WMS mission {mission.WmsId} can not start because LoadUnit {mission.LoadingUnitId} is not in a cell.");
                    }
                    else
                    {
                        moveLoadingUnitProvider.ActivateMove(mission.FsmId, mission.MissionType, mission.LoadingUnitId, bayNumber, MessageActor.MissionManager);
                    }
                }
                else if (mission.Status == MissionStatus.Waiting)
                {
                    var position = baysDataProvider.GetPositionByLocation(mission.LoadingUnitDestination);
                    if (!position.IsUpper)
                    {
                        var loadingUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(mission.LoadingUnitId);
                        moveLoadingUnitProvider.ResumeMoveLoadUnit(mission.FsmId, loadingUnitSource, loadingUnitSource, bayNumber, null, MessageActor.MissionManager);
                        return;
                    }
                }
                /******************** MOVE THIS PIECE OF CODE AFTER THE END OF THE MACHINE MISSION*/
                // there are more operations for the same wms mission
                var newOperation = newOperations.OrderBy(o => o.Priority).First();
                this.Logger.LogInformation("Bay {bayNumber}: WMS mission {missionId} has operation {operationId} to execute.", bayNumber, mission.WmsId.Value, newOperation.Id);

                baysDataProvider.AssignWmsMission(bayNumber, mission, newOperation.Id);
                this.NotifyAssignedMissionOperationChanged(bayNumber, wmsMission.Id, newOperation.Id, activeMissions.Count());
                /******************************/
            }
            else
            {
                // wms mission is finished
                this.Logger.LogInformation("Bay {bayNumber}: WMS mission {missionId} completed.", bayNumber, mission.WmsId.Value);

                missionsDataProvider.Complete(mission.Id);
                baysDataProvider.ClearMission(bayNumber);
                this.NotifyAssignedMissionOperationChanged(bayNumber, null, null, activeMissions.Count());

                // check if there are other missions for this LU in this bay
                var nextMission = activeMissions.FirstOrDefault(m =>
                    m.LoadingUnitId == mission.LoadingUnitId
                    &&
                    m.WmsId.HasValue
                    &&
                    m.WmsId != mission.WmsId);

                var loadingUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(mission.LoadingUnitId);

                if (nextMission is null)
                {
                    // send back the LU

                    moveLoadingUnitProvider.ResumeMoveLoadUnit(mission.FsmId, loadingUnitSource, LoadingUnitLocation.Cell, bayNumber, null, MessageActor.MissionManager);
                }
                // else are there other missions for this LU and another bay?
                //{
                // update WmsId in the current machine mission and move to another bay
                //}
                else
                {
                    // close current mission
                    moveLoadingUnitProvider.StopMove(mission.FsmId, bayNumber, bayNumber, MessageActor.MissionManager);

                    // activate new mission
                    moveLoadingUnitProvider.ActivateMove(nextMission.FsmId, nextMission.MissionType, nextMission.LoadingUnitId, bayNumber, MessageActor.MissionManager);
                }
            }
        }

        private static void GetPersistedMissions(IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var machineMissionsProvider = serviceProvider.GetRequiredService<IMachineMissionsProvider>();

            var missions = missionsDataProvider.GetAllExecutingMissions().ToList();
            foreach (var mission in missions)
            {
                if (string.IsNullOrEmpty(mission.FsmRestoreStateName))
                {
                    mission.FsmRestoreStateName = mission.FsmStateName;
                }
                mission.FsmStateName = "MoveLoadingUnitErrorState";
                if (mission.FsmRestoreStateName == "MoveLoadingUnitBayChainState")
                {
                    mission.NeedHomingAxis = Axis.BayChain;
                }
                else if (mission.FsmRestoreStateName == "MoveLoadingUnitLoadElevatorState"
                    || mission.FsmRestoreStateName == "MoveLoadingUnitDepositUnitState"
                    )
                {
                    mission.NeedMovingBackward = true;
                    mission.NeedHomingAxis = Axis.Horizontal;
                }
                else if (mission.FsmRestoreStateName == "MoveLoadingUnitMoveToTargetState")
                {
                    mission.NeedHomingAxis = Axis.Horizontal;
                }
                missionsDataProvider.Update(mission);

                machineMissionsProvider.AddMission(mission, mission.FsmId);
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

                            if (machineProvider.IsHomingExecuted)
                            {
                                machineModeDataProvider.Mode = MachineMode.Automatic;
                                this.Logger.LogInformation($"Machine status switched to {machineModeDataProvider.Mode}");
                            }
                            else
                            {
                                var bays = bayProvider.GetAll();
                                if (bays.Any(x => x.Carousel != null && !x.Carousel.IsHomingExecuted))
                                {
                                    var bayNumber = bays.First(x => x.Carousel != null && !x.Carousel.IsHomingExecuted).Number;
                                    IHomingMessageData homingData = new HomingMessageData(Axis.BayChain, Calibration.FindSensor, null);

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
                                    IHomingMessageData homingData = new HomingMessageData(Axis.HorizontalAndVertical, Calibration.FindSensor, null);

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

                            if (!missionsDataProvider.GetAllActiveMissions().Any())
                            {
                                var machineModeDataProvider = scope.ServiceProvider.GetRequiredService<IMachineModeVolatileDataProvider>();
                                machineModeDataProvider.Mode = MachineMode.Compact;
                                this.Logger.LogInformation($"Machine status switched to {machineModeDataProvider.Mode}");
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

        private async Task OnBayOperationalStatusChangedAsync()
        {
            await this.InvokeSchedulerAsync();
        }

        private async Task OnDataLayerReadyAsync(IServiceProvider serviceProvider)
        {
            GetPersistedMissions(serviceProvider);
            this.dataLayerIsReady = true;
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

                    var modeProvider = scope.ServiceProvider.GetRequiredService<IMachineModeProvider>();

                    if (modeProvider.GetCurrent() == MachineMode.Automatic)
                    {
                        await this.ScheduleMissionsOnBayAsync(bay.Number, scope.ServiceProvider);
                    }
                    else if (modeProvider.GetCurrent() == MachineMode.Compact)
                    {
                        await this.ScheduleCompactingMissionsAsync(scope.ServiceProvider);
                    }
                }
            }
        }

        #endregion
    }
}
