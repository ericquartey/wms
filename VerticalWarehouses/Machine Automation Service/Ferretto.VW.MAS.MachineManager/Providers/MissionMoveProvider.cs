using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
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

        private readonly NotificationEvent notificationEvent;

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
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        public ILogger<MachineManagerService> Logger { get; }

        #endregion

        #region Methods

        public void OnCommand(CommandMessage message, IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var missions = missionsDataProvider.GetAllActiveMissions();
            foreach (var mission in missions)
            {
                switch (mission.FsmStateName)
                {
                    case nameof(MissionMoveStartState):
                        var startState = new MissionMoveStartState(mission, serviceProvider);
                        startState.OnCommand(message);
                        break;
                }
            }
        }

        public void OnNotification(NotificationMessage message, IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var missions = missionsDataProvider.GetAllActiveMissions();
            foreach (var mission in missions)
            {
                switch (mission.FsmStateName)
                {
                    case nameof(MissionMoveStartState):
                        var startState = new MissionMoveStartState(mission, serviceProvider);
                        startState.OnNotification(message);
                        break;
                }
            }
        }

        public bool Start(int missionId, CommandMessage commandMessage, IServiceProvider serviceProvider)
        {
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var mission = missionsDataProvider.GetById(missionId);
            var newState = new MissionMoveNewState(mission, serviceProvider);

            return newState.OnEnter(commandMessage);
        }

        public bool TryCreateMachineMission(CommandMessage command, IServiceProvider serviceProvider, out int? missionId)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            missionId = null;

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

                    var newMission = missionsDataProvider.CreateBayMission(messageData.LoadingUnitId.Value, command.RequestingBay);
                    missionId = newMission.Id;
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
