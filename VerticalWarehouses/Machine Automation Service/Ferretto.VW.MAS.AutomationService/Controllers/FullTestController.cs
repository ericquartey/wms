using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.MachineManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FullTestController : ControllerBase
    {
        //private readonly IMachineModeProvider machineModeProvider;

        #region Constructors

        public FullTestController(
            //IMachineModeProvider machineModeProvider
            )
        {
            //this.machineModeProvider = machineModeProvider ?? throw new ArgumentNullException(nameof(machineModeProvider));
        }

        #endregion

        #region Methods

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Start(List<int> loadunits, int cycle)
        {
            //this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Compact);
            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            //this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Manual);
            return this.Accepted();
        }

        #endregion
    }
}
