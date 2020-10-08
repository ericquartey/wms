using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager
{
    internal partial class MachineManagerService
    {
        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return
                command.Destination == MessageActor.MachineManager
                ||
                command.Destination == MessageActor.Any;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command, IServiceProvider serviceProvider)
        {
            switch (command.Type)
            {
                case MessageType.ChangeRunningState:
                    lock (this.syncMachineObject)
                    {
                        this.OnChangeRunningStateCommandReceived(command, serviceProvider);
                    }
                    break;

                case MessageType.MoveLoadingUnit:
                    lock (this.syncObject)
                    {
                        this.OnMoveLoadingUnit(command, serviceProvider);
                    }
                    break;
            }
            return Task.CompletedTask;
        }

        private void OnChangeRunningStateCommandReceived(CommandMessage command, IServiceProvider serviceProvider)
        {
            if (command is null)
            {
                return;
            }

            var machineVolatileDataProvider = serviceProvider.GetRequiredService<IMachineVolatileDataProvider>();
            if (!machineVolatileDataProvider.IsAutomationServiceReady)
            {
                this.Logger.LogError($"Failed to start Change Running State machine mission: DataLayer is not ready!");
                this.NotifyCommandError(command);
                return;
            }

            if (command.Data is ChangeRunningStateMessageData messageData)
            {
                switch (messageData.CommandAction)
                {
                    case CommandAction.Start:
                        var machineMissionsProvider = serviceProvider.GetRequiredService<IMachineMissionsProvider>();
                        if (machineMissionsProvider.TryCreateMachineMission(FsmType.ChangeRunningType, command, out var missionId))
                        {
                            try
                            {
                                machineMissionsProvider.StartMachineMission(missionId, command);
                            }
                            catch (Exception ex)
                            {
                                this.Logger.LogError($"Failed to start Change Running State machine mission {ex.Message}");
                                this.NotifyCommandError(command);
                            }
                        }
                        else
                        {
                            this.Logger.LogWarning("Failed to create Change Running State machine mission");
                            var activeMachineMission = machineMissionsProvider.GetMissionsByType(FsmType.ChangeRunningType, MissionType.Manual).FirstOrDefault();
                            if (activeMachineMission != null)
                            {
                                machineMissionsProvider.StopMachineMission(activeMachineMission.FsmId, StopRequestReason.Stop);
                            }
                        }

                        break;

                    case CommandAction.Abort:
                    case CommandAction.Pause:
                    case CommandAction.Resume:
                        this.Logger.LogError($"Invalid command action {messageData.CommandAction} for Change Running State Mission");
                        this.NotifyCommandError(command);

                        break;

                    case CommandAction.Stop:
                        this.Logger.LogError($"Invalid command action {messageData.CommandAction} for Change Running State Mission");
                        break;
                }
            }
            else
            {
                this.Logger.LogError($"Invalid command message data {command.Data.GetType().Name} for Change Running State Command");
                this.NotifyCommandError(command);
            }
        }

        private void OnMoveLoadingUnit(CommandMessage command, IServiceProvider serviceProvider)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var machineVolatileDataProvider = serviceProvider.GetRequiredService<IMachineVolatileDataProvider>();
            if (!machineVolatileDataProvider.IsAutomationServiceReady)
            {
                this.Logger.LogError($"Unable to start load unit movement: dataLayer is not ready.");
                this.NotifyCommandError(command);
                return;
            }
            var missionsDataProvider = serviceProvider.GetRequiredService<IMissionsDataProvider>();
            var missionMoveProvider = serviceProvider.GetRequiredService<IMissionMoveProvider>();

            if (command.Data is MoveLoadingUnitMessageData messageData)
            {
                switch (messageData.CommandAction)
                {
                    case CommandAction.Start:
                        {
                            if (missionMoveProvider.TryCreateMachineMission(command, serviceProvider, out var mission)
                                && mission != null
                                )
                            {
                                try
                                {
                                    if (!missionMoveProvider.StartMission(mission, command, serviceProvider, true))
                                    {
                                        machineVolatileDataProvider.Mode = MachineMode.Manual;
                                        this.Logger.LogInformation($"Start Mission error: Machine status switched to {machineVolatileDataProvider.Mode}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    this.Logger.LogError($"Failed to start mission: {ex}");
                                    this.NotifyCommandError(command);
                                }
                            }
                            else
                            {
                                this.Logger.LogError("Failed to create Move mission");
                                this.NotifyCommandError(command);
                            }
                        }

                        break;

                    case CommandAction.Activate:
                        try
                        {
                            var mission = missionsDataProvider.GetById(messageData.MissionId.Value);
                            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

                            // Call UpdateWaitingMissionOnBay(): check if there is a mission to complete in bay,
                            // and if exists, complete it and exit from the OnMoveLoadingUnit() method
                            if (missionMoveProvider.UpdateWaitingMissionToDoubleBay(serviceProvider, missionsDataProvider, baysDataProvider, mission))
                            {
                                this.Logger.LogInformation($"Resuming missions on bay {mission.TargetBay}. Please wait ...");
                                break;
                            }

                            // So call the UpdateWaitingMission and start the current mission...
                            if (!missionMoveProvider.UpdateWaitingMission(missionsDataProvider, baysDataProvider, mission)
                                || !missionMoveProvider.StartMission(mission, command, serviceProvider, false)
                                )
                            {
                                this.Logger.LogWarning($"Conditions not met to activate Mission:Id={mission.Id}, Load Unit {mission.LoadUnitId} .");
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError($"Failed to activate mission: {ex}");
                        }
                        break;

                    case CommandAction.Abort:
                        try
                        {
                            if (messageData.MissionId != null)
                            {
                                if (!missionMoveProvider.StopMission(messageData.MissionId.Value, StopRequestReason.Abort, serviceProvider))
                                {
                                    this.Logger.LogError("Supplied mission Id to be aborted is no longer valid");
                                    this.NotifyCommandError(command);
                                }
                            }
                            else
                            {
                                foreach (var mission in missionsDataProvider.GetAllMissions())
                                {
                                    missionMoveProvider.StopMission(mission.Id, StopRequestReason.Abort, serviceProvider);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError($"Failed to abort mission: {ex}");
                        }
                        break;

                    case CommandAction.Stop:
                        try
                        {
                            if (messageData.MissionId != null)
                            {
                                if (!missionMoveProvider.StopMission(messageData.MissionId.Value, StopRequestReason.Stop, serviceProvider))
                                {
                                    this.Logger.LogError("Supplied mission Id to be stopped is no longer valid");
                                    this.NotifyCommandError(command);
                                }
                            }
                            else
                            {
                                foreach (var mission in missionsDataProvider.GetAllActiveMissions())
                                {
                                    missionMoveProvider.StopMission(mission.Id, StopRequestReason.Stop, serviceProvider);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError($"Failed to stop mission: {ex}");
                        }
                        break;

                    //case CommandAction.Pause:
                    //    if (messageData.MissionId != null)
                    //    {
                    //        if (!this.machineMissionsProvider.PauseMachineMission(messageData.MissionId.Value))
                    //        {
                    //            this.Logger.LogError("Supplied mission Id to be stopped is no longer valid");
                    //            this.NotifyCommandError(command);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        foreach (var mission in this.machineMissionsProvider.GetMissionsByFsmType(FsmType.MoveLoadingUnit))
                    //        {
                    //            mission.PauseMachineMission();
                    //        }
                    //    }

                    //    break;

                    case CommandAction.Resume:
                        try
                        {
                            if (messageData.MissionId != null)
                            {
                                if (!missionMoveProvider.ResumeMission(messageData.MissionId.Value, command, serviceProvider))
                                {
                                    this.Logger.LogError($"Supplied mission Id {messageData.MissionId} to be resumed is no longer valid");
                                    this.NotifyCommandError(command);
                                }
                            }
                            else
                            {
                                foreach (var mission in missionsDataProvider.GetAllActiveMissions())
                                {
                                    missionMoveProvider.ResumeMission(mission.Id, command, serviceProvider);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError($"Failed to resume mission: {ex}");
                        }

                        break;
                }
            }
            else
            {
                this.Logger.LogError($"Invalid command message data {command.Data.GetType().Name} for Move Loading Unit Command");
                this.NotifyCommandError(command);
            }
            missionsDataProvider.CheckPendingChanges();
        }

        #endregion
    }
}
