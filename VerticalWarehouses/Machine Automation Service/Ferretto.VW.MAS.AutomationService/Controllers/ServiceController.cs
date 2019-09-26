using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class ServiceController : BaseAutomationController
    {
        #region Fields

        // TODO: avoid hardcoding constants in code
        private const double ChainLength = 2850;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovements;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ServiceController(
            IEventAggregator eventAggregator,
            IElevatorDataProvider elevatorDataProvider,
            IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer,
            ILogger<ServiceController> logger)
            : base(eventAggregator)
        {
            if (horizontalManualMovementsDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalManualMovementsDataLayer));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = logger;
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.horizontalManualMovements = horizontalManualMovementsDataLayer;
        }

        #endregion

        #region Methods

        [HttpPost("search-horizontal-zero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SearchHorizontalZero()
        {
            var horizontalAxis = this.elevatorDataProvider.GetHorizontalAxis();
            var actualSpeed = horizontalAxis.EmptyLoadMovement.Speed * (double)this.horizontalManualMovements.FeedRateHM;

            var speed = new[] { actualSpeed };
            var acceleration = new[] { horizontalAxis.EmptyLoadMovement.Acceleration };
            var deceleration = new[] { horizontalAxis.EmptyLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.FindZero,
                ChainLength,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.PublishCommand(
                    messageData,
                    $"Execute Find Horizontal Zero Positioning Command",
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning);

            return this.Accepted();
        }

        #endregion
    }
}
