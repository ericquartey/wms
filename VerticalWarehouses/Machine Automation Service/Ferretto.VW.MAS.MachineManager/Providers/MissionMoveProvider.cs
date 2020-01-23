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

        private readonly object syncObject = new object();

        #endregion

        #region Constructors

        public MissionMoveProvider(
                    IEventAggregator eventAggregator,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            ILogger<MachineManagerService> logger
            )
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }
            this.eventAggregator = eventAggregator;
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
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
            lock (this.syncObject)
            {
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
                                this.Logger.LogError(ex.NotificationMessage.Description, "Error while activating a State.");
                                //this.eventAggregator.GetEvent<NotificationEvent>().Publish(ex.NotificationMessage);

                                state.OnStop(StopRequestReason.Error);
                            }
                        }
                    }
                }
            }
        }

        public bool ResumeMission(int missionId, CommandMessage command, IServiceProvider serviceProvider)
        {
            lock (this.syncObject)
            {
                var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                var mission = missionsDataProvider.GetById(missionId);
                if (mission != null)
                {
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
                        }
                    }
                }

                return true;
            }
        }

        public bool StartMission(Mission mission, CommandMessage command, IServiceProvider serviceProvider, bool showErrors)
        {
            lock (this.syncObject)
            {
                var newState = new MissionMoveNewStep(mission, serviceProvider, this.eventAggregator);

                try
                {
                    return newState.OnEnter(command, showErrors);
                }
                catch (StateMachineException ex)
                {
                    this.Logger.LogError(ex.NotificationMessage.Description, "Error while activating a State.");
                    //this.eventAggregator.GetEvent<NotificationEvent>().Publish(ex.NotificationMessage);

                    newState.OnStop(StopRequestReason.Error);
                    return false;
                }
            }
        }

        public bool StopMission(int missionId, StopRequestReason stopRequest, IServiceProvider serviceProvider)
        {
            lock (this.syncObject)
            {
                var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                var mission = missionsDataProvider.GetById(missionId);
                if (mission != null)
                {
                    if (mission.Status == MissionStatus.Completed)
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
        }

        public bool TestMission(int missionId, CommandMessage command, IServiceProvider serviceProvider)
        {
            lock (this.syncObject)
            {
                var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                var mission = missionsDataProvider.GetById(missionId);
                if (mission != null)
                {
                    var state = GetStateByClassName(serviceProvider, mission, this.eventAggregator);
                    if (state != null)
                    {
                        state.OnResume(command);
                    }
                }

                return true;
            }
        }

        public bool TryCreateMachineMission(CommandMessage command, IServiceProvider serviceProvider, out Mission mission)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            lock (this.syncObject)
            {
                mission = null;

                if (command.Data is IMoveLoadingUnitMessageData messageData
                    && messageData.LoadUnitId.HasValue
                    )
                {
                    var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                    if (missionsDataProvider.CanCreateMission(messageData.LoadUnitId.Value, command.RequestingBay))
                    {
                        var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                        mission = missionsDataProvider.CreateBayMission(messageData.LoadUnitId.Value, command.RequestingBay, MissionType.Manual);
                        if (this.UpdateWaitingMission(missionsDataProvider, baysDataProvider, mission))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool UpdateWaitingMission(IMissionsDataProvider missionsDataProvider, IBaysDataProvider baysDataProvider, Mission mission)
        {
            // if there is a new or waiting mission we have to take her place
            var waitMission = missionsDataProvider.GetAllMissions()
                .FirstOrDefault(m => m.LoadUnitId == mission.LoadUnitId
                    && m.Id != mission.Id
                    && !m.WmsId.HasValue
                    && (m.Status == MissionStatus.Waiting || m.Status == MissionStatus.New || m.Status == MissionStatus.Completed)
                );
            if (waitMission != null)
            {
                //using (var transaction = missionsDataProvider.GetContextTransaction())
                {
                    try
                    {
                        if (baysDataProvider.IsMissionInBay(waitMission))
                        {
                            baysDataProvider.AssignMission(mission.TargetBay, mission);
                        }

                        missionsDataProvider.Delete(waitMission.Id);
                        this.Logger.LogDebug($"{this.GetType().Name}: Delete {waitMission}");
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                    //transaction.Commit();
                }
            }
            return true;
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
