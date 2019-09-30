using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepositPickupProcedureController : BaseAutomationController
    {
        public DepositPickupProcedureController(
                IEventAggregator eventAggregator)
                : base(eventAggregator)
        {
        }

        [HttpGet("parameters")]
        public ActionResult<DepositPickUpParameters> GetParameters()
        {            
            var parameters = new DepositPickUpParameters
            {   Delay = 2,
                RequiredCycles = 10000
            };

            return this.Ok(parameters);
        }

        [HttpPost("reset")]
        [HttpPost]
        public IActionResult Reset()
        {
            // TODO reset Total completed cycles            

            return this.Ok();
        }

        [HttpPost("mark-as-completed")]
        public IActionResult MarkAsCompleted()
        {
            // TODO save completed cycles                        

            return this.Ok();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            var messageData = new StopMessageData(StopRequestReason.Stop);
            this.PublishCommand(
                messageData,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop);

            return this.Accepted();
        }


    }
}
