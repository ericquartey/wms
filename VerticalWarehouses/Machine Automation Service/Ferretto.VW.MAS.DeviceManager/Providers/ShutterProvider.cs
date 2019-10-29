using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class ShutterProvider : BaseProvider, IShutterProvider
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ShutterProvider(
            IBaysProvider baysProvider,
            ISensorsProvider sensorsProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Methods

        public void Move(ShutterMovementDirection direction, BayNumber bayNumber, MessageActor sender)
        {
            var parameters = this.setupProceduresDataProvider.GetShutterManualMovements();

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
                MovementMode.ShutterPosition,
                MovementType.Relative,
                delay: 0,
                highSpeedDurationOpen: 0,
                highSpeedDurationClose: 0,
                lowerSpeed: 0);

            this.PublishCommand(
                messageData,
                "Execute Shutter Positioning Move Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.ShutterPositioning,
                bayNumber,
                BayNumber.None);
        }

        // return true if movement is started
        public bool MoveTo(ShutterPosition targetPosition, BayNumber bayNumber, MessageActor sender)
        {
            var direction = ShutterMovementDirection.NotSpecified;
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

            if (direction == ShutterMovementDirection.NotSpecified)
            {
                throw new InvalidOperationException(Resources.Shutters.ThePositionIsNotValid);
            }

            var parameters = this.setupProceduresDataProvider.GetShutterManualMovements();

            var speedRate = parameters.FeedRate * parameters.MaxSpeed;
            var lowSpeed = parameters.FeedRate * parameters.MinSpeed;

            var bay = this.baysProvider.GetByNumber(bayNumber);

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
                lowSpeed);

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

            var parameters = this.setupProceduresDataProvider.GetShutterManualMovements();

            var speedRate = parameters.FeedRate * parameters.MaxSpeed;

            var lowSpeed = parameters.FeedRate * parameters.MinSpeed;

            var bay = this.baysProvider.GetByNumber(bayNumber);

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
                lowSpeed);

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
                "Stop Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Stop,
                bayNumber,
                BayNumber.None);
        }

        #endregion
    }
}
