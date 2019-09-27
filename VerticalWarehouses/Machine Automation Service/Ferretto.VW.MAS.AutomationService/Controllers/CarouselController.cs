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
        public ActionResult<decimal> GetPosition()
        {
            try
            {
                var position = this.GetPositionController(this.BayNumber);
                return this.Ok(position);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<decimal>(ex);
            }
        }

        [HttpPost("move")]
        public void Move(HorizontalMovementDirection direction)
        {
            var targetPosition = this.horizontalAxis.CarouselDistance;

            targetPosition *= ((direction == HorizontalMovementDirection.Forwards) ? -1 : 1);

            decimal[] speed = { this.horizontalAxis.MaxEmptySpeedHA * this.horizontalManualMovements.FeedRateHM / 10 };
            decimal[] acceleration = { this.horizontalAxis.MaxEmptyAccelerationHA };
            decimal[] deceleration = { this.horizontalAxis.MaxEmptyDecelerationHA };
            decimal[] switchPosition = { 0 };

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
        }

        [HttpPost("move-manual")]
        public void MoveManual(HorizontalMovementDirection direction)
        {
            var targetPosition = this.horizontalAxis.CarouselDistance;

            targetPosition *= ((direction == HorizontalMovementDirection.Forwards) ? -1 : 1);

            decimal[] speed = { this.horizontalAxis.MaxFullSpeed * this.horizontalManualMovements.FeedRateHM / 10 };
            decimal[] acceleration = { this.horizontalAxis.MaxFullAccelerationHA };
            decimal[] deceleration = { this.horizontalAxis.MaxFullDecelerationHA };
            decimal[] switchPosition = { 0 };

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

        private decimal GetPositionController(BayNumber bayNumber)
        {
            var messageData = new RequestPositionMessageData(Axis.Horizontal, (int)bayNumber);

            void publishAction()
            {
                this.PublishCommand(
                messageData,
                "Request shutter position",
                MessageActor.FiniteStateMachines,
                MessageType.RequestPosition);
            }

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);
            return notifyData.CurrentPosition ?? 0;
        }

        #endregion
    }
}
