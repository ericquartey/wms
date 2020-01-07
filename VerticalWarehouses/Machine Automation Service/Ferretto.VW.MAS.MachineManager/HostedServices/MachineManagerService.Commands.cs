using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
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
                    this.OnChangeRunningStateCommandReceived(command);
                    break;

                case MessageType.MoveLoadingUnit:
                    this.OnMoveLoadingUnit(command, serviceProvider);
                    break;
            }

            return Task.CompletedTask;
        }

        private void OnChangeRunningStateCommandReceived(CommandMessage command)
        {
            if (command is null)
            {
                return;
            }

            if (!this.isDataLayerReady)
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
                        if (this.machineMissionsProvider.TryCreateMachineMission(FsmType.ChangeRunningType, command, out var missionId))
                        {
                            try
                            {
                                this.machineMissionsProvider.StartMachineMission(missionId, command);
                            }
                            catch (Exception ex)
                            {
                                this.Logger.LogError($"Failed to start Change Running State machine mission {ex.Message}");
                                this.NotifyCommandError(command);
                            }
                        }
                        else
                        {
                            this.Logger.LogError("Failed to create Change Running State machine mission");
                            this.NotifyCommandError(command);
                        }

                        break;

                    case CommandAction.Abort:
                    case CommandAction.Pause:
                    case CommandAction.Resume:
                        this.Logger.LogError($"Invalid command action {messageData.CommandAction} for Change Running State Mission");
                        this.NotifyCommandError(command);

                        break;

                    case CommandAction.Stop:
                        /*
                        if (messageData.MissionId != null)
                        {
                            this.machineMissionsProvider.StopMachineMission(messageData.MissionId.Value, StopRequestReason.Stop);
                        }
                        else
                        {
                            foreach (var mission in this.machineMissionsProvider.GetMissionsByType(MissionType.ChangeRunningType))
                            {
                                mission.StopMachine(StopRequestReason.Stop);
                            }
                        }
                        */
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
                return;
            }

            if (!this.isDataLayerReady)
            {
                this.Logger.LogError($"Failed to start Move Loading Unit State machine mission: DataLayer is not ready!");
                this.NotifyCommandError(command);
                return;
            }

            if (command.Data is MoveLoadingUnitMessageData messageData)
            {
                switch (messageData.CommandAction)
                {
                    case CommandAction.Start:

                        if (this.missionMoveProvider.TryCreateMachineMission(command, serviceProvider, out var missionId)
                            && missionId.HasValue
                            )
                        {
                            try
                            {
                                this.missionMoveProvider.StartMission(missionId.Value, command, serviceProvider);
                            }
                            catch (Exception ex)
                            {
                                this.Logger.LogError($"Failed to start Move Loading Unit State machine mission: {ex.Message}");
                                this.NotifyCommandError(command);
                            }
                        }
                        else
                        {
                            this.Logger.LogError("Failed to create Move Loading Unit State machine mission");
                            this.NotifyCommandError(command);
                        }

                        break;

                    case CommandAction.Activate:
                        try
                        {
                            var mission = this.missionsDataProvider.GetByGuid(messageData.MissionId.Value);
                            this.missionMoveProvider.StartMission(mission.Id, command, serviceProvider);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError($"Failed to start Move Loading Unit State machine mission: {ex.Message}");
                        }
                        break;

                    //case CommandAction.Abort:
                    //    if (messageData.MissionId != null)
                    //    {
                    //        if (!this.machineMissionsProvider.AbortMachineMission(messageData.MissionId.Value))
                    //        {
                    //            this.Logger.LogError("Supplied mission Id to be aborted is no longer valid");
                    //            this.NotifyCommandError(command);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        foreach (var mission in this.machineMissionsProvider.GetMissionsByFsmType(FsmType.MoveLoadingUnit))
                    //        {
                    //            mission.AbortMachineMission();
                    //        }
                    //    }

                    //    break;

                    case CommandAction.Stop:
                        if (messageData.MissionId != null)
                        {
                            if (!this.missionMoveProvider.StopMission(messageData.MissionId.Value, StopRequestReason.Stop, serviceProvider))
                            {
                                this.Logger.LogError("Supplied mission Id to be stopped is no longer valid");
                                this.NotifyCommandError(command);
                            }
                        }
                        else
                        {
                            foreach (var mission in this.missionsDataProvider.GetAllActiveMissions())
                            {
                                this.missionMoveProvider.StopMission(mission.FsmId, StopRequestReason.Stop, serviceProvider);
                            }
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
                        if (messageData.MissionId != null)
                        {
                            if (!this.missionMoveProvider.ResumeMission(messageData.MissionId.Value, command, serviceProvider))
                            {
                                this.Logger.LogError("Supplied mission Id to be stopped is no longer valid");
                                this.NotifyCommandError(command);
                            }
                        }
                        else
                        {
                            foreach (var mission in this.missionsDataProvider.GetAllActiveMissions())
                            {
                                this.missionMoveProvider.ResumeMission(mission.FsmId, command, serviceProvider);
                            }
                        }

                        break;
                }
            }
            else
            {
                this.Logger.LogError($"Invalid command message data {command.Data.GetType().Name} for Move Loading Unit Command");
                this.NotifyCommandError(command);
            }
        }

        #endregion
    }
}
