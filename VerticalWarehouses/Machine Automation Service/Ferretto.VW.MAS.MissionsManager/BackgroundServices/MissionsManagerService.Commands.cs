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

        protected override Task OnCommandReceivedAsync(CommandMessage command)
        {
            switch (command.Type)
            {
                case MessageType.WeightAcquisitionCommand:
                {
                    this.OnWeightAcquisitionProcedureCommandReceived(command);
                    break;
                }

                case MessageType.ChangeRunningState:
                {
                    this.OnChangeRunningStateCommandReceived(command);
                    break;
                }
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
                    if (this.missionsProvider.TryCreateMachineMission(MissionType.ChangeRunningType, out var missionId))
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

        private void OnWeightAcquisitionProcedureCommandReceived(CommandMessage command)
        {
            if (command is null)
            {
                return;
            }

            switch (((WeightAcquisitionCommandMessageData)command.Data).CommandAction)
            {
                case CommandAction.Start:
                    if (this.missionsProvider.TryCreateMachineMission(MissionType.WeightAcquisition, out var missionId))
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

        #endregion
    }
}
