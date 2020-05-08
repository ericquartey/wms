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
    internal class ExternalBayProvider : BaseProvider, IExternalBayProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<ExternalBayProvider> logger;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ExternalBayProvider(
            IBaysDataProvider baysDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            IMachineResourcesProvider machineResourcesProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IEventAggregator eventAggregator,
            ILogger<ExternalBayProvider> logger)
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

        public MachineErrorCode CanElevatorDeposit(BayNumber bayNumber)    /*BayPosition bayPosition*/
        {
            /*var bayNumber = bayPosition.Bay.Number;*/
            var returnValue = MachineErrorCode.NoError;
            // Check the zero sensor
            if (!this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber))
            {
                returnValue = MachineErrorCode.SensorZeroBayNotActiveAtStart;
            }
            else
            {
                // Check if bay is fully
                if (this.machineResourcesProvider.IsDrawerInBayInternalPosition(bayNumber) ||
                    this.machineResourcesProvider.IsDrawerInBayExternalPosition(bayNumber))
                {
                    returnValue = MachineErrorCode.ExternalBayOccupied;
                }
            }

            return returnValue;
        }

        public MachineErrorCode CanElevatorPickup(BayNumber bayNumber)   /*BayPosition bayPosition*/
        {
            /*var bayNumber = bayPosition.Bay.Number;*/
            var returnValue = MachineErrorCode.NoError;
            // Check the zero sensor
            if (!this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber))
            {
                returnValue = MachineErrorCode.SensorZeroBayNotActiveAtStart;
            }
            else
            {
                // Check if bay is emptied
                if (!this.machineResourcesProvider.IsDrawerInBayInternalPosition(bayNumber))
                {
                    returnValue = MachineErrorCode.ExternalBayEmpty;
                }
            }

            return returnValue;
        }

        public ActionPolicy CanMove(ExternalBayMovementDirection direction, BayNumber bayNumber, MovementCategory movementCategory)
        {
            var bay = this.baysDataProvider.GetByNumber(bayNumber);
            if (bay is null)
            {
                return new ActionPolicy { Reason = "The bay is null!" };
            }
            //if (bay.Carousel is null)   // ADD define the interface  bay.External
            //{
            //    return new ActionPolicy { Reason = Resources.Bays.TheSpecifiedBayHasNoCarousel };
            //}

            // Always allow the manual movements
            if (movementCategory == MovementCategory.Manual)
            {
                return ActionPolicy.Allowed;
            }

            var isLoadingUnitInExternalPosition = this.machineResourcesProvider.IsDrawerInBayExternalPosition(bayNumber);
            var isLoadingUnitInInternalPosition = this.machineResourcesProvider.IsDrawerInBayInternalPosition(bayNumber);

            switch (direction)
            {
                case ExternalBayMovementDirection.TowardMachine:
                    if (isLoadingUnitInInternalPosition)
                    {
                        return new ActionPolicy { Reason = Resources.Bays.TheBayContainsAtLeastOneLoadingUnit };
                    }

                    break;

                case ExternalBayMovementDirection.TowardOperator:
                    if (
#if CHECK_BAY_SENSOR
                        isLoadingUnitInExternalPosition ||
#endif
                        bay.Positions.FirstOrDefault().LoadingUnit != null          // .FirstOrDefault(p => p.IsExternal).LoadingUnit
                        )
                    {
                        return new ActionPolicy { Reason = Resources.Bays.TheBayContainsALoadingUnitInItsExternalPosition };
                    }

                    break;

                default:
                    throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(ExternalBayMovementDirection));
            }

            if (!this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber) &&
                movementCategory != MovementCategory.Manual &&
                direction == ExternalBayMovementDirection.TowardOperator)
            {
                //return new ActionPolicy { Reason = Resources.Bays.TheBayChainIsNotInZeroPosition };
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

        public bool IsExternalPositionOccupied(BayNumber bayNumber)
        {
            var val = this.machineResourcesProvider.IsDrawerInBayExternalPosition(bayNumber);
            return val;
        }

        public bool IsInternalPositionOccupied(BayNumber bayNumber)
        {
            var val = this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber) &&
                this.machineResourcesProvider.IsDrawerInBayInternalPosition(bayNumber);
            return val;
        }

        public void Move(ExternalBayMovementDirection direction, int? loadUnitId, BayNumber bayNumber, MessageActor sender)
        {
            // For automatic movements

            var policy = this.CanMove(direction, bayNumber, MovementCategory.Automatic);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }
            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            var targetPosition = (direction == ExternalBayMovementDirection.TowardOperator) ? bay.Carousel.ElevatorDistance : 0;   // ADD   bay.External.Race

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
                MovementType.Absolute,
                MovementMode.ExtBayChain,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction is ExternalBayMovementDirection.TowardOperator ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards);

            if (loadUnitId.HasValue)
            {
                messageData.LoadingUnitId = loadUnitId;
            }

            this.logger.LogDebug(
                $"Move External Bay " +
                $"bayNumber: {bayNumber}; " +
                $"direction: {direction}; " +
                $"LoadUnitId: {loadUnitId}; " +
                $"targetPosition: {targetPosition}; " +
                $"speed: {speed[0]:0.00}; " +
                $"acceleration: {acceleration[0]:0.00}; " +
                $"deceleration: {deceleration[0]:0.00};");

            this.PublishCommand(
                messageData,
                $"Execute External {Axis.BayChain} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.None);
        }

        public void MoveAssisted(ExternalBayMovementDirection direction, BayNumber bayNumber, MessageActor sender)
        {
            // For assisted movements

            var policy = this.CanMove(direction, bayNumber, MovementCategory.Assisted);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }
            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            var race = 1100.0d; // bay.External.Race
            var targetPosition = (direction == ExternalBayMovementDirection.TowardOperator) ? race : -race;

            // Remove this comment
            //var procedureParameters = this.baysDataProvider.GetAssistedMovementsCarousel(bayNumber);   // .GetAssistedMovementsExternalBay(bayNumber)
            var feedRate = 1;    // feedRate = procedureParameters.FeedRate

            var speed = new[] { bay.FullLoadMovement.Speed * feedRate };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,  // .Absolute MODIFY THIS!!!
                MovementMode.ExtBayChain,   // CHECK!! defined a new movement mode
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction is ExternalBayMovementDirection.TowardOperator ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards);

            this.logger.LogDebug(
                $"Move External Bay Assisted " +
                $"bayNumber: {bayNumber}; " +
                $"direction: {direction}; " +
                $"targetPosition: {targetPosition}; " +
                $"feedrate: {feedRate}; " +
                $"speed: {speed[0]:0.00}; " +
                $"acceleration: {acceleration[0]:0.00}; " +
                $"deceleration: {deceleration[0]:0.00};");

            this.PublishCommand(
                messageData,
                $"Execute external {Axis.BayChain} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.None);
        }

        public void MoveManual(ExternalBayMovementDirection direction, double distance, int? loadUnitId, bool bypassConditions, BayNumber bayNumber, MessageActor sender)
        {
            // For manual movements

            if (!bypassConditions)
            {
                var policy = this.CanMove(direction, bayNumber, MovementCategory.Manual);
                if (!policy.IsAllowed)
                {
                    throw new InvalidOperationException(policy.Reason);
                }
            }

            var bay = this.baysDataProvider.GetByNumber(bayNumber);
            var race = 1100.0d; // bay.Carousel.ElevatorDistance  // ADD    bay.External.Race
            var targetPosition = race;
            // Check the module of distance to be moved
            if (distance > 0 && distance < race + Math.Abs(bay.ChainOffset))   //   bay.External.Race + Math.Abs(...
            {
                targetPosition = distance;
            }

            targetPosition *= direction is ExternalBayMovementDirection.TowardOperator ? 1 : -1;

            // Uncomment this line
            //var procedureParameters = this.baysDataProvider.GetManualMovementsCarousel(bayNumber);   // ADD  .GetManualMovementsExternalBay(bayNumber)
            var feedRate = 0.25d;

            var speed = new[] { bay.FullLoadMovement.Speed * feedRate };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,
                MovementMode.ExtBayChainManual,   // ADDED!! New movement mode
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction is ExternalBayMovementDirection.TowardOperator ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards);

            if (loadUnitId.HasValue)
            {
                messageData.LoadingUnitId = loadUnitId;
            }
            messageData.BypassConditions = bypassConditions;

            this.logger.LogDebug(
                $"Move External Bay Manual " +
                $"bayNumber: {bayNumber}; " +
                $"direction: {direction}; " +
                $"LoadUnitId: {loadUnitId}; " +
                $"targetPosition: {targetPosition}; " +
                $"feedrate: {feedRate}; " +
                $"speed: {speed[0]:0.00}; " +
                $"acceleration: {acceleration[0]:0.00}; " +
                $"deceleration: {deceleration[0]:0.00};");

            this.PublishCommand(
                messageData,
                $"Execute External {Axis.BayChain} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.None);
        }

        public void MovementForExtraction(double distance, int? loadUnitId, BayNumber bayNumber, MessageActor sender)
        {
            // TODO: Add your implementation code here
            return;
        }

        public void MovementForInsertion(BayNumber bayNumber, MessageActor sender)
        {
            // TODO: Add your implementation code here
            return;
        }

        public void StartTest(BayNumber bayNumber, MessageActor sender)
        {
            // TODO: Add your implementation code here
            return;
        }

        public void Stop(BayNumber bayNumber, MessageActor sender)
        {
            var messageData = new StopMessageData(StopRequestReason.Stop);
            this.PublishCommand(
                messageData,
                $"Stop on external bay {bayNumber}",
                MessageActor.DeviceManager,
                sender,
                MessageType.Stop,
                bayNumber,
                BayNumber.None);
        }

        public void StopTest(BayNumber bayNumber, MessageActor sender)
        {
            // TODO: Add your implementation code here
            return;
        }

        #endregion
    }
}
