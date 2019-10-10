using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MissionsManager.BackgroundServices
{
    internal partial class MissionsManagerService
    {
        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return
                command.Destination == MessageActor.MissionsManager
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
                    this.OnMoveLoadingUnit(command);
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

            switch (((ChangeRunningStateMessageData)command.Data).CommandAction)
            {
                case CommandAction.Start:
                    if (this.missionsProvider.TryCreateMachineMission(MissionType.ChangeRunningType, command, out var missionId))
                    {
                        this.missionsProvider.StartMachineMission(missionId, command);
                    }
                    else
                    {
                        this.Logger.LogDebug("Failed to create Change Running State machine mission");
                        this.NotifyCommandError(command);
                    }

                    break;
            }
        }

        private void OnMoveLoadingUnit(CommandMessage command)
        {
            if (command is null)
            {
                return;
            }

            switch (((MoveLoadingUnitMessageData)command.Data).CommandAction)
            {
                case CommandAction.Start:
                    if (this.missionsProvider.TryCreateMachineMission(MissionType.MoveLoadingUnit, command, out var missionId))
                    {
                        this.missionsProvider.StartMachineMission(missionId, command);
                    }
                    else
                    {
                        this.Logger.LogDebug("Failed to create Move Loading UNit State machine mission");
                        this.NotifyCommandError(command);
                    }

                    break;

                case CommandAction.Abort:
                    break;

                case CommandAction.Stop:
                    break;
            }
        }

        #endregion
    }
}
