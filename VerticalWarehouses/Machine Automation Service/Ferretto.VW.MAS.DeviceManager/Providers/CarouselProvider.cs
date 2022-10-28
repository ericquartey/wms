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

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public CarouselProvider(
            IBaysDataProvider baysDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            IMachineResourcesProvider machineResourcesProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IEventAggregator eventAggregator,
            ILogger<CarouselProvider> logger)
            : base(eventAggregator)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
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

        public ActionPolicy CanMove(VerticalMovementDirection direction, Bay bay, MovementCategory movementCategory)
        {
            if (bay.Carousel is null)
            {
                return new ActionPolicy { Reason = Resources.Bays.ResourceManager.GetString("TheSpecifiedBayHasNoCarousel", CommonUtils.Culture.Actual) };
            }

            var isLoadingUnitInLowerPosition = this.machineResourcesProvider.IsDrawerInBayBottom(bay.Number);
            var isLoadingUnitInUpperPosition = this.machineResourcesProvider.IsDrawerInBayTop(bay.Number);

            switch (direction)
            {
                case VerticalMovementDirection.Down:
                    if (isLoadingUnitInLowerPosition
                        || (isLoadingUnitInUpperPosition && movementCategory != MovementCategory.Manual)
                        )
                    {
                        return new ActionPolicy { Reason = Resources.Bays.ResourceManager.GetString("TheBayContainsAtLeastOneLoadingUnit", CommonUtils.Culture.Actual) };
                    }

                    break;

                case VerticalMovementDirection.Up:
                    if (
#if CHECK_BAY_SENSOR
                        (isLoadingUnitInUpperPosition && movementCategory != MovementCategory.Manual) ||
#endif
                        bay.Positions.FirstOrDefault(p => p.IsUpper).LoadingUnit != null
                        )
                    {
                        return new ActionPolicy { Reason = Resources.Bays.ResourceManager.GetString("TheBayContainsALoadingUnitInItsUpperPosition", CommonUtils.Culture.Actual) };
                    }

                    if (movementCategory != MovementCategory.Manual && bay.Positions.Any(p => p.IsBlocked))
                    {
                        return new ActionPolicy { Reason = Resources.Bays.ResourceManager.GetString("TheBayPositionIsBlocked", CommonUtils.Culture.Actual) };
                    }

                    break;

                default:
                    throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(VerticalMovementDirection));
            }

            if (!this.machineResourcesProvider.IsSensorZeroOnBay(bay.Number) && movementCategory != MovementCategory.Manual)
            {
                return new ActionPolicy { Reason = Resources.Bays.ResourceManager.GetString("TheBayChainIsNotInZeroPosition", CommonUtils.Culture.Actual) };
            }

            return ActionPolicy.Allowed;
        }

        public double GetPosition(BayNumber bayNumber)
        {
            return this.baysDataProvider.GetChainPosition(bayNumber);
        }

        public void Homing(Calibration calibration, int? loadingUnitId, bool showErrors, BayNumber bayNumber, MessageActor sender, bool bypassSensor)
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.BayChain, calibration, loadingUnitId, showErrors, false, false, bypassSensor);
            this.PublishCommand(
                homingData,
                $"Execute homing {calibration} Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Homing,
                bayNumber,
                BayNumber.None);
        }

        public void MoveFindZero(BayNumber requestingBay, MessageActor sender)
        {
            //var policy = ActionPolicy.Allowed;

            //if (!policy.IsAllowed)
            //{
            //    throw new InvalidOperationException(policy.Reason);
            //}

            var bay = this.baysDataProvider.GetByNumberCarousel(requestingBay);

            var chainPosition = this.baysDataProvider.GetChainPosition(requestingBay);
            var targetPosition = chainPosition - 20;

            var speed = new[] { bay.Carousel.HomingCreepSpeed };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Absolute,
                MovementMode.BayChainFindZero,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.BayChain} Vertical find zero Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                requestingBay,
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

        public void Move(VerticalMovementDirection direction, int? loadUnitId, Bay bay, MessageActor sender)
        {
            var policy = this.CanMove(direction, bay, MovementCategory.Automatic);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }
            if (bay.Carousel is null || bay.EmptyLoadMovement is null)
            {
                bay = this.baysDataProvider.GetByNumberCarousel(bay.Number);
            }

            var targetPosition = bay.Carousel.ElevatorDistance * (direction is VerticalMovementDirection.Up ? 1 : -1);

            // if weight is unknown we move as full weight
            double scalingFactor = 1;
            if (loadUnitId.HasValue)
            {
                var loadUnit = this.loadingUnitsDataProvider.GetById(loadUnitId.Value);
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

            if (loadUnitId.HasValue)
            {
                messageData.LoadingUnitId = loadUnitId;
            }

            this.logger.LogDebug(
                $"Move Carousel " +
                $"bayNumber: {bay.Number}; " +
                $"direction: {direction}; " +
                $"LoadUnitId: {loadUnitId}; " +
                $"targetPosition: {targetPosition:0.00}; " +
                $"speed: {speed[0]:0.00}; " +
                $"acceleration: {acceleration[0]:0.00}; " +
                $"deceleration: {deceleration[0]:0.00};");

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                bay.Number,
                BayNumber.None);
        }

        public void MoveAssisted(VerticalMovementDirection direction, BayNumber bayNumber, MessageActor sender)
        {
            var bay = this.baysDataProvider.GetByNumberCarousel(bayNumber);
            var policy = this.CanMove(direction, bay, MovementCategory.Assisted);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var targetPosition = bay.Carousel.ElevatorDistance * (direction is VerticalMovementDirection.Up ? 1 : -1);

            var procedureParameters = bay.Carousel.AssistedMovements;

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
                $"speed: {speed[0]:0.00}; " +
                $"acceleration: {acceleration[0]:0.00}; " +
                $"deceleration: {deceleration[0]:0.00};");

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.None);
        }

        public void StartTest(BayNumber bayNumber, MessageActor sender)
        {
            var bay = this.baysDataProvider.GetByNumberCarousel(bayNumber);
            var policy = this.CanMove(VerticalMovementDirection.Up, bay, MovementCategory.Assisted);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var targetPosition = bay.Carousel.ElevatorDistance;

            var procedureParameters = this.setupProceduresDataProvider.GetBayCarouselCalibration(bayNumber);

            var speed = new[] { bay.FullLoadMovement.Speed * procedureParameters.FeedRate };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,
                MovementMode.BayTest,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                procedureParameters.RequiredCycles,
                lowerBound: 0,
                upperBound: 0,
                delay: 0,
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.logger.LogDebug(
                $"Start Carousel Test " +
                $"bayNumber: {bayNumber}; " +
                $"targetPosition: {targetPosition}; " +
                $"feedrate: {procedureParameters.FeedRate}; " +
                $"speed: {speed[0]:0.00}; " +
                $"acceleration: {acceleration[0]:0.00}; " +
                $"deceleration: {deceleration[0]:0.00};");

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.None);
        }

        public void MoveManual(VerticalMovementDirection direction, double distance, int? loadUnitId, bool bypassConditions, Bay bay, MessageActor sender)
        {
            if (!bypassConditions)
            {
                var policy = this.CanMove(direction, bay, MovementCategory.Manual);
                if (!policy.IsAllowed)
                {
                    throw new InvalidOperationException(policy.Reason);
                }
            }
            if (bay.Carousel is null || bay.EmptyLoadMovement is null)
            {
                bay = this.baysDataProvider.GetByNumberCarousel(bay.Number);
            }

            this.machineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = false;

            var targetPosition = bay.Carousel.ElevatorDistance;
            if (distance > 0 && distance < bay.Carousel.ElevatorDistance + Math.Abs(bay.ChainOffset))
            {
                targetPosition = distance;
            }

            targetPosition *= direction is VerticalMovementDirection.Up ? 1 : -1;

            var procedureParameters = bay.Carousel.ManualMovements;

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

            if (loadUnitId.HasValue)
            {
                messageData.LoadingUnitId = loadUnitId;
            }
            messageData.BypassConditions = bypassConditions;

            this.logger.LogDebug(
                $"Move Carousel Manual " +
                $"bayNumber: {bay.Number}; " +
                $"direction: {direction}; " +
                $"LoadUnitId: {loadUnitId}; " +
                $"targetPosition: {targetPosition:0.00}; " +
                $"feedrate: {procedureParameters.FeedRate}; " +
                $"speed: {speed[0]:0.00}; " +
                $"acceleration: {acceleration[0]:0.00}; " +
                $"deceleration: {deceleration[0]:0.00};");

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                bay.Number,
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

        public void StopTest(BayNumber bayNumber, MessageActor sender)
        {
            this.PublishCommand(
                null,
                $"Stop carousel test on bay {bayNumber}",
                MessageActor.DeviceManager,
                sender,
                MessageType.StopTest,
                bayNumber,
                BayNumber.None);
        }

        #endregion
    }
}
