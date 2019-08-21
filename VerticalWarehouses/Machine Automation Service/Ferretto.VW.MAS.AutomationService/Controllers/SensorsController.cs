using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Microsoft.AspNetCore.Http;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsController : BaseAutomationController
    {
        #region Constructors

        public SensorsController(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        }

        #endregion

        #region Methods

        [HttpPost("force-notification")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult ForceNotification()
        {
            this.PublishCommand(
                    null,
                    "Sensors changed Command",
                    MessageActor.FiniteStateMachines,
                    MessageType.SensorsChanged);

            return this.Accepted();
        }

        #endregion
    }
}
