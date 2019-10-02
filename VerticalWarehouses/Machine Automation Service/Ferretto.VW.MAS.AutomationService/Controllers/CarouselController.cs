using System;
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

        private readonly IBaysProvider baysProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovements;

        #endregion

        #region Constructors

        public CarouselController(
            IEventAggregator eventAggregator,
            IElevatorDataProvider elevatorDataProvider,
            IBaysProvider baysProvider,
            IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer)
            : base(eventAggregator)
        {
            if (baysProvider is null)
            {
                throw new ArgumentNullException(nameof(baysProvider));
            }

            if (horizontalManualMovementsDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalManualMovementsDataLayer));
            }

            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.baysProvider = baysProvider;
            this.horizontalManualMovements = horizontalManualMovementsDataLayer;
        }

        #endregion

        #region Methods

        [HttpGet("position")]
        public ActionResult<decimal> GetPosition()
        {
            throw new NotImplementedException("Carousel positioning not implemented");
        }

        [HttpPost("move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult Move(HorizontalMovementDirection direction)
        {
            var bay = this.baysProvider.GetByNumber(this.BayNumber);
            if (bay.Type != BayType.Carousel)
            {
                throw new InvalidOperationException($"Cannot operate carousel on bay {this.BayNumber} because it has no carousel.");
            }

            var targetPosition = bay.Carousel.ElevatorDistance;

            targetPosition *= (direction == HorizontalMovementDirection.Forwards) ? -1 : 1;

            var axis = this.elevatorDataProvider.GetHorizontalAxis();

            // TODO: scale movement speed by weight
            var speed = new[] { axis.EmptyLoadMovement.Speed * (double)this.horizontalManualMovements.FeedRateHM / 10 };
            var acceleration = new[] { axis.EmptyLoadMovement.Acceleration };
            var deceleration = new[] { axis.EmptyLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.BayChain,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition,
                direction);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);

            return this.Accepted();
        }

        [HttpPost("move-manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MoveManual(HorizontalMovementDirection direction)
        {
            var bay = this.baysProvider.GetByNumber(this.BayNumber);
            if (bay.Type != BayType.Carousel)
            {
                throw new InvalidOperationException($"Cannot operate carousel on bay {this.BayNumber} because it has no carousel.");
            }

            var targetPosition = bay.Carousel.ElevatorDistance;

            targetPosition *= ((direction == HorizontalMovementDirection.Forwards) ? -1 : 1);

            var axis = this.elevatorDataProvider.GetHorizontalAxis();
            var speed = new[] { axis.MaximumLoadMovement.Speed * (double)this.horizontalManualMovements.FeedRateHM / 10 };
            var acceleration = new[] { axis.MaximumLoadMovement.Acceleration };
            var deceleration = new[] { axis.MaximumLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.BayChainManual,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition,
                direction);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);

            return this.Accepted();
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
