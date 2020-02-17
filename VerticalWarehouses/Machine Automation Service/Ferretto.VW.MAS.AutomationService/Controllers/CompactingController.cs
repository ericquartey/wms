using System;
using Ferretto.VW.MAS.MachineManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompactingController : ControllerBase
    {
        #region Fields

        private readonly IMachineModeProvider machineModeProvider;

        #endregion

        #region Constructors

        public CompactingController(IMachineModeProvider machineModeProvider)
        {
            this.machineModeProvider = machineModeProvider ?? throw new ArgumentNullException(nameof(machineModeProvider));
        }

        #endregion

        #region Methods

        [HttpPost("compacting")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Compacting()
        {
            this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Compact);
            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Manual);
            return this.Accepted();
        }

        #endregion
    }
}
