using System;
using System.ComponentModel;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class CarouselProvider : BaseProvider, ICarouselProvider
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public CarouselProvider(
            IBaysProvider baysProvider,
            IElevatorDataProvider elevatorDataProvider,
            IMachineResourcesProvider machineResourcesProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Methods

        public ActionPolicy CanMove(VerticalMovementDirection direction, BayNumber bayNumber)
        {
            var bay = this.baysProvider.GetByNumber(bayNumber);
            if (bay.Carousel is null)
            {
                return new ActionPolicy { Reason = Resources.Bays.TheSpecifiedBayHasNoCarousel };
            }

            var isLoadingUnitInLowerPosition = this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber);
            var isLoadingUnitInUpperPosition = this.machineResourcesProvider.IsDrawerInBayTop(bayNumber);

            switch (direction)
            {
                case VerticalMovementDirection.Down:
                    if (isLoadingUnitInLowerPosition || isLoadingUnitInUpperPosition)
                    {
                        return new ActionPolicy { Reason = Resources.Bays.TheBayContainsAtLeastOneLoadingUnit };
                    }

                    break;

                case VerticalMovementDirection.Up:
                    if (isLoadingUnitInUpperPosition)
                    {
                        return new ActionPolicy { Reason = Resources.Bays.TheBayContainsALoadingUnitInItsUpperPosition };
                    }

                    break;

                default:
                    throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(VerticalMovementDirection));
            }

            if (!this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber))
            {
                return new ActionPolicy { Reason = Resources.Bays.TheBayChainIsNotInZeroPosition };
            }

            return ActionPolicy.Allowed;
        }

        public double GetPosition(BayNumber bayNumber)
        {
            return this.baysProvider.GetChainPosition(bayNumber);
        }

        public void Homing(Calibration calibration, BayNumber bayNumber, MessageActor sender)
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.BayChain, calibration);
            this.PublishCommand(
                homingData,
                $"Execute homing {calibration} Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Homing,
                bayNumber,
                BayNumber.None);
        }

        public void Move(VerticalMovementDirection direction, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanMove(direction, bayNumber);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var bay = this.baysProvider.GetByNumber(bayNumber);
            var targetPosition = bay.Carousel.ElevatorDistance;

            targetPosition *= direction is VerticalMovementDirection.Up ? 1 : -1;

            var axis = this.elevatorDataProvider.GetHorizontalAxis();

            var procedureParameters = this.setupProceduresDataProvider.GetHorizontalManualMovements();

            // TODO: scale movement speed by weight
            var speed = new[] { axis.EmptyLoadMovement.Speed * procedureParameters.FeedRate };
            var acceleration = new[] { axis.EmptyLoadMovement.Acceleration };
            var deceleration = new[] { axis.EmptyLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,
                MovementMode.BayChain,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction is VerticalMovementDirection.Up ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.None);
        }

        public void MoveManual(VerticalMovementDirection direction, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanMove(direction, bayNumber);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var bay = this.baysProvider.GetByNumber(bayNumber);
            var targetPosition = bay.Carousel.ElevatorDistance;

            targetPosition *= direction is VerticalMovementDirection.Up ? 1 : -1;

            var axis = this.elevatorDataProvider.GetHorizontalAxis();
            var procedureParameters = this.setupProceduresDataProvider.GetHorizontalManualMovements();
            var speed = new[] { axis.FullLoadMovement.Speed * procedureParameters.FeedRate };
            var acceleration = new[] { axis.FullLoadMovement.Acceleration };
            var deceleration = new[] { axis.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,
                MovementMode.BayChainManual,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction is VerticalMovementDirection.Up ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
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
                $"Stop carousel on bay {bayNumber}",
                MessageActor.DeviceManager,
                sender,
                MessageType.Stop,
                bayNumber,
                BayNumber.None);
        }

        #endregion
    }
}
