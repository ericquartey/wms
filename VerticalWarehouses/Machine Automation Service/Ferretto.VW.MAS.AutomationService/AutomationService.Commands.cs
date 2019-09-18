using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.AutomationService
{
    partial class AutomationService
    {


        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return
                command.Destination == MessageActor.AutomationService
                ||
                command.Destination == MessageActor.Any;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command)
        {
            switch(command.Type)
            {
                case MessageType.PowerEnable:
                this.currentStateMachine = new PowerEnableStateMachine(command, this.configuredBays, this.eventAggregator, this.logger, this.serviceScopeFactory);
                this.currentStateMachine.Start();
                break;

                case MessageType.Stop:
                this.currentStateMachine?.Stop(StopRequestReason.Stop);
                break;
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
