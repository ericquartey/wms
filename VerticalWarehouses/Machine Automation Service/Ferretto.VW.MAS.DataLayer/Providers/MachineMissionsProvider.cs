using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Data;
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
    public class MachineMissionsProvider : IMachineMissionsProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly List<IMission> machineMissions;

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public MachineMissionsProvider(
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.eventAggregator = eventAggregator;
            this.serviceScopeFactory = serviceScopeFactory;
            this.machineMissions = new List<IMission>();
        }

        #endregion

        #region Methods

        public bool AbortMachineMission(Guid missionId)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.Id.Equals(missionId));
            if (mission is null)
            {
                return false;
            }

            mission.AbortMachineMission();
            return true;
        }

        public IMission GetMissionById(Guid missionId)
        {
            return this.machineMissions.FirstOrDefault(m => m.Id == missionId);
        }

        public List<IMission> GetMissionsByType(FSMType type)
        {
            return this.machineMissions.Where(m => m.Type == type).ToList();
        }

        public bool PauseMachineMission(Guid missionId)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.Id.Equals(missionId));
            if (mission is null)
            {
                return false;
            }

            mission.PauseMachineMission();
            return true;
        }

        public bool ResumeMachineMission(Guid missionId)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.Id.Equals(missionId));
            if (mission is null)
            {
                return false;
            }

            mission.ResumeMachineMission();
            return true;
        }

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

        public bool TryCreateMachineMission(FSMType missionType, CommandMessage command, out Guid missionId)
        {
            missionId = Guid.Empty;

            if (this.CanCreateStateMachine(missionType, command))
            {
                IMission newMission = null;
                switch (missionType)
                {
                    case FSMType.ChangeRunningType:
                        newMission = new MachineMission<IChangeRunningStateStateMachine>(this.serviceScopeFactory, this.OnActiveStateMachineCompleted);
                        break;

                    case FSMType.MoveLoadingUnit:
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

        public bool TryCreateMachineMission(FSMType missionType, MoveLoadingUnitMessageData command, BayNumber bayNumber, out Guid missionId)
        {
            missionId = Guid.Empty;
            var returnValue = false;
            // TODO: exclude missions with the same WmsId or the same Bay

            //if (command.WmsId.HasValue)
            //{
            //    returnValue = !this.machineMissions.Where(m => m.Type == missionType).Select(x => x.GetCurrent() as MoveLoadingUnitMessageData).Any(w => w.WmsId.HasValue && w.WmsId == command.WmsId);
            //    var miss = this.machineMissions.Where(m => m.Type == FSMType.MoveLoadingUnit).Select(x => x.GetCurrent() as IFiniteStateMachine);
            //}
            //else
            //{
            //    returnValue = !this.machineMissions.Where(m => m.Type == missionType).Select(x => x as MoveLoadingUnitMessageData).Any(w => w.TargetBay == bayNumber);
            //}
            if (returnValue)
            {
                IMission newMission = new MachineMission<IMoveLoadingUnitStateMachine>(this.serviceScopeFactory, this.OnActiveStateMachineCompleted);
                this.machineMissions.Add(newMission);
                missionId = newMission.Id;
            }
            return returnValue;
        }

        /// <summary>
        /// Handles logic for deciding if a specific mission type can be created or not. Mostly based on related finite state machine type, instances and statuses.
        /// </summary>
        /// <param name="requestedMission">TYpe of mission to be created</param>
        /// <param name="command">Command received to create mission. Provides information useful to decide id mission two or more missions of the same type are allowed or not</param>
        /// <returns>True if the mission type can be created, false otherwise</returns>
        private bool CanCreateStateMachine(FSMType requestedMission, CommandMessage command)
        {
            var returnValue = this.machineMissions.All(m => m.Type != requestedMission || m.AllowMultipleInstances(command));

            if (returnValue)
            {
                returnValue = this.EvaluateMissionPolicies(requestedMission, command);
            }

            return returnValue;
        }

        private bool EvaluateMissionPolicies(FSMType moveRequestedMission, CommandMessage command)
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

                if (eventArgs.NotificationMessage != null)
                {
                    this.eventAggregator.GetEvent<NotificationEvent>().Publish(eventArgs.NotificationMessage);
                }
            }
        }

        #endregion
    }
}
