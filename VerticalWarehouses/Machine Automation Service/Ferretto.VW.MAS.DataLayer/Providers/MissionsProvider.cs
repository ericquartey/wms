using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Missions;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public class MissionsProvider : IMissionsProvider
    {

        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly List<MachineMission> machineMissions;

        private readonly IServiceScope serviceScope;

        #endregion

        #region Constructors

        public MissionsProvider(
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.eventAggregator = eventAggregator;
            this.serviceScope = serviceScopeFactory.CreateScope();
            this.machineMissions = new List<MachineMission>();
        }

        #endregion



        #region Methods

        public bool StartMachineMission(Guid missionId, CommandMessage command)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.Id.Equals(missionId));
            if (mission is null)
            {
                return false;
            }

            mission.StartMachine(command);

            return true;
        }

        public bool StopMachineMission(Guid missionId, StopRequestReason reason)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.Id.Equals(missionId));
            if (mission is null)
            {
                return false;
            }

            mission.StopMachine(reason);

            return true;
        }

        public bool TryCreateMachineMission(MissionType missionType, out Guid missionId)
        {
            missionId = Guid.Empty;

            if (this.CanCreateStateMachine(missionType))
            {
                MachineMission newMission = null;

                switch (missionType)
                {
                    case MissionType.ChangeRunningType:
                        newMission = new MachineMission(this.serviceScope.ServiceProvider.GetRequiredService<IChangeRunningStateStateMachine>(), this.OnActiveStateMachineCompleted);
                        break;
                }

                if (newMission != null)
                {
                    this.machineMissions.Add(newMission);

                    missionId = newMission.Id;
                }
            }

            return true;
        }

        /// <summary>
        /// Handles logic for deciding if a specific mission type can be created or not. Mostly based on related finite state machine type, instances and statuses
        /// </summary>
        /// <param name="requestedMission">TYpe of mission to be created</param>
        /// <returns>True if the mission type can be created, false otherwise</returns>
        private bool CanCreateStateMachine(MissionType requestedMission)
        {
            bool returnValue = true;
            switch (requestedMission)
            {
                case MissionType.ChangeRunningType:
                    returnValue = !this.machineMissions.Any(mm => mm.MissionMachine is IChangeRunningStateStateMachine _);
                    break;
            }

            return returnValue;
        }

        private void OnActiveStateMachineCompleted(object sender, FiniteStateMachinesEventArgs eventArgs)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.Id.Equals(eventArgs.InstanceId));
            if (mission != null)
            {

                mission.EndMachine();

                this.machineMissions.Remove(mission);

                mission.MissionMachine.Completed -= this.OnActiveStateMachineCompleted;

                mission.Dispose();

                this.eventAggregator.GetEvent<NotificationEvent>().Publish(eventArgs.NotificationMessage);
            }
        }

        #endregion
    }
}
