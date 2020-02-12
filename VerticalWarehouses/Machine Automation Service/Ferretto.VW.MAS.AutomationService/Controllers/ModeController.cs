using System;
using Ferretto.VW.MAS.MachineManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModeController : ControllerBase
    {
        #region Fields

        private readonly IMachineModeProvider machineModeProvider;

        #endregion

        #region Constructors

        public ModeController(IMachineModeProvider machineModeProvider)
        {
            this.machineModeProvider = machineModeProvider ?? throw new ArgumentNullException(nameof(machineModeProvider));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<CommonUtils.Messages.MachineMode> Get()
        {
            return this.machineModeProvider.GetCurrent();
        }

        [HttpPost("automatic")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SetAutomatic()
        {
            this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Automatic);

            return this.Accepted();
        }

        [HttpPost("manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SetManual()
        {
            this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Manual);

            return this.Accepted();
        }

        #endregion
    }
}
