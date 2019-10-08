using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.FiniteStateMachines.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShuttersController : BaseAutomationController
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly IShutterManualMovementsDataLayer shutterManualMovementsDataLayer;

        private readonly IShutterTestParametersProvider shutterTestParametersProvider;

        #endregion

        #region Constructors

        public ShuttersController(
            IEventAggregator eventAggregator,
            IShutterTestParametersProvider shutterTestParametersProvider,
            IShutterManualMovementsDataLayer shutterManualMovementsDataLayer,
            IBaysProvider baysProvider,
            ISensorsProvider sensorsProvider)
            : base(eventAggregator)
        {
            if (shutterTestParametersProvider is null)
            {
                throw new ArgumentNullException(nameof(shutterTestParametersProvider));
            }

            if (shutterManualMovementsDataLayer is null)
            {
                throw new ArgumentNullException(nameof(shutterManualMovementsDataLayer));
            }

            if (baysProvider is null)
            {
                throw new ArgumentNullException(nameof(baysProvider));
            }

            if (sensorsProvider is null)
            {
                throw new ArgumentNullException(nameof(sensorsProvider));
            }

            this.baysProvider = baysProvider;
            this.sensorsProvider = sensorsProvider;
            this.shutterTestParametersProvider = shutterTestParametersProvider;
            this.shutterManualMovementsDataLayer = shutterManualMovementsDataLayer;
        }

        #endregion

        #region Methods

        [HttpGet("shutters/position")]
        public ActionResult<ShutterPosition> GetShutterPosition()
        {
            return this.Ok(this.sensorsProvider.GetShutterPosition(this.BayNumber));
        }

        [HttpGet]
        public ActionResult<ShutterTestParameters> GetTestParameters()
        {
            var parameters = this.shutterTestParametersProvider.Get();

            return this.Ok(parameters);
        }

        [HttpPost("move")]
        public void Move(ShutterMovementDirection direction)
        {
            var speedRate = this.shutterManualMovementsDataLayer.FeedRateSM * this.shutterManualMovementsDataLayer.MinSpeed;

            // speed is negative to go up
            speedRate *= (direction == ShutterMovementDirection.Up) ? -1 : 1;

            var targetPosition = direction == ShutterMovementDirection.Up
                ? ShutterPosition.Opened
                : ShutterPosition.Closed;

            var bay = this.baysProvider.GetByNumber(this.BayNumber);

            var messageData = new ShutterPositioningMessageData(
                targetPosition,
                direction,
                bay.Shutter.Type,
                speedRate,
                0,
                0,
                MovementMode.ShutterPosition,
                MovementType.Relative,
                0,
                0,
                0,
                0,
                0);

            this.PublishCommand(
                messageData,
                "Execute Shutter Positioning Movement Command",
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning);
        }

        [HttpPost("moveTo")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult MoveTo(ShutterPosition targetPosition)
        {
            var direction = ShutterMovementDirection.None;
            var position = this.sensorsProvider.GetShutterPosition(this.BayNumber);
            switch (targetPosition)
            {
                case ShutterPosition.Closed:
                    if (position == ShutterPosition.Half || position == ShutterPosition.Opened)
                    {
                        direction = ShutterMovementDirection.Down;
                    }
                    break;

                case ShutterPosition.Half:
                    if (position == ShutterPosition.Opened)
                    {
                        direction = ShutterMovementDirection.Down;
                    }
                    else if (position == ShutterPosition.Closed)
                    {
                        direction = ShutterMovementDirection.Up;
                    }
                    break;

                case ShutterPosition.Opened:
                    if (position == ShutterPosition.Half || position == ShutterPosition.Closed)
                    {
                        direction = ShutterMovementDirection.Up;
                    }
                    break;

                default:
                    break;
            }
            if (direction == ShutterMovementDirection.None)
            {
                if (targetPosition != position)
                {
                    return this.BadRequest(Resources.Shutters.ThePositionIsNotValid);
                }
                else
                {
                    // destination already reached
                    return this.Accepted();
                }
            }

            var speedRate = this.shutterManualMovementsDataLayer.FeedRateSM * this.shutterManualMovementsDataLayer.MaxSpeed;
            if (speedRate == 0)
            {
                return this.BadRequest(Resources.Shutters.TheSpeedRateIsNotValid);
            }

            var lowSpeed = this.shutterManualMovementsDataLayer.FeedRateSM * this.shutterManualMovementsDataLayer.MinSpeed;
            if (lowSpeed == 0)
            {
                return this.BadRequest(Resources.Shutters.TheMinSpeedIsNotValid);
            }

            var bay = this.baysProvider.GetByNumber(this.BayNumber);

            // speed is negative to go up
            speedRate *= (direction == ShutterMovementDirection.Up) ? -1 : 1;
            lowSpeed *= (direction == ShutterMovementDirection.Up) ? -1 : 1;

            var messageData = new ShutterPositioningMessageData(
                targetPosition,
                direction,
                bay.Shutter.Type,
                speedRate,
                this.shutterManualMovementsDataLayer.HigherDistance,
                this.shutterManualMovementsDataLayer.LowerDistance,
                MovementMode.ShutterPosition,
                MovementType.Absolute,
                0,
                0,
                this.shutterManualMovementsDataLayer.HighSpeedDurationOpen,
                this.shutterManualMovementsDataLayer.HighSpeedDurationClose,
                lowSpeed);

            this.PublishCommand(
                messageData,
                "Execute Shutter Positioning Movement Command",
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning);

            return this.Accepted();
        }

        [HttpPost("run-test")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult RunTest(int delayInSeconds, int testCycleCount)
        {
            if (delayInSeconds <= 0)
            {
                return this.BadRequest(Resources.Shutters.TheDelayBetweenTestCyclesMustBeStrictlyPositive);
            }

            if (testCycleCount <= 0)
            {
                return this.BadRequest(Resources.Shutters.TheNumberOfTestCyclesMustBeStrictlyPositive);
            }

            var speedRate = this.shutterManualMovementsDataLayer.FeedRateSM * this.shutterManualMovementsDataLayer.MaxSpeed;
            if (speedRate == 0)
            {
                return this.BadRequest(Resources.Shutters.TheSpeedRateIsNotValid);
            }
            var lowSpeed = this.shutterManualMovementsDataLayer.FeedRateSM * this.shutterManualMovementsDataLayer.MinSpeed;
            if (lowSpeed == 0)
            {
                return this.BadRequest(Resources.Shutters.TheMinSpeedIsNotValid);
            }

            var bay = this.baysProvider.GetByNumber(this.BayNumber);

            var delayInMilliseconds = delayInSeconds * 1000;

            var messageData = new ShutterPositioningMessageData(
                ShutterPosition.None,
                ShutterMovementDirection.None,
                bay.Shutter.Type,
                speedRate,
                this.shutterManualMovementsDataLayer.HigherDistance,
                this.shutterManualMovementsDataLayer.LowerDistance,
                MovementMode.ShutterTest,
                MovementType.Absolute,
                testCycleCount,
                delayInMilliseconds,
                this.shutterManualMovementsDataLayer.HighSpeedDurationOpen,
                this.shutterManualMovementsDataLayer.HighSpeedDurationClose,
                lowSpeed);

            this.PublishCommand(
                messageData,
                "Execute Shutter Test Loop Command",
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning);

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
