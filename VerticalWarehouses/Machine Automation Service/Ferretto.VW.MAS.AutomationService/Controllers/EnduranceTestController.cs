using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnduranceTestController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IElevatorProvider elevatorProvider;

        #endregion

        #region Constructors

        public EnduranceTestController(
            IElevatorProvider elevatorProvider)
        {
            this.elevatorProvider = elevatorProvider ?? throw new ArgumentNullException(nameof(elevatorProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("start/repetitive-horizontal")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult StartHorizontalMovements(int bayPositionId, int loadingUnitId)
        {
            this.elevatorProvider.StartRepetitiveHorizontalMovements(bayPositionId, loadingUnitId, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.elevatorProvider.Stop(this.BayNumber, MessageActor.WebApi);

            return this.Accepted();
        }

        #endregion
    }
}
