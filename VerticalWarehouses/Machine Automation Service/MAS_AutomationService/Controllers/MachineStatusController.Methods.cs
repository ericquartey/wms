using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    public partial class MachineStatusController : ControllerBase
    {
        #region Methods

        private void ExecutePowerOff_Method()
        {
            var powerEnableMessageData = new PowerEnableMessageData(false);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    powerEnableMessageData,
                    "Power Enable Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.PowerEnable));
        }

        private void ExecutePowerOn_Method()
        {
            var powerEnableMessageData = new PowerEnableMessageData(true);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    powerEnableMessageData,
                    "Power Enable Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.PowerEnable));
        }

        private void ExecuteResetSecurity_Method()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    null,
                    "Reset Security Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.ResetSecurity));
        }

        #endregion
    }
}
