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
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal class MissionMoveProvider : IMissionMoveProvider
    {
        #region Fields

        private static readonly IDictionary<string, ConstructorInfo> cacheStates = new Dictionary<string, ConstructorInfo>();

        private readonly IEventAggregator eventAggregator;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

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
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var missions = missionsDataProvider.GetAllActiveMissions();
            if (missions.Any())
            {
                foreach (var mission in missions)
                {
                    var state = GetStateByClassName(serviceProvider, mission, this.eventAggregator);

                    if (state != null)
                    {
                        state.OnNotification(message);
                    }
                }
            }
        }

        public bool ResumeMission(Guid missionId, CommandMessage command, IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var mission = missionsDataProvider.GetByGuid(missionId);
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

        public bool StartMission(Mission mission, CommandMessage command, IServiceProvider serviceProvider)
        {
            var newState = new MissionMoveNewState(mission, serviceProvider, this.eventAggregator);

            return newState.OnEnter(command);
        }

        public bool StopMission(Guid missionId, StopRequestReason stopRequest, IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var mission = missionsDataProvider.GetByGuid(missionId);
            if (mission != null)
            {
                var state = GetStateByClassName(serviceProvider, mission, this.eventAggregator);
                if (state != null)
                {
                    state.OnStop(stopRequest);
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
                && messageData.LoadingUnitId.HasValue
                )
            {
                var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
                if (missionsDataProvider.CanCreateMission(messageData.LoadingUnitId.Value, command.RequestingBay))
                {
                    // if there is a mission waiting we have to take her place
                    var waitMission = missionsDataProvider.GetAllExecutingMissions().FirstOrDefault(m =>
                        m.LoadingUnitId == messageData.LoadingUnitId.Value
                        && m.Status == MissionStatus.Waiting
                        );
                    if (waitMission != null)
                    {
                        try
                        {
                            missionsDataProvider.Delete(waitMission.Id);
                            this.Logger.LogDebug($"{this.GetType().Name}: Delete {waitMission}");
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }

                    mission = missionsDataProvider.CreateBayMission(messageData.LoadingUnitId.Value, command.RequestingBay);
                    return true;
                }
            }

            return false;
        }

        private static IMissionMoveBase GetStateByClassName(IServiceProvider serviceProvider, Mission mission, IEventAggregator eventAggregator)
        {
            ConstructorInfo ctor = null;
            lock (cacheStates)
            {
                if (cacheStates.ContainsKey(mission.FsmStateName))
                {
                    ctor = cacheStates[mission.FsmStateName];
                }
                else
                {
                    var ns = typeof(MissionMoveBase).Namespace;
                    var type = Type.GetType(string.Concat(ns, ".", mission.FsmStateName));

                    ctor = type?.GetConstructor(new[] { typeof(Mission), typeof(IServiceProvider), typeof(IEventAggregator) });
                    cacheStates[mission.FsmStateName] = ctor;
                }
            }
            return (IMissionMoveBase)ctor?.Invoke(new object[] { mission, serviceProvider, eventAggregator });
        }

        #endregion
    }
}
