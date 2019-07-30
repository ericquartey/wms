using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    public partial class MachineStatusController : ControllerBase
    {

        #region Methods

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
