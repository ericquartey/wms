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

        public void AddMission(Mission mission, Guid fsmId)
        {
            if (mission != null
                && this.GetMissionById(fsmId) is null
                )
            {
                var newMission = new MachineMission<IMoveLoadingUnitStateMachine>(this.serviceScopeFactory, this.OnActiveStateMachineCompleted);
                newMission.FsmId = fsmId;
                if (newMission.MachineData is DataModels.Mission data)
                {
                    data.CreationDate = mission.CreationDate;
                    data.DestinationCellId = mission.DestinationCellId;
                    data.FsmId = mission.FsmId;
                    data.FsmRestoreStateName = mission.FsmRestoreStateName;
                    data.FsmStateName = mission.FsmStateName;
                    data.Id = mission.Id;
                    data.LoadingUnitCellSourceId = mission.LoadingUnitCellSourceId;
                    data.LoadingUnitDestination = mission.LoadingUnitDestination;
                    data.LoadingUnitId = mission.LoadingUnitId;
                    data.LoadingUnitSource = mission.LoadingUnitSource;
                    data.NeedHomingAxis = mission.NeedHomingAxis;
                    data.NeedMovingBackward = mission.NeedMovingBackward;
                    data.MissionType = mission.MissionType;
                    data.Priority = mission.Priority;
                    data.RestoreConditions = mission.RestoreConditions;
                    data.Status = mission.Status;
                    data.TargetBay = mission.TargetBay;
                    data.WmsId = mission.WmsId;
                }
                this.machineMissions.Add(newMission);
            }
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

        public bool TryCreateMachineMission(FsmType fsmType, CommandMessage command, out Guid fsmId)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            fsmId = Guid.Empty;

            if (this.CanCreateStateMachine(fsmType, command))
            {
                IMission newMission = null;
                switch (fsmType)
                {
                    case FsmType.ChangeRunningType:
                        newMission = new MachineMission<IChangeRunningStateStateMachine>(this.serviceScopeFactory, this.OnActiveStateMachineCompleted);
                        break;

                    case FsmType.MoveLoadingUnit:
                        if (command.Data is MoveLoadingUnitMessageData messageData)
                        {
                            // if there is a mission waiting we have to take her place
                            var waitMission = this.machineMissions.FirstOrDefault(m =>
                                m.Type == fsmType
                                && ((DataModels.Mission)m.MachineData).MissionType == messageData.MissionType
                                && ((DataModels.Mission)m.MachineData).LoadingUnitId == messageData.LoadingUnitId
                                && ((DataModels.Mission)m.MachineData).Status == MissionStatus.Waiting
                                );
                            if (waitMission != null
                                && waitMission.MachineData is DataModels.Mission waitData)
                            {
                                try
                                {
                                    this.StopMachineMission(waitData.FsmId, StopRequestReason.Stop);
                                }
                                catch (Exception)
                                {
                                    return false;
                                }
                            }
                        }
                        newMission = new MachineMission<IMoveLoadingUnitStateMachine>(this.serviceScopeFactory, this.OnActiveStateMachineCompleted);
                        break;
                }

                if (newMission != null)
                {
                    if (newMission.MachineData is DataModels.Mission mission)
                    {
                        mission.MissionType = MissionType.Manual;
                        mission.TargetBay = command.RequestingBay;
                        mission.CreationDate = DateTime.Now;
                    }
                    this.machineMissions.Add(newMission);
                    fsmId = newMission.FsmId;

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
        private bool CanCreateStateMachine(FsmType requestedMission, CommandMessage command)
        {
            var serviceScope = this.serviceScopeFactory.CreateScope();

            var returnValue = this.machineMissions.All(m => m.Type != requestedMission || m.AllowMultipleInstances(command));

            if (returnValue)
            {
                returnValue = this.EvaluateMissionPolicies(requestedMission, command, serviceScope.ServiceProvider);
            }
            else
            {
                var errorProvider = serviceScope.ServiceProvider.GetRequiredService<IErrorsProvider>();
                errorProvider.RecordNew(MachineErrorCode.AnotherMissionOfThisTypeIsActive);
            }

            return returnValue;
        }

        private bool EvaluateMissionPolicies(FsmType moveRequestedMission, CommandMessage command, IServiceProvider serviceProvider)
        {
            var returnValue = true;
            if (command.Type == MessageType.MoveLoadingUnit)
            {
                if (command.Data is MoveLoadingUnitMessageData messageData)
                {
                    switch (messageData.MissionType)
                    {
                        case MissionType.Manual:
                            var errorProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
                            // no duplicate of LU
                            returnValue = !this.machineMissions.Any(m =>
                                m.Type == moveRequestedMission
                                && ((DataModels.Mission)m.MachineData).MissionType == messageData.MissionType
                                && ((DataModels.Mission)m.MachineData).LoadingUnitId == messageData.LoadingUnitId
                                && ((DataModels.Mission)m.MachineData).Status == MissionStatus.Executing
                                );
                            if (!returnValue)
                            {
                                errorProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisLoadUnit);
                            }
                            else
                            {
                                // no duplicate of targetBay
                                returnValue = !this.machineMissions.Any(m =>
                                    m.Type == moveRequestedMission
                                    && ((DataModels.Mission)m.MachineData).MissionType == messageData.MissionType
                                    && ((DataModels.Mission)m.MachineData).TargetBay == messageData.TargetBay
                                    && ((DataModels.Mission)m.MachineData).Status == MissionStatus.Executing
                                    );
                                if (!returnValue)
                                {
                                    errorProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisBay);
                                }
                            }
                            break;
                    }
                }
            }
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
