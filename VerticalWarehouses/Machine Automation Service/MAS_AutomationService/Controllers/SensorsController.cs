using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class SensorsController : ControllerBase
    {
        #region Methods

        [HttpGet("force-notification")]
        public IActionResult ForceNotification([FromServices] IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    null,
                    "Sensors changed Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.SensorsChanged));

            return this.Ok();
        }

        #endregion
    }
}
