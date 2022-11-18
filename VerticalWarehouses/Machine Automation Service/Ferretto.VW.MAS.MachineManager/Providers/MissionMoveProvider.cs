using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.MissionMove;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal class MissionMoveProvider : IMissionMoveProvider
    {
        #region Fields

        private static readonly IDictionary<MissionStep, ConstructorInfo> cacheStates = new Dictionary<MissionStep, ConstructorInfo>();

        private readonly IEventAggregator eventAggregator;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        #endregion

        #region Constructors

        public MissionMoveProvider(
            IEventAggregator eventAggregator,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            ILogger<MachineManagerService> logger,
            IMachineVolatileDataProvider machineVolatileDataProvider
            )
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }
            this.eventAggregator = eventAggregator;
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
        }

        #endregion

        #region Properties

        public ILogger<MachineManagerService> Logger { get; }

        #endregion

        #region Methods

        // not used at the moment, leaved for future use
        public void OnCommand(CommandMessage message, IServiceProvider serviceProvider)
        {
            //var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            //var missions = missionsDataProvider.GetAllActiveMissions();
            //foreach (var mission in missions)
            //{
            //    switch (mission.FsmStateName)
            //    {
            //        case nameof(MissionMoveStartState):
            //            var startState = new MissionMoveStartState(mission, serviceProvider, this.eventAggregator);
            //            startState.OnCommand(message);
            //            break;
            //    }
            //}
        }

        public void OnNotification(NotificationMessage message, IServiceProvider serviceProvider)
        {
            if (!this.loadingUnitMovementProvider.FilterNotifications(message, MessageActor.MachineManager))
            {
                return;
            }
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var missions = missionsDataProvider.GetAllActiveMissions().Where(x => x.Status != MissionStatus.New);
            if (missions.Any())
            {
                foreach (var mission in missions)
                {
                    var state = GetStateByClassName(serviceProvider, mission, this.eventAggregator);

                    if (state != null)
                    {
                        try
                        {
                            state.OnNotification(message);
                        }
                        catch (StateMachineException ex)
                        {
                            this.Logger.LogError(ex.NotificationMessage.Description, "State notification error.");
                            //this.eventAggregator.GetEvent<NotificationEvent>().Publish(ex.NotificationMessage);

                            state.OnStop(StopRequestReason.Error);
                        }
                    }
                    else
                    {
                        this.Logger.LogError($"Error while processing a notification: GetStateByClassName {mission.Step} not found");
                    }
                }
            }
            missionsDataProvider.CheckPendingChanges();
        }

        public bool ResumeMission(int missionId, CommandMessage command, IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var mission = missionsDataProvider.GetById(missionId);
            var state = GetStateByClassName(serviceProvider, mission, this.eventAggregator);
            if (state != null)
            {
                try
                {
                    state.OnResume(command);
                }
                catch (StateMachineException ex)
                {
                    this.Logger.LogError(ex.NotificationMessage.Description, "Error while resuming a State.");
                    //this.eventAggregator.GetEvent<NotificationEvent>().Publish(ex.NotificationMessage);

                    state.OnStop(StopRequestReason.Error);
                    return false;
                }
            }
            else
            {
                this.Logger.LogError($"Error while resuming a State: GetStateByClassName {mission.Step} not found");
                return false;
            }

            return true;
        }

        public bool StartMission(Mission mission, CommandMessage command, IServiceProvider serviceProvider, bool showErrors)
        {
            var newState = new MissionMoveNewStep(mission, serviceProvider, this.eventAggregator);

            try
            {
                return newState.OnEnter(command, showErrors);
            }
            catch (StateMachineException ex)
            {
                this.Logger.LogError(ex.NotificationMessage.Description, "Error while starting a mission.");
                //this.eventAggregator.GetEvent<NotificationEvent>().Publish(ex.NotificationMessage);

                newState.OnStop(StopRequestReason.Stop);
                return false;
            }
        }

        public bool StopMission(int missionId, StopRequestReason stopRequest, IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var mission = missionsDataProvider.GetById(missionId);
            if (mission != null)
            {
                if (mission.Status == MissionStatus.Completed
                    || mission.Status == MissionStatus.Aborted
                    || mission.Status == MissionStatus.New
                    )
                {
                    missionsDataProvider.Complete(missionId);
                }
                else
                {
                    var state = GetStateByClassName(serviceProvider, mission, this.eventAggregator);
                    if (state != null)
                    {
                        state.OnStop(stopRequest);
                    }
                }
            }

            return true;
        }

        public bool TryCreateMachineMission(CommandMessage command, IServiceProvider serviceProvider, out Mission mission)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            mission = null;

            if (command.Data is IMoveLoadingUnitMessageData messageData
                && messageData.LoadUnitId.HasValue
                )
            {
                var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                if (missionsDataProvider.CanCreateMission(messageData.LoadUnitId.Value, command.RequestingBay))
                {
                    var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                    mission = missionsDataProvider.CreateBayMission(messageData.LoadUnitId.Value, command.RequestingBay, messageData.MissionType);
                    if (mission != null
                        && this.UpdateWaitingMission(missionsDataProvider, baysDataProvider, mission))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool UpdateWaitingMission(IMissionsDataProvider missionsDataProvider, IBaysDataProvider baysDataProvider, Mission mission)
        {
            this.Logger.LogDebug($"Check the waiting missions... Id={mission.Id}, Load Unit {mission.LoadUnitId}, status {mission.Status}, step {mission.Step}");

            // if there is a new or waiting mission we have to take her place
            var waitMission = missionsDataProvider.GetAllMissions()
                .FirstOrDefault(m => m.LoadUnitId == mission.LoadUnitId
                    && m.Id != mission.Id
                    && ((m.Status == MissionStatus.Waiting && m.Step == MissionStep.WaitPick && m.Step != MissionStep.WaitDepositExternalBay)
                        || m.Status == MissionStatus.New && m.MissionType != MissionType.WMS && m.MissionType != MissionType.OUT)
                );

            if (waitMission != null)
            {
                try
                {
                    if (baysDataProvider.IsMissionInBay(waitMission))
                    {
                        baysDataProvider.ClearMission(mission.TargetBay);
                    }

                    if (waitMission.NeedHomingAxis != Axis.None
                        && (waitMission.Status == MissionStatus.Waiting
                            || waitMission.Status == MissionStatus.New
                            )
                        )
                    {
                        mission.NeedHomingAxis = waitMission.NeedHomingAxis;

                        // Update need homing axis parameter only if vertical & horizontal homing has been executed
                        if (waitMission.NeedHomingAxis == Axis.HorizontalAndVertical &&
                            this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay])
                        {
                            mission.NeedHomingAxis = Axis.None;
                        }
                    }

                    if (waitMission.WmsId.HasValue)
                    {
                        missionsDataProvider.Complete(waitMission.Id);
                        this.Logger.LogDebug($"{this.GetType().Name}: Complete {waitMission}");
                    }
                    else
                    {
                        missionsDataProvider.Delete(waitMission.Id);
                        this.Logger.LogDebug($"{this.GetType().Name}: Delete {waitMission}");
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex.Message);
                    return false;
                }
            }
            else
            {
                if (baysDataProvider.IsLoadUnitInBay(mission.TargetBay, mission.LoadUnitId))
                {
                    baysDataProvider.ClearMission(mission.TargetBay);
                }
            }
            return true;
        }

        public bool UpdateWaitingMissionToDoubleBay(IServiceProvider serviceProvider, IMissionsDataProvider missionsDataProvider, IBaysDataProvider baysDataProvider, Mission mission)
        {
            this.Logger.LogDebug($"Check the waiting missions on double bay...");

            // Check the type of bay and apply the waiting routine only with double bay
            var bay = baysDataProvider.GetByNumberPositions(mission.TargetBay);
            if (!bay.IsDouble || (bay.IsDouble && bay.IsExternal))
            {
                // No mission to check
                this.Logger.LogTrace($"Bay is NOT double... No check waiting missions");
                return false;
            }

            if (mission.Step == MissionStep.WaitDepositBay)
            {
                this.Logger.LogDebug($"Mission {mission.Id} already waiting for resume");
                return true;
            }

            // Retrieve the mission on the bay with Step = MissionStep.WaitDepositInBay
            var waitMissionOnCurrentBay = missionsDataProvider.GetAllActiveMissions()
                .FirstOrDefault(
                    m => m.LoadUnitId != mission.LoadUnitId &&
                    m.Id != mission.Id &&
                    m.Step == MissionStep.WaitDepositBay
                );
            if (waitMissionOnCurrentBay != null)
            {
                this.Logger.LogDebug($"Resume mission: MissionId:{waitMissionOnCurrentBay.Id} for loading unit {waitMissionOnCurrentBay.LoadUnitId} on location:{waitMissionOnCurrentBay.LoadUnitDestination}");

                var moveLoadUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();
                // Resume the mission on waiting
                moveLoadUnitProvider.ResumeMoveLoadUnit(
                    waitMissionOnCurrentBay.Id,
                    waitMissionOnCurrentBay.LoadUnitSource,
                    waitMissionOnCurrentBay.LoadUnitDestination,
                    waitMissionOnCurrentBay.TargetBay,
                    waitMissionOnCurrentBay.WmsId,
                    waitMissionOnCurrentBay.MissionType,
                    MessageActor.MachineManager);

                // Wait the completion of waitMissionOnCurrentBay, not go ahead
                return true;
            }

            // No mission to deposit into bay, go ahead
            this.Logger.LogDebug($"No missions are waiting in double bay. Go ahead...");
            return false;
        }

        private static IMissionMoveBase GetStateByClassName(IServiceProvider serviceProvider, Mission mission, IEventAggregator eventAggregator)
        {
            ConstructorInfo ctor = null;
            lock (cacheStates)
            {
                if (cacheStates.ContainsKey(mission.Step))
                {
                    ctor = cacheStates[mission.Step];
                }
                else
                {
                    var ns = typeof(MissionMoveBase).Namespace;
                    var type = Type.GetType(string.Concat(ns, ".MissionMove", mission.Step.ToString(), "Step"));

                    ctor = type?.GetConstructor(new[] { typeof(Mission), typeof(IServiceProvider), typeof(IEventAggregator) });
                    cacheStates[mission.Step] = ctor;
                }
            }
            return (IMissionMoveBase)ctor?.Invoke(new object[] { mission, serviceProvider, eventAggregator });
        }

        #endregion
    }
}
