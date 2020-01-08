using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.MachineManager.MissionMove;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal class MissionMoveProvider : IMissionMoveProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public MissionMoveProvider(
            IEventAggregator eventAggregator,
            ILogger<MachineManagerService> logger
            )
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }
            this.eventAggregator = eventAggregator;
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var missions = missionsDataProvider.GetAllActiveMissions();
            if (missions.Any())
            {
                foreach (var mission in missions)
                {
                    switch (mission.FsmStateName)
                    {
                        case nameof(MissionMoveStartState):
                            {
                                var state = new MissionMoveStartState(mission, serviceProvider, this.eventAggregator);
                                state.OnNotification(message);
                                break;
                            }
                        case nameof(MissionMoveBayChainState):
                            {
                                var state = new MissionMoveBayChainState(mission, serviceProvider, this.eventAggregator);
                                state.OnNotification(message);
                                break;
                            }
                        case nameof(MissionMoveCloseShutterState):
                            {
                                var state = new MissionMoveCloseShutterState(mission, serviceProvider, this.eventAggregator);
                                state.OnNotification(message);
                                break;
                            }
                        case nameof(MissionMoveDepositUnitState):
                            {
                                var state = new MissionMoveDepositUnitState(mission, serviceProvider, this.eventAggregator);
                                state.OnNotification(message);
                                break;
                            }
                        case nameof(MissionMoveEndState):
                            {
                                var state = new MissionMoveEndState(mission, serviceProvider, this.eventAggregator);
                                state.OnNotification(message);
                                break;
                            }
                        case nameof(MissionMoveErrorState):
                            {
                                var state = new MissionMoveErrorState(mission, serviceProvider, this.eventAggregator);
                                state.OnNotification(message);
                                break;
                            }
                        case nameof(MissionMoveLoadElevatorState):
                            {
                                var state = new MissionMoveLoadElevatorState(mission, serviceProvider, this.eventAggregator);
                                state.OnNotification(message);
                                break;
                            }
                        case nameof(MissionMoveToTargetState):
                            {
                                var state = new MissionMoveToTargetState(mission, serviceProvider, this.eventAggregator);
                                state.OnNotification(message);
                                break;
                            }
                        case nameof(MissionMoveWaitPickState):
                            {
                                var state = new MissionMoveWaitPickState(mission, serviceProvider, this.eventAggregator);
                                state.OnNotification(message);
                                break;
                            }
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
                switch (mission.FsmStateName)
                {
                    case nameof(MissionMoveBayChainState):
                        {
                            var state = new MissionMoveBayChainState(mission, serviceProvider, this.eventAggregator);
                            state.OnResume(command);
                            break;
                        }
                    case nameof(MissionMoveErrorState):
                        {
                            var state = new MissionMoveErrorState(mission, serviceProvider, this.eventAggregator);
                            state.OnResume(command);
                            break;
                        }
                    case nameof(MissionMoveWaitPickState):
                        {
                            var state = new MissionMoveWaitPickState(mission, serviceProvider, this.eventAggregator);
                            state.OnResume(command);
                            break;
                        }
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
                switch (mission.FsmStateName)
                {
                    case nameof(MissionMoveNewState):
                        {
                            var state = new MissionMoveNewState(mission, serviceProvider, this.eventAggregator);
                            state.OnStop(stopRequest);
                            break;
                        }
                    case nameof(MissionMoveStartState):
                        {
                            var state = new MissionMoveStartState(mission, serviceProvider, this.eventAggregator);
                            state.OnStop(stopRequest);
                            break;
                        }
                    case nameof(MissionMoveBayChainState):
                        {
                            var state = new MissionMoveBayChainState(mission, serviceProvider, this.eventAggregator);
                            state.OnStop(stopRequest);
                            break;
                        }
                    case nameof(MissionMoveCloseShutterState):
                        {
                            var state = new MissionMoveCloseShutterState(mission, serviceProvider, this.eventAggregator);
                            state.OnStop(stopRequest);
                            break;
                        }
                    case nameof(MissionMoveDepositUnitState):
                        {
                            var state = new MissionMoveDepositUnitState(mission, serviceProvider, this.eventAggregator);
                            state.OnStop(stopRequest);
                            break;
                        }
                    case nameof(MissionMoveErrorState):
                        {
                            var state = new MissionMoveErrorState(mission, serviceProvider, this.eventAggregator);
                            state.OnStop(stopRequest);
                            break;
                        }
                    case nameof(MissionMoveLoadElevatorState):
                        {
                            var state = new MissionMoveLoadElevatorState(mission, serviceProvider, this.eventAggregator);
                            state.OnStop(stopRequest);
                            break;
                        }
                    case nameof(MissionMoveToTargetState):
                        {
                            var state = new MissionMoveToTargetState(mission, serviceProvider, this.eventAggregator);
                            state.OnStop(stopRequest);
                            break;
                        }
                    case nameof(MissionMoveWaitPickState):
                        {
                            var state = new MissionMoveWaitPickState(mission, serviceProvider, this.eventAggregator);
                            state.OnStop(stopRequest);
                            break;
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

        #endregion
    }
}
