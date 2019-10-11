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
    internal class BayChainProvider : BaseProvider, IBayChainProvider
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public BayChainProvider(
            IBaysProvider baysProvider,
            IElevatorDataProvider elevatorDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Properties

        public double HorizontalPosition { get; set; }

        #endregion

        #region Methods

        public void Move(HorizontalMovementDirection direction, BayNumber bayNumber, MessageActor sender)
        {
            var bay = this.baysProvider.GetByNumber(bayNumber);
            if (bay.Carousel is null)
            {
                throw new InvalidOperationException($"Cannot operate carousel on bay {bayNumber} because it has no carousel.");
            }

            var targetPosition = bay.Carousel.ElevatorDistance;

            targetPosition *= (direction == HorizontalMovementDirection.Forwards) ? -1 : 1;

            var axis = this.elevatorDataProvider.GetHorizontalAxis();

            var procedureParameters = this.setupProceduresDataProvider.GetHorizontalManualMovements();

            // TODO: scale movement speed by weight
            var speed = new[] { axis.EmptyLoadMovement.Speed * procedureParameters.FeedRate };
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
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.None);
        }

        public void MoveManual(HorizontalMovementDirection direction, BayNumber bayNumber, MessageActor sender)
        {
            var bay = this.baysProvider.GetByNumber(bayNumber);
            if (bay.Carousel is null)
            {
                throw new InvalidOperationException($"Cannot operate carousel on bay {bayNumber} because it has no carousel.");
            }

            var targetPosition = bay.Carousel.ElevatorDistance;

            targetPosition *= ((direction == HorizontalMovementDirection.Forwards) ? -1 : 1);

            var axis = this.elevatorDataProvider.GetHorizontalAxis();
            var procedureParameters = this.setupProceduresDataProvider.GetHorizontalManualMovements();
            var speed = new[] { axis.MaximumLoadMovement.Speed * procedureParameters.FeedRate };
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
                sender,
                MessageType.Positioning,
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
