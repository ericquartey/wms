using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class CarouselController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IHorizontalAxisDataLayer horizontalAxis;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovements;

        private readonly ILogger logger;

        private readonly ISetupStatusDataLayer setupStatus;

        private readonly IVerticalAxisDataLayer verticalAxis;

        private readonly IVerticalManualMovementsDataLayer verticalManualMovements;

        #endregion

        #region Constructors

        public CarouselController(
            IEventAggregator eventAggregator,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer,
            IHorizontalAxisDataLayer horizontalAxisDataLayer,
            IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer,
            ISetupStatusDataLayer setupStatusDataLayer,
            ILogger<ElevatorController> logger)
        {
            this.eventAggregator = eventAggregator;
            this.verticalAxis = verticalAxisDataLayer;
            this.verticalManualMovements = verticalManualMovementsDataLayer;
            this.horizontalAxis = horizontalAxisDataLayer;
            this.horizontalManualMovements = horizontalManualMovementsDataLayer;
            this.setupStatus = setupStatusDataLayer;
            this.logger = logger;
        }

        #endregion

        #region Methods

        [HttpGet("position")]
        public ActionResult<decimal> GetVerticalPosition()
        {
            return 0;
        }

        [HttpPost("move")]
        public void Move([FromBody]CarouselMovementParameters parameters)
        {
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("stop")]
        public IActionResult Stop()
        {
            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
                   new CommandMessage(
                       null,
                       "Stop Command",
                       MessageActor.FiniteStateMachines,
                       MessageActor.WebApi,
                       MessageType.Stop));

            return this.Ok();
        }

        #endregion
    }
}
