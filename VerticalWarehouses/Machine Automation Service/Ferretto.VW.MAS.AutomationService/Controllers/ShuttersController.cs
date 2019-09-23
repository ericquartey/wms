using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Microsoft.AspNetCore.Http;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Models;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShuttersController : BaseAutomationController
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationProvider;

        private readonly IShutterManualMovementsDataLayer shutterManualMovementsDataLayer;

        private readonly IShutterTestParametersProvider shutterTestParametersProvider;

        #endregion

        #region Constructors

        public ShuttersController(
            IEventAggregator eventAggregator,
            IShutterTestParametersProvider shutterTestParametersProvider,
            IShutterManualMovementsDataLayer shutterManualMovementsDataLayer,
            IConfigurationValueManagmentDataLayer configurationProvider)
            : base(eventAggregator)
        {
            if (shutterTestParametersProvider is null)
            {
                throw new ArgumentNullException(nameof(shutterTestParametersProvider));
            }

            if (configurationProvider is null)
            {
                throw new ArgumentNullException(nameof(configurationProvider));
            }

            if (shutterManualMovementsDataLayer is null)
            {
                throw new ArgumentNullException(nameof(shutterManualMovementsDataLayer));
            }

            this.shutterTestParametersProvider = shutterTestParametersProvider;
            this.shutterManualMovementsDataLayer = shutterManualMovementsDataLayer;
            this.configurationProvider = configurationProvider;
        }

        #endregion

        #region Methods

        [HttpGet("shutters/position")]
        public ActionResult<ShutterPosition> GetShutterPosition(int bayNumber)
        {
            // TODO add check on bay number

            return this.Ok(this.GetShutterPositionController(bayNumber));
        }

        [HttpGet]
        public ActionResult<ShutterTestParameters> GetTestParameters()
        {
            var parameters = this.shutterTestParametersProvider.Get();

            return this.Ok(parameters);
        }

        [HttpPost("{bayNumber}/move")]
        public void Move(int bayNumber, ShutterMovementDirection direction)
        {
            var speedRate = this.shutterManualMovementsDataLayer.FeedRateSM * this.shutterManualMovementsDataLayer.MinSpeed;

            // speed is negative to go up
            speedRate *= (direction == ShutterMovementDirection.Up) ? -1 : 1;

            var targetPosition = direction == ShutterMovementDirection.Up
                ? ShutterPosition.Opened
                : ShutterPosition.Closed;

            var messageData = new ShutterPositioningMessageData(
                targetPosition,
                direction,
                ShutterType.Shutter3Type, // TODO HACK remove this hardcoded value
                speedRate,
                0,
                0,
                MovementMode.Position,
                MovementType.Relative,
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

        [HttpPost("{bayNumber}/moveTo")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult MoveTo(int bayNumber, ShutterPosition targetPosition)
        {
            var direction = ShutterMovementDirection.None;
            var position = this.GetShutterPositionController(bayNumber);
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

            // speed is negative to go up
            speedRate *= (direction == ShutterMovementDirection.Up) ? -1 : 1;
            lowSpeed *= (direction == ShutterMovementDirection.Up) ? -1 : 1;

            var messageData = new ShutterPositioningMessageData(
                targetPosition,
                direction,
                ShutterType.Shutter3Type, // TODO HACK remove this hardcoded value
                speedRate,
                this.shutterManualMovementsDataLayer.HigherDistance,
                this.shutterManualMovementsDataLayer.LowerDistance,
                MovementMode.Position,
                MovementType.Absolute,
                0,
                0,
                this.shutterManualMovementsDataLayer.HighSpeedPercent,
                lowSpeed);

            this.PublishCommand(
                messageData,
                "Execute Shutter Positioning Movement Command",
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning);

            return this.Accepted();
        }

        [HttpPost("{bayNumber}/run-test")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult RunTest(int bayNumber, int delayInSeconds, int testCycleCount)
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

            var delayInMilliseconds = delayInSeconds * 1000;

            var messageData = new ShutterPositioningMessageData(
                ShutterPosition.None,
                ShutterMovementDirection.None,
                ShutterType.Shutter3Type, // TODO HACK remove this hardcoded value
                speedRate,
                this.shutterManualMovementsDataLayer.HigherDistance,
                this.shutterManualMovementsDataLayer.LowerDistance,
                MovementMode.ShutterTest,
                MovementType.Absolute,
                testCycleCount,
                delayInMilliseconds,
                this.shutterManualMovementsDataLayer.HighSpeedPercent,
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

        private ShutterPosition GetShutterPositionController(int bayNumber)
        {
            var messageData = new RequestPositionMessageData(Axis.None, bayNumber);

            void publishAction()
            {
                this.PublishCommand(
                messageData,
                "Request shutter position",
                MessageActor.FiniteStateMachines,
                MessageType.RequestPosition);
            }

            var notifyData = this.WaitForResponseEventAsync<ShutterPositioningMessageData>(
                MessageType.ShutterPositioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);
            return notifyData.ShutterPosition;
        }

        #endregion
    }
}
