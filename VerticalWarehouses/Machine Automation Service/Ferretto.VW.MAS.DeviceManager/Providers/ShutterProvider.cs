using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class ShutterProvider : BaseProvider, IShutterProvider
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ShutterProvider(
            IBaysProvider baysProvider,
            ISensorsProvider sensorsProvider,
            IElevatorDataProvider elevatorDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Methods

        public void Move(ShutterMovementDirection direction, BayNumber bayNumber, MessageActor sender)
        {
            var parameters = this.setupProceduresDataProvider.GetAll().ShutterManualMovements;

            var speedRate = parameters.FeedRate * parameters.MinSpeed;

            // speed is negative to go up
            speedRate *= (direction == ShutterMovementDirection.Up) ? -1 : 1;

            var targetPosition = direction == ShutterMovementDirection.Up
                ? ShutterPosition.Opened
                : ShutterPosition.Closed;

            var bay = this.baysProvider.GetByNumber(bayNumber);

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
                "Execute Shutter Positioning Move Command",
                MessageActor.FiniteStateMachines,
                sender,
                MessageType.ShutterPositioning,
                bayNumber,
                BayNumber.None);
        }

        public void MoveTo(ShutterPosition targetPosition, BayNumber bayNumber, MessageActor sender)
        {
            var direction = ShutterMovementDirection.None;
            var position = this.sensorsProvider.GetShutterPosition(bayNumber);
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
                    throw new InvalidOperationException(Resources.Shutters.ThePositionIsNotValid);
                }
                else
                {
                    // destination already reached
                    return;
                }
            }

            var parameters = this.setupProceduresDataProvider.GetAll().ShutterManualMovements;

            var speedRate = parameters.FeedRate * parameters.MaxSpeed;
            if (speedRate == 0)
            {
                throw new InvalidOperationException(Resources.Shutters.TheSpeedRateIsNotValid);
            }

            var lowSpeed = parameters.FeedRate * parameters.MinSpeed;
            if (lowSpeed == 0)
            {
                throw new InvalidOperationException(Resources.Shutters.TheMinSpeedIsNotValid);
            }

            var bay = this.baysProvider.GetByNumber(bayNumber);

            // speed is negative to go up
            speedRate *= (direction == ShutterMovementDirection.Up) ? -1 : 1;
            lowSpeed *= (direction == ShutterMovementDirection.Up) ? -1 : 1;

            var messageData = new ShutterPositioningMessageData(
                targetPosition,
                direction,
                bay.Shutter.Type,
                speedRate,
                parameters.HigherDistance,
                parameters.LowerDistance,
                MovementMode.ShutterPosition,
                MovementType.Absolute,
                0,
                0,
                parameters.HighSpeedDurationOpen,
                parameters.HighSpeedDurationClose,
                lowSpeed);

            this.PublishCommand(
                messageData,
                "Execute Shutter Positioning MoveTo Command",
                MessageActor.FiniteStateMachines,
                sender,
                MessageType.ShutterPositioning,
                bayNumber,
                BayNumber.None);
        }

        public void RunTest(int delayInSeconds, int testCycleCount, BayNumber bayNumber, MessageActor sender)
        {
            if (delayInSeconds <= 0)
            {
                throw new InvalidOperationException(Resources.Shutters.TheDelayBetweenTestCyclesMustBeStrictlyPositive);
            }

            if (testCycleCount <= 0)
            {
                throw new InvalidOperationException(Resources.Shutters.TheNumberOfTestCyclesMustBeStrictlyPositive);
            }

            var parameters = this.setupProceduresDataProvider.GetAll().ShutterManualMovements;

            var speedRate = parameters.FeedRate * parameters.MaxSpeed;

            var lowSpeed = parameters.FeedRate * parameters.MinSpeed;

            var bay = this.baysProvider.GetByNumber(bayNumber);

            var delayInMilliseconds = delayInSeconds * 1000;

            var messageData = new ShutterPositioningMessageData(
                ShutterPosition.None,
                ShutterMovementDirection.None,
                bay.Shutter.Type,
                speedRate,
                parameters.HigherDistance,
                parameters.LowerDistance,
                MovementMode.ShutterTest,
                MovementType.Absolute,
                testCycleCount,
                delayInMilliseconds,
                parameters.HighSpeedDurationOpen,
                parameters.HighSpeedDurationClose,
                lowSpeed);

            this.PublishCommand(
                messageData,
                "Execute Shutter Test Loop Command",
                MessageActor.FiniteStateMachines,
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
                "Stop Command",
                MessageActor.FiniteStateMachines,
                sender,
                MessageType.Stop,
                bayNumber,
                BayNumber.None);
        }

        #endregion
    }
}
