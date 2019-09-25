using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages.Data;
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

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovements;

        private readonly ILogger logger;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalManualMovementsDataLayer verticalManualMovements;

        #endregion

        #region Constructors

        public CarouselController(
            IEventAggregator eventAggregator,
            IElevatorDataProvider elevatorDataProvider,
            IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer,
            IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer,
            ISetupStatusProvider setupStatusProvider,
            ILogger<CarouselController> logger)
            : base(eventAggregator)
        {
            if (verticalManualMovementsDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(verticalManualMovementsDataLayer));
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

            this.elevatorDataProvider = elevatorDataProvider ?? throw new System.ArgumentNullException(nameof(elevatorDataProvider));
            this.verticalManualMovements = verticalManualMovementsDataLayer;
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
            var messageData = new StopMessageData(StopRequestReason.Stop);
            this.PublishCommand(
                messageData,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop);

            return this.Accepted();
        }

        #endregion
    }
}
