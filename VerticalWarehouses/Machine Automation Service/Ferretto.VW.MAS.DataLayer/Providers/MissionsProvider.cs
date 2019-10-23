﻿using System;
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

        private readonly List<IMission> machineMissions;

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public MissionsProvider(
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.eventAggregator = eventAggregator;
            this.serviceScopeFactory = serviceScopeFactory;
            this.machineMissions = new List<IMission>();
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

        public bool TryCreateMachineMission(MissionType missionType, CommandMessage command, out Guid missionId)
        {
            missionId = Guid.Empty;

            if (this.CanCreateStateMachine(missionType, command))
            {
                IMission newMission = null;
                switch (missionType)
                {
                    case MissionType.ChangeRunningType:
                        newMission = new MachineMission<IChangeRunningStateStateMachine>(this.serviceScopeFactory, this.OnActiveStateMachineCompleted);
                        break;

                    case MissionType.MoveLoadingUnit:
                        newMission = new MachineMission<IMoveLoadingUnitStateMachine>(this.serviceScopeFactory, this.OnActiveStateMachineCompleted);
                        break;
                }

                if (newMission != null)
                {
                    this.machineMissions.Add(newMission);

                    missionId = newMission.Id;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles logic for deciding if a specific mission type can be created or not. Mostly based on related finite state machine type, instances and statuses.
        /// </summary>
        /// <param name="requestedMission">TYpe of mission to be created</param>
        /// <param name="command">Command received to create mission. Provides information useful to decide id mission two or more missions of the same type are allowed or not</param>
        /// <returns>True if the mission type can be created, false otherwise</returns>
        private bool CanCreateStateMachine(MissionType requestedMission, CommandMessage command)
        {
            return this.machineMissions.All(m => m.Type != requestedMission || m.AllowMultipleInstances(command));
            /*
            var returnValue = true;

            if (this.machineMissions.Any(mm => mm.Type == requestedMission))
            {
                returnValue = this.machineMissions.All(mm => mm.AllowMultipleInstances(command));
            }

            if (returnValue)
            {
                returnValue = this.EvaluateMissionPolicies(requestedMission, command);
            }

            return returnValue;
            */
        }

        private bool EvaluateMissionPolicies(MissionType moveRequestedMission, CommandMessage command)
        {
            return true;
        }

        private void OnActiveStateMachineCompleted(object sender, FiniteStateMachinesEventArgs eventArgs)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.Id.Equals(eventArgs.InstanceId));
            if (mission != null)
            {
                mission.EndMachine();

                this.machineMissions.Remove(mission);

                mission.RemoveHandler(this.OnActiveStateMachineCompleted);

                mission.Dispose();

                this.eventAggregator.GetEvent<NotificationEvent>().Publish(eventArgs.NotificationMessage);
            }
        }

        #endregion
    }
}
