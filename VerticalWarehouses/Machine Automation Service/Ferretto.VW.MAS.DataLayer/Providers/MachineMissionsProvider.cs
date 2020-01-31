using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
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

        private readonly IMachineMissionsVolatileProvider machineMissionsVolatileProvider;

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public MachineMissionsProvider(
            IMachineMissionsVolatileProvider machineMissionsVolatileProvider,
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.eventAggregator = eventAggregator;
            this.serviceScopeFactory = serviceScopeFactory;

            this.machineMissionsVolatileProvider = machineMissionsVolatileProvider ?? throw new ArgumentNullException(nameof(machineMissionsVolatileProvider));
        }

        #endregion

        #region Properties

        private List<IMission> machineMissions => this.machineMissionsVolatileProvider.MachineMissions;

        #endregion

        #region Methods

        public bool AbortMachineMission(Guid fsmId)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.FsmId.Equals(fsmId));
            if (mission is null)
            {
                return false;
            }

            mission.AbortMachineMission();
            return true;
        }

        public IMission GetMissionById(Guid fsmId)
        {
            return this.machineMissions.FirstOrDefault(m => m.FsmId == fsmId);
        }

        public List<IMission> GetMissionsByFsmType(FsmType fsmType)
        {
            return this.machineMissions.Where(m => m.Type == fsmType).ToList();
        }

        public IEnumerable<IMission> GetMissionsByType(FsmType fsmType, MissionType type)
        {
            return this.machineMissions.Where(m => (m.Type == fsmType)
                    && (m.MachineData is IMoveLoadingUnitMachineData data)
                    && data.MissionType == type);
        }

        public bool PauseMachineMission(Guid fsmId)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.FsmId.Equals(fsmId));
            if (mission is null)
            {
                return false;
            }

            mission.PauseMachineMission();
            return true;
        }

        public bool ResumeMachineMission(Guid fsmId, CommandMessage command)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.FsmId.Equals(fsmId));
            if (mission is null)
            {
                return false;
            }

            mission.ResumeMachineMission(command);
            return true;
        }

        public bool StartMachineMission(Guid fsmId, CommandMessage command)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.FsmId.Equals(fsmId));
            if (mission is null)
            {
                return false;
            }

            mission.StartMachine(command);
            return true;
        }

        public bool StopMachineMission(Guid fsmId, StopRequestReason reason)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.FsmId.Equals(fsmId));
            if (mission is null)
            {
                return false;
            }

            mission.StopMachine(reason);

            return true;
        }

        public bool TryCreateMachineMission(FsmType fsmType, CommandMessage command, out Guid missionId)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            missionId = Guid.Empty;

            switch (fsmType)
            {
                case FsmType.ChangeRunningType:
                    if (this.CanCreateStateMachine(fsmType, command))
                    {
                        IMission newMission = null;
                        newMission = new MachineMission<IChangeRunningStateStateMachine>(this.serviceScopeFactory, this.OnActiveStateMachineCompleted);

                        if (newMission != null)
                        {
                            if (newMission.MachineData is DataModels.Mission mission)
                            {
                                mission.MissionType = MissionType.Manual;
                                mission.TargetBay = command.RequestingBay;
                                mission.CreationDate = DateTime.Now;
                            }
                            this.machineMissions.Add(newMission);
                            missionId = newMission.FsmId;

                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;
            }

            return false;
        }

        private static bool EvaluateMissionPolicies(FsmType moveRequestedMission, CommandMessage command, IServiceProvider serviceProvider)
        {
            var returnValue = true;
            return returnValue;
        }

        /// <summary>
        /// Handles logic for deciding if a specific mission type can be created or not. Mostly based on related finite state machine type, instances and statuses.
        /// </summary>
        /// <param name="requestedMission">TYpe of mission to be created</param>
        /// <param name="command">Command received to create mission. Provides information useful to decide id mission two or more missions of the same type are allowed or not</param>
        /// <returns>True if the mission type can be created, false otherwise</returns>
        private bool CanCreateStateMachine(FsmType requestedMission, CommandMessage command)
        {
            var serviceScope = this.serviceScopeFactory.CreateScope();

            var returnValue = this.machineMissions.All(m => m.Type != requestedMission || m.AllowMultipleInstances(command));

            if (returnValue)
            {
                returnValue = EvaluateMissionPolicies(requestedMission, command, serviceScope.ServiceProvider);
            }
            //else
            //{
            //    var errorProvider = serviceScope.ServiceProvider.GetRequiredService<IErrorsProvider>();
            //    errorProvider.RecordNew(MachineErrorCode.AnotherMissionOfThisTypeIsActive);
            //}

            return returnValue;
        }

        private void OnActiveStateMachineCompleted(object sender, FiniteStateMachinesEventArgs eventArgs)
        {
            var mission = this.machineMissions.FirstOrDefault(mm => mm.FsmId.Equals(eventArgs.InstanceId));

            Debug.Assert(mission != null);

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
