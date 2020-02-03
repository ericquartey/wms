using System;
using System.ComponentModel;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class CarouselProvider : BaseProvider, ICarouselProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<CarouselProvider> logger;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public CarouselProvider(
            IBaysDataProvider baysDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            IMachineResourcesProvider machineResourcesProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IEventAggregator eventAggregator,
            ILogger<CarouselProvider> logger)
            : base(eventAggregator)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public MachineErrorCode CanElevatorDeposit(BayPosition bayPosition)
        {
            var bayNumber = bayPosition.Bay.Number;
            var returnValue = MachineErrorCode.NoError;
            if (!this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber))
            {
                returnValue = MachineErrorCode.SensorZeroBayNotActiveAtStart;
            }
            else if (bayPosition.IsUpper
                && this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                )
            {
                returnValue = MachineErrorCode.TopLevelBayOccupied;
            }
            else if (!bayPosition.IsUpper
                && this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber)
                )
            {
                returnValue = MachineErrorCode.BottomLevelBayOccupied;
            }
            return returnValue;
        }

        public MachineErrorCode CanElevatorPickup(BayPosition bayPosition)
        {
            var bayNumber = bayPosition.Bay.Number;
            var returnValue = MachineErrorCode.NoError;
            if (!this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber))
            {
                returnValue = MachineErrorCode.SensorZeroBayNotActiveAtStart;
            }
            else if (bayPosition.IsUpper
                && !this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                )
            {
                returnValue = MachineErrorCode.TopLevelBayEmpty;
            }
            else if (!bayPosition.IsUpper
                && !this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber)
                )
            {
                returnValue = MachineErrorCode.BottomLevelBayEmpty;
            }
            return returnValue;
        }

        public ActionPolicy CanMove(VerticalMovementDirection direction, BayNumber bayNumber, MovementCategory movementCategory)
        {
            var bay = this.baysDataProvider.GetByNumber(bayNumber);
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
                    if (
#if CHECK_BAY_SENSOR
                        isLoadingUnitInUpperPosition ||
#endif
                        bay.Positions.FirstOrDefault(p => p.IsUpper).LoadingUnit != null
                        )
                    {
                        return new ActionPolicy { Reason = Resources.Bays.TheBayContainsALoadingUnitInItsUpperPosition };
                    }

                    break;

                default:
                    throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(VerticalMovementDirection));
            }

            if (!this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber) && movementCategory != MovementCategory.Manual)
            {
                return new ActionPolicy { Reason = Resources.Bays.TheBayChainIsNotInZeroPosition };
            }

            return ActionPolicy.Allowed;
        }

        public double GetPosition(BayNumber bayNumber)
        {
            return this.baysDataProvider.GetChainPosition(bayNumber);
        }

        public void Homing(Calibration calibration, int? loadingUnitId, bool showErrors, BayNumber bayNumber, MessageActor sender)
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.BayChain, calibration, loadingUnitId, showErrors);
            this.PublishCommand(
                homingData,
                $"Execute homing {calibration} Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Homing,
                bayNumber,
                BayNumber.None);
        }

        public bool IsOnlyBottomPositionOccupied(BayNumber bayNumber)
        {
            return (this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber)
                && !this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                && this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber)
                );
        }

        public bool IsOnlyTopPositionOccupied(BayNumber bayNumber)
        {
            return (this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber)
                && this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                && !this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber)
                );
        }

        public void Move(VerticalMovementDirection direction, int? loadingUnitId, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanMove(direction, bayNumber, MovementCategory.Automatic);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }
            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            var targetPosition = bay.Carousel.ElevatorDistance * (direction is VerticalMovementDirection.Up ? 1 : -1);

            // if weight is unknown we move as full weight
            double scalingFactor = 1;
            if (loadingUnitId.HasValue)
            {
                var loadUnit = this.loadingUnitsDataProvider.GetById(loadingUnitId.Value);
                if (loadUnit.MaxNetWeight + loadUnit.Tare > 0 && loadUnit.GrossWeight > 0)
                {
                    scalingFactor = loadUnit.GrossWeight / (loadUnit.MaxNetWeight + loadUnit.Tare);
                }
            }
            var speed = new[] { bay.EmptyLoadMovement.Speed - ((bay.EmptyLoadMovement.Speed - bay.FullLoadMovement.Speed) * scalingFactor) };
            var acceleration = new[] { bay.EmptyLoadMovement.Acceleration - ((bay.EmptyLoadMovement.Acceleration - bay.FullLoadMovement.Acceleration) * scalingFactor) };
            var deceleration = new[] { bay.EmptyLoadMovement.Deceleration - ((bay.EmptyLoadMovement.Deceleration - bay.FullLoadMovement.Deceleration) * scalingFactor) };
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

            this.logger.LogDebug(
                $"Move Carousel " +
                $"bayNumber: {bayNumber}; " +
                $"direction: {direction}; " +
                $"targetPosition: {targetPosition}; " +
                $"speed: {speed}; " +
                $"acceleration: {acceleration}; " +
                $"deceleration: {deceleration};");

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.None);
        }

        public void MoveAssisted(VerticalMovementDirection direction, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanMove(direction, bayNumber, MovementCategory.Assisted);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }
            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            var targetPosition = bay.Carousel.ElevatorDistance * (direction is VerticalMovementDirection.Up ? 1 : -1);

            var procedureParameters = this.baysDataProvider.GetAssistedMovementsCarousel(bayNumber);

            var speed = new[] { bay.FullLoadMovement.Speed * procedureParameters.FeedRate };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
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

            this.logger.LogDebug(
                $"Move Carousel Assisted " +
                $"bayNumber: {bayNumber}; " +
                $"direction: {direction}; " +
                $"targetPosition: {targetPosition}; " +
                $"feedrate: {procedureParameters.FeedRate}; " +
                $"speed: {speed}; " +
                $"acceleration: {acceleration}; " +
                $"deceleration: {deceleration};");

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.None);
        }

        public void MoveManual(VerticalMovementDirection direction, double distance, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanMove(direction, bayNumber, MovementCategory.Manual);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var bay = this.baysDataProvider.GetByNumber(bayNumber);
            var targetPosition = bay.Carousel.ElevatorDistance;
            if (distance > 0 && distance < bay.Carousel.ElevatorDistance + Math.Abs(bay.ChainOffset))
            {
                targetPosition = distance;
            }

            targetPosition *= direction is VerticalMovementDirection.Up ? 1 : -1;

            var procedureParameters = this.baysDataProvider.GetManualMovementsCarousel(bayNumber);

            var speed = new[] { bay.FullLoadMovement.Speed * procedureParameters.FeedRate };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
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

            this.logger.LogDebug(
                $"Move Carousel Manual " +
                $"bayNumber: {bayNumber}; " +
                $"direction: {direction}; " +
                $"targetPosition: {targetPosition}; " +
                $"feedrate: {procedureParameters.FeedRate}; " +
                $"speed: {speed}; " +
                $"acceleration: {acceleration}; " +
                $"deceleration: {deceleration};");

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
