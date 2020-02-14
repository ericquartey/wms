using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.LaserDriver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaserController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly ILaserProvider laserProvider;

        #endregion

        #region Constructors

        public LaserController(ILaserProvider laserProvider)
        {
            this.laserProvider = laserProvider ?? throw new ArgumentNullException(nameof(laserProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("swith-off")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SwitchOff()
        {
            this.laserProvider.SwitchOff(this.BayNumber);
            return this.Accepted();
        }

        [HttpPost("swith-on")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SwitchOn()
        {
            this.laserProvider.SwitchOn(this.BayNumber);
            return this.Accepted();
        }

        #endregion
    }
}
