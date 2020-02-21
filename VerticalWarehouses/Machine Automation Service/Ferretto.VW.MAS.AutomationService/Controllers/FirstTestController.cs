using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.MachineManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FirstTestController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IMachineModeProvider machineModeProvider;

        #endregion

        #region Constructors

        public FirstTestController(IMachineModeProvider machineModeProvider)
        {
            this.machineModeProvider = machineModeProvider ?? throw new ArgumentNullException(nameof(machineModeProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Start(int loadunit)
        {
            this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.FirstTest, this.BayNumber, new List<int>() { loadunit });
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
