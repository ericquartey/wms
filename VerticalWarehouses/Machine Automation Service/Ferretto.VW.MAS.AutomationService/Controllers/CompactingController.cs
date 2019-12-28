using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompactingController : ControllerBase
    {
        public CompactingController()
        { }

        [HttpPost("compacting")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Compacting()
        {
            return this.Accepted();
        }
        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            return this.Accepted();
        }
    }
}
