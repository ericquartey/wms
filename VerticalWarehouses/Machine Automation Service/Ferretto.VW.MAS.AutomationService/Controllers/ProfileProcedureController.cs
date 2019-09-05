using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileProcedureController : BaseAutomationController
    {
        #region Constructors

        public ProfileProcedureController(
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        }

        #endregion

        #region Methods

        [HttpPost("{bayNumber}/run")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Run(int bayNumber)
        {
            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.PublishCommand(
                null,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop);

            return this.Accepted();
        }

        #endregion
    }
}
