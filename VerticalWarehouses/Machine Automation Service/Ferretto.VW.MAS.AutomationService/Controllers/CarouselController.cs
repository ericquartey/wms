using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarouselController : BaseAutomationController
    {
        #region Fields

        private readonly IHorizontalAxisDataLayer horizontalAxis;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovements;

        private readonly ILogger logger;

        private readonly ISetupStatusProvider setupStatusProvider;

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
            ISetupStatusProvider setupStatusProvider,
            ILogger<CarouselController> logger)
            : base(eventAggregator)
        {
            if (verticalAxisDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            if (verticalManualMovementsDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(verticalManualMovementsDataLayer));
            }

            if (horizontalAxisDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(horizontalAxisDataLayer));
            }

            if (horizontalManualMovementsDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(horizontalManualMovementsDataLayer));
            }

            if (setupStatusProvider is null)
            {
                throw new System.ArgumentNullException(nameof(setupStatusProvider));
            }

            if (logger is null)
            {
                throw new System.ArgumentNullException(nameof(logger));
            }

            this.verticalAxis = verticalAxisDataLayer;
            this.verticalManualMovements = verticalManualMovementsDataLayer;
            this.horizontalAxis = horizontalAxisDataLayer;
            this.horizontalManualMovements = horizontalManualMovementsDataLayer;
            this.setupStatusProvider = setupStatusProvider;
            this.logger = logger;
        }

        #endregion

        #region Methods

        [HttpGet("position")]
        public ActionResult<decimal> GetVerticalPosition()
        {
            throw new System.NotImplementedException();
        }

        [HttpPost("move")]
        public void Move([FromBody]CarouselMovementParameters parameters)
        {
            throw new System.NotImplementedException();
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
