using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class ShutterProvider : BaseProvider, IShutterProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ILogger<ShutterProvider> logger;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ShutterProvider(
            IBaysDataProvider baysDataProvider,
            ISensorsProvider sensorsProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IEventAggregator eventAggregator,
            ILogger<ShutterProvider> logger)
            : base(eventAggregator)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void ContinuePositioning(BayNumber requestingBay, MessageActor sender)
        {
            this.PublishCommand(
                null,
                $"Continue Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.ContinueMovement,
                requestingBay,
                requestingBay);
        }

        /// <summary>
        /// Moves shutter in slow speed. Used for manual movements.
        /// It stops when top or bottom sensors are detected or when operator sends a stop command.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="bypassConditions"></param>
        /// <param name="bayNumber"></param>
        /// <param name="sender"></param>
        public void Move(ShutterMovementDirection direction, bool bypassConditions, BayNumber bayNumber, MessageActor sender)
        {
            var parameters = this.baysDataProvider.GetManualMovementsShutter(bayNumber);
            var maxSpeed = this.baysDataProvider.GetShutterMaxSpeed(bayNumber);
            var minSpeed = this.baysDataProvider.GetShutterMinSpeed(bayNumber);

            var speedRate = parameters.FeedRate * minSpeed;

            // speed is negative to go up
            speedRate *= (direction == ShutterMovementDirection.Up) ? -1 : 1;

            var targetPosition = direction == ShutterMovementDirection.Up
                ? ShutterPosition.Opened
                : ShutterPosition.Closed;

            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            var messageData = new ShutterPositioningMessageData(
                targetPosition,
                direction,
                bay.Shutter.Type,
                speedRate,
                MovementMode.ShutterPosition,
                MovementType.Relative,
                delay: 0,
                highSpeedDurationOpen: 0,
                highSpeedDurationClose: 0,
                highSpeedHalfDurationOpen: null,
                highSpeedHalfDurationClose: null,
                lowerSpeed: 0);

            messageData.BypassConditions = bypassConditions;

            this.logger.LogDebug(
                $"Move Shutter " +
                $"direction: {direction}; " +
                $"targetPosition: {targetPosition}; " +
                $"feedRate: {parameters.FeedRate}; " +
                $"speed: {speedRate}; " +
                $"minspeed: {minSpeed}; " +
                $"maxspeed: {maxSpeed}; ");

            this.PublishCommand(
                messageData,
                "Execute Shutter Positioning Move Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.ShutterPositioning,
                bayNumber,
                BayNumber.None);
        }

        /// <summary>
        /// Moves shutter to one of the defined positions
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="bayNumber"></param>
        /// <param name="sender"></param>
        /// <returns>true if movement is started</returns>
        public bool MoveTo(ShutterPosition targetPosition, BayNumber bayNumber, MessageActor sender)
        {
            var direction = ShutterMovementDirection.NotSpecified;
            var shutterInverter = this.baysDataProvider.GetShutterInverterIndex(bayNumber);
            var position = this.sensorsProvider.GetShutterPosition(shutterInverter);

            switch (targetPosition)
            {
                case ShutterPosition.Closed:
                    if (position == ShutterPosition.Half || position == ShutterPosition.Opened || position == ShutterPosition.Closed)
                    {
                        direction = ShutterMovementDirection.Down;
                    }
                    break;

                case ShutterPosition.Half:
                    if (position == ShutterPosition.Opened || position == ShutterPosition.Half)
                    {
                        direction = ShutterMovementDirection.Down;
                    }
                    else if (position == ShutterPosition.Closed)
                    {
                        direction = ShutterMovementDirection.Up;
                    }
                    break;

                case ShutterPosition.Opened:
                    if (position == ShutterPosition.Half || position == ShutterPosition.Closed || position == ShutterPosition.Opened)
                    {
                        direction = ShutterMovementDirection.Up;
                    }
                    break;

                default:
                    break;
            }

            if (direction == ShutterMovementDirection.NotSpecified)
            {
                throw new InvalidOperationException(Resources.Shutters.ResourceManager.GetString("ThePositionIsNotValid", CommonUtils.Culture.Actual));
            }

            var parameters = this.baysDataProvider.GetAssistedMovementsShutter(bayNumber);
            var maxSpeed = this.baysDataProvider.GetShutterMaxSpeed(bayNumber);
            var minSpeed = this.baysDataProvider.GetShutterMinSpeed(bayNumber);

            var speedRate = parameters.FeedRate * maxSpeed;
            var lowSpeed = parameters.FeedRate * minSpeed;

            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            if (bay.Shutter.Type == ShutterType.NotSpecified)
            {
                throw new InvalidOperationException(Resources.Shutters.ResourceManager.GetString("TheShutterTypeIsNotValid", CommonUtils.Culture.Actual));
            }

            // speed is negative to go up
            speedRate *= direction == ShutterMovementDirection.Up ? -1 : 1;
            lowSpeed *= direction == ShutterMovementDirection.Up ? -1 : 1;

            var messageData = new ShutterPositioningMessageData(
                targetPosition,
                direction,
                bay.Shutter.Type,
                speedRate,
                MovementMode.ShutterPosition,
                MovementType.Absolute,
                0,
                parameters.HighSpeedDurationOpen,
                parameters.HighSpeedDurationClose,
                parameters.HighSpeedHalfDurationOpen,
                parameters.HighSpeedHalfDurationClose,
                lowSpeed);

            this.logger.LogDebug(
                $"MoveTo Shutter " +
                $"direction: {direction}; " +
                $"targetPosition: {targetPosition}; " +
                $"feedRate: {parameters.FeedRate}; " +
                $"speed: {speedRate}; " +
                $"minspeed: {minSpeed}; " +
                $"maxspeed: {maxSpeed}; " +
                $"highspeeddurationopen: {parameters.HighSpeedDurationOpen}; " +
                $"highspeeddurationclose: {parameters.HighSpeedDurationClose};" +
                $"highspeedHalfdurationopen: {parameters.HighSpeedHalfDurationOpen}; " +
                $"highspeedHalfdurationclose: {parameters.HighSpeedHalfDurationClose}");

            this.PublishCommand(
                messageData,
                "Execute Shutter Positioning MoveTo Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.ShutterPositioning,
                bayNumber,
                BayNumber.None);

            return true;
        }

        public void ResetTest(BayNumber bayNumber)
        {
            var procedureParameters = this.setupProceduresDataProvider.GetBayShutterTest(bayNumber);

            this.setupProceduresDataProvider.ResetPerformedCycles(procedureParameters);
        }

        public void RunTest(int delayInSeconds, int testCycleCount, BayNumber bayNumber, MessageActor sender)
        {
            if (delayInSeconds < 0)
            {
                throw new InvalidOperationException(Resources.Shutters.ResourceManager.GetString("TheDelayBetweenTestCyclesMustBeStrictlyPositive", CommonUtils.Culture.Actual));
            }

            if (testCycleCount <= 0)
            {
                throw new InvalidOperationException(Resources.Shutters.ResourceManager.GetString("TheNumberOfTestCyclesMustBeStrictlyPositive", CommonUtils.Culture.Actual));
            }

            var parameters = this.baysDataProvider.GetAssistedMovementsShutter(bayNumber);
            var maxSpeed = this.baysDataProvider.GetShutterMaxSpeed(bayNumber);
            var minSpeed = this.baysDataProvider.GetShutterMinSpeed(bayNumber);

            var speedRate = parameters.FeedRate * maxSpeed;

            var lowSpeed = parameters.FeedRate * minSpeed;

            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            var delayInMilliseconds = delayInSeconds * 1000;

            var messageData = new ShutterPositioningMessageData(
                ShutterPosition.NotSpecified,
                ShutterMovementDirection.NotSpecified,
                bay.Shutter.Type,
                speedRate,
                MovementMode.ShutterTest,
                MovementType.Absolute,
                delayInMilliseconds,
                parameters.HighSpeedDurationOpen,
                parameters.HighSpeedDurationClose,
                parameters.HighSpeedHalfDurationOpen,
                parameters.HighSpeedHalfDurationClose,
                lowSpeed);

            this.logger.LogDebug(
                $"RunTest Shutter " +
                $"feedRate: {parameters.FeedRate}; " +
                $"speed: {speedRate}; " +
                $"minspeed: {minSpeed}; " +
                $"maxspeed: {maxSpeed}; " +
                $"highspeeddurationopen: {parameters.HighSpeedDurationOpen}; " +
                $"highspeeddurationclose: {parameters.HighSpeedDurationClose}" +
                $"highspeedHalfdurationopen: {parameters.HighSpeedHalfDurationOpen}; " +
                $"highspeedHalfdurationclose: {parameters.HighSpeedHalfDurationClose}");

            this.PublishCommand(
                messageData,
                "Execute Shutter Test Loop Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.ShutterPositioning,
                bayNumber,
                BayNumber.None);
        }

        public void Stop(BayNumber bayNumber, MessageActor sender)
        {
            var messageData = new StopMessageData(StopRequestReason.Stop);
            this.PublishCommand(
                messageData,
                "Stop shutter Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Stop,
                bayNumber,
                BayNumber.None);
        }

        #endregion
    }
}
