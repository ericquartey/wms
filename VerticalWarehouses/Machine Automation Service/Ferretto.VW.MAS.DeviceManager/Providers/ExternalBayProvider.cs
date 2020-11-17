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

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ExternalBayProvider(
            IBaysDataProvider baysDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            IMachineResourcesProvider machineResourcesProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IEventAggregator eventAggregator,
            ILogger<ExternalBayProvider> logger)
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
            if (bay.External is null)
            {
                return new ActionPolicy { Reason = Resources.Bays.ResourceManager.GetString("TheSpecifiedBayIsNotAnExternalBay", CommonUtils.Culture.Actual) };
            }

            // Always allow the manual movements
            if (movementCategory == MovementCategory.Manual)
            {
                return ActionPolicy.Allowed;
            }

            // Check the position of loading unit onto the bay and the given movement direction
            var isLoadingUnitInExternalPosition = this.machineResourcesProvider.IsDrawerInBayExternalPosition(bayNumber);
            var isLoadingUnitInInternalPosition = this.machineResourcesProvider.IsDrawerInBayInternalPosition(bayNumber);

            switch (direction)
            {
                case ExternalBayMovementDirection.TowardMachine:
                    if (isLoadingUnitInInternalPosition)
                    {
                        return new ActionPolicy { Reason = Resources.Bays.ResourceManager.GetString("TheBayContainsAtLeastOneLoadingUnit", CommonUtils.Culture.Actual) };
                    }

                    break;

                case ExternalBayMovementDirection.TowardOperator:
#if CHECK_BAY_SENSOR
                    if (isLoadingUnitInExternalPosition)
                    {
                        return new ActionPolicy { Reason = Resources.Bays.ResourceManager.GetString("TheBayContainsALoadingUnitInItsExternalPosition", CommonUtils.Culture.Actual) };
                    }
#endif
                    break;

                default:
                    throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(ExternalBayMovementDirection));
            }

            // Not admitted the movement toward machine and the presence of zero sensor (mechanical constraints)
            if (this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber) &&
                movementCategory != MovementCategory.Manual &&
                direction == ExternalBayMovementDirection.TowardMachine)
            {
                return new ActionPolicy { Reason = Resources.Bays.ResourceManager.GetString("TheExtBayCannotPerformAnInvalidMovement", CommonUtils.Culture.Actual) };
            }

            // Not admitted the automatic movement if bay does not contain a loading unit
            if ((movementCategory == MovementCategory.Automatic) &&
                bay.Positions.FirstOrDefault().LoadingUnit != null &&
                !isLoadingUnitInExternalPosition &&
                !isLoadingUnitInInternalPosition)
            {
                return new ActionPolicy { Reason = Resources.Bays.ResourceManager.GetString("TheExtBayCannotPerformAnInvalidMovement", CommonUtils.Culture.Actual) };
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

            // Target position is positive with TowardOperator movement direction, otherwise is negative
            var targetPosition = (direction == ExternalBayMovementDirection.TowardOperator) ? bay.External.Race : -bay.External.Race;

            var speed = new[] { bay.FullLoadMovement.Speed };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,
                MovementMode.ExtBayChain,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction is ExternalBayMovementDirection.TowardOperator ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards);

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
                $"Direction: {direction}; " +
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

            var race = bay.External.Race;

            var distance = race;

            // Be careful: the distance value is used in a relative position mode
            switch (direction)
            {
                case ExternalBayMovementDirection.TowardOperator:
                    distance = race - Math.Abs(this.baysDataProvider.GetChainPosition(bayNumber)) - bay.ChainOffset;
                    break;

                case ExternalBayMovementDirection.TowardMachine:
                    distance = bay.ChainOffset - Math.Abs(this.baysDataProvider.GetChainPosition(bayNumber));
                    break;
            }

            //var targetPosition = (direction == ExternalBayMovementDirection.TowardOperator) ? race + bay.ChainOffset : bay.ChainOffset; // for .Absolute
            var targetPosition = distance;

            var procedureParameters = this.baysDataProvider.GetAssistedMovementsExternalBay(bayNumber);

            var speed = new[] { bay.FullLoadMovement.Speed * procedureParameters.FeedRate };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,  //.Absolute,
                MovementMode.ExtBayChain,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction is ExternalBayMovementDirection.TowardOperator ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards);

            this.logger.LogDebug(
                $"Move External Bay Assisted " +
                $"bayNumber: {bayNumber}; " +
                $"direction: {direction}; " +
                $"targetPosition: {targetPosition}; " +
                $"current bay axis position: {this.baysDataProvider.GetChainPosition(bayNumber)}; " +
                $"chain offset parameter: {bay.ChainOffset}; " +
                $"Direction: {direction}; " +
                $"feedrate: {procedureParameters.FeedRate}; " +
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

            this.machineVolatileDataProvider.IsBayHomingExecuted[bayNumber] = false;

            var bay = this.baysDataProvider.GetByNumber(bayNumber);
            var targetPosition = bay.External.Race;    // USE for .Relative
            // Check the module of distance to be moved
            const double EXT_RACE_FOR_EXTRACTION = 250.0d;
            if (distance > 0 && distance < bay.External.Race + Math.Abs(bay.ChainOffset) + EXT_RACE_FOR_EXTRACTION)
            {
                targetPosition = distance;
            }

            // Target position is positive with TowardOperator movement direction, otherwise position is negative
            targetPosition *= direction is ExternalBayMovementDirection.TowardOperator ? 1 : -1;

            // Use this for .Absolute
            //const double EXT_RACE_FOR_EXTRACTION = 250.0d;
            //const double INTERNAL_LIMIT = -150.0d;
            //var targetPosition = (direction is ExternalBayMovementDirection.TowardOperator) ?
            //    bay.External.Race + EXT_RACE_FOR_EXTRACTION :
            //    INTERNAL_LIMIT;

            var procedureParameters = this.baysDataProvider.GetManualMovementsExternalBay(bayNumber);

            var speed = new[] { bay.FullLoadMovement.Speed * procedureParameters.FeedRate };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,    //.Absolute,
                MovementMode.ExtBayChainManual,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction is ExternalBayMovementDirection.TowardOperator ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards);

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
                $"bypass: {bypassConditions}; " +
                $"targetPosition: {targetPosition:0.00}; " +
                $"Direction: {direction}; " +
                $"feedrate: {procedureParameters.FeedRate}; " +
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

        public void MovementForExtraction(int? loadUnitId, BayNumber bayNumber, MessageActor sender)
        {
            // Manual movement

            var policy = this.CanMove(ExternalBayMovementDirection.TowardOperator, bayNumber, MovementCategory.Manual);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            var race = bay.External.Race;
            var extraRace = bay.External.ExtraRace;

            // Target position is positive with TowardOperator movement direction
            var targetPosition = extraRace;

            // Check the module of distance to be moved
            const double MAX_RACE_FOR_EXTRACTION = 35.0d;
            if (Math.Abs(this.baysDataProvider.GetChainPosition(bayNumber)) + extraRace > race + MAX_RACE_FOR_EXTRACTION)
            {
                targetPosition = (race + MAX_RACE_FOR_EXTRACTION) - Math.Abs(this.baysDataProvider.GetChainPosition(bayNumber));
            }

            // Use this for .Absolute
            //const double MAX_RACE_FOR_EXTRACTION = 35.0d;
            //var targetPosition = Math.Abs(this.baysDataProvider.GetChainPosition(bayNumber)) + extraRace;

            var procedureParameters = this.baysDataProvider.GetManualMovementsExternalBay(bayNumber);

            var speed = new[] { bay.FullLoadMovement.Speed * procedureParameters.FeedRate };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,    //.Absolute,
                MovementMode.ExtBayChainManual,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                HorizontalMovementDirection.Backwards);

            this.logger.LogDebug(
                $"Move External Bay For Extraction " +
                $"bayNumber: {bayNumber}; " +
                $"LoadUnitId: {loadUnitId}; " +
                $"targetPosition: {targetPosition}; " +
                $"Direction: {HorizontalMovementDirection.Backwards}; " +
                $"feedrate: {procedureParameters.FeedRate}; " +
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

        public void MovementForInsertion(BayNumber bayNumber, MessageActor sender)
        {
            // Manual movement

            if (this.machineResourcesProvider.IsSensorZeroOnBay(bayNumber))
            {
                throw new InvalidOperationException(Resources.Bays.ResourceManager.GetString("TheExtBayCannotPerformAnInvalidMovement", CommonUtils.Culture.Actual));
            }

            var policy = this.CanMove(ExternalBayMovementDirection.TowardMachine, bayNumber, MovementCategory.Manual);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            var race = bay.External.Race;
            var extraRace = bay.External.ExtraRace;

            var targetPosition = extraRace;

            // Check the module of distance to be moved (to be checked: is it really necessary?)
            const double MAX_RACE_FOR_INSERTION = 35.0d;
            if (Math.Abs(this.baysDataProvider.GetChainPosition(bayNumber)) - extraRace < MAX_RACE_FOR_INSERTION)
            {
                targetPosition = (MAX_RACE_FOR_INSERTION + extraRace) - Math.Abs(this.baysDataProvider.GetChainPosition(bayNumber));
            }

            // Target position is negative with TowardMachine movement direction
            targetPosition *= -1;

            // Use this for .Absolute
            //const double MAX_RACE_FOR_EXTRACTION = 35.0d;
            //var targetPosition = Math.Abs(this.baysDataProvider.GetChainPosition(bayNumber)) - extraRace;

            var procedureParameters = this.baysDataProvider.GetManualMovementsExternalBay(bayNumber);

            var speed = new[] { bay.FullLoadMovement.Speed * procedureParameters.FeedRate };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,    //.Absolute,
                MovementMode.ExtBayChainManual,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.logger.LogDebug(
                $"Move External Bay For Insertion " +
                $"bayNumber: {bayNumber}; " +
                $"targetPosition: {targetPosition}; " +
                $"Direction: {HorizontalMovementDirection.Backwards}; " +
                $"feedrate: {procedureParameters.FeedRate}; " +
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

        public void StartTest(BayNumber bayNumber, MessageActor sender)
        {
            var direction = this.IsInternalPositionOccupied(bayNumber) ?
                ExternalBayMovementDirection.TowardOperator :
                ExternalBayMovementDirection.TowardMachine;

            var policy = this.CanMove(direction, bayNumber, MovementCategory.Assisted);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var bay = this.baysDataProvider.GetByNumber(bayNumber);
            var procedureParameters = this.setupProceduresDataProvider.GetBayExternalCalibration(bayNumber);

            var distance = bay.External.Race;
            switch (direction)
            {
                case ExternalBayMovementDirection.TowardOperator:
                    distance = bay.External.Race - Math.Abs(this.baysDataProvider.GetChainPosition(bayNumber)) - bay.ChainOffset;
                    break;

                case ExternalBayMovementDirection.TowardMachine:
                    distance = 0 - Math.Abs(this.baysDataProvider.GetChainPosition(bayNumber)) + bay.ChainOffset;
                    break;
            }

            //var targetPosition = (direction == ExternalBayMovementDirection.TowardOperator) ? race + bay.ChainOffset : bay.ChainOffset; // for .Absolute
            var targetPosition = distance;

            var speed = new[] { bay.FullLoadMovement.Speed * procedureParameters.FeedRate };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,   // .Absolute
                MovementMode.ExtBayTest,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                procedureParameters.RequiredCycles,
                lowerBound: 0,
                upperBound: 0,
                delay: 0,
                switchPosition,
                direction is ExternalBayMovementDirection.TowardOperator ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards);

            this.logger.LogDebug(
                $"Start External Bay Calibration Test " +
                $"bayNumber: {bayNumber}; " +
                $"targetPosition: {targetPosition}; " +
                $"feedrate: {procedureParameters.FeedRate}; " +
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
            this.PublishCommand(
                null,
                $"Stop external bay {bayNumber} calibration",
                MessageActor.DeviceManager,
                sender,
                MessageType.StopTest,
                bayNumber,
                BayNumber.None);
        }

        #endregion
    }
}
