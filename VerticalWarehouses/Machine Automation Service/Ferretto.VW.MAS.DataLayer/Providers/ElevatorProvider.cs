using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class ElevatorProvider : BaseProvider, IElevatorProvider
    {
        #region Fields

        private readonly IBayPositionControlDataLayer bayPositionControl;

        private readonly ICellControlDataLayer cellControlDataLayer;

        private readonly DataLayerContext dataContext;

        private readonly IHorizontalAxisDataLayer horizontalAxisDataLayer;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer;

        private readonly IHorizontalMovementLongerDepositDataLayer horizontalMovementLongerDepositDataLayer;

        private readonly IHorizontalMovementLongerPickupDataLayer horizontalMovementLongerPickupDataLayer;

        private readonly IHorizontalMovementShorterDepositDataLayer horizontalMovementShorterDepositDataLayer;

        private readonly IHorizontalMovementShorterPickupDataLayer horizontalMovementShorterPickupDataLayer;

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly IMachineConfigurationProvider machineConfigurationProvider;

        private readonly IOffsetCalibrationDataLayer offsetCalibrationDataLayer;

        private readonly IPanelControlDataLayer panelControlDataLayer;

        private readonly IResolutionCalibrationDataLayer resolutionCalibrationDataLayer;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalAxisDataLayer verticalAxisDataLayer;

        private readonly IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer;

        private readonly IWeightControlDataLayer weightControl;

        #endregion

        #region Constructors

        public ElevatorProvider(
            DataLayerContext dataContext,
            IEventAggregator eventAggregator,
            IPanelControlDataLayer panelControlDataLayer,
            IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer,
            IHorizontalMovementShorterDepositDataLayer horizontalMovementShorterDepositDataLayer,
            IHorizontalMovementLongerDepositDataLayer horizontalMovementLongerDepositDataLayer,
            IHorizontalMovementShorterPickupDataLayer horizontalMovementShorterPickupDataLayer,
            IHorizontalMovementLongerPickupDataLayer horizontalMovementLongerPickupDataLayer,
            IHorizontalAxisDataLayer horizontalAxisDataLayer,
            IResolutionCalibrationDataLayer resolutionCalibrationDataLayer,
            IOffsetCalibrationDataLayer offsetCalibrationDataLayer,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            IBayPositionControlDataLayer bayPositionControl,
            ICellControlDataLayer cellControlDataLayer,
            ISetupStatusProvider setupStatusProvider,
            IWeightControlDataLayer weightControl,
            IMachineConfigurationProvider machineConfigurationProvider,
            ISensorsProvider sensorsProvider,
            IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer,
            ILoadingUnitsProvider loadingUnitsProvider)
            : base(eventAggregator)
        {
            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (panelControlDataLayer is null)
            {
                throw new ArgumentNullException(nameof(panelControlDataLayer));
            }

            if (horizontalManualMovementsDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalManualMovementsDataLayer));
            }

            if (horizontalMovementShorterPickupDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalMovementShorterPickupDataLayer));
            }

            if (horizontalMovementLongerPickupDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalMovementLongerPickupDataLayer));
            }

            if (horizontalMovementShorterDepositDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalMovementShorterDepositDataLayer));
            }

            if (horizontalMovementLongerDepositDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalMovementLongerDepositDataLayer));
            }

            if (horizontalAxisDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalAxisDataLayer));
            }

            if (resolutionCalibrationDataLayer is null)
            {
                throw new ArgumentNullException(nameof(resolutionCalibrationDataLayer));
            }

            if (offsetCalibrationDataLayer is null)
            {
                throw new ArgumentNullException(nameof(offsetCalibrationDataLayer));
            }

            if (verticalAxisDataLayer is null)
            {
                throw new ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            if (bayPositionControl is null)
            {
                throw new ArgumentNullException(nameof(bayPositionControl));
            }

            if (cellControlDataLayer is null)
            {
                throw new ArgumentNullException(nameof(cellControlDataLayer));
            }

            if (setupStatusProvider is null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            if (weightControl is null)
            {
                throw new ArgumentNullException(nameof(weightControl));
            }

            if (machineConfigurationProvider is null)
            {
                throw new ArgumentNullException(nameof(machineConfigurationProvider));
            }

            if (sensorsProvider is null)
            {
                throw new ArgumentNullException(nameof(sensorsProvider));
            }

            if (verticalManualMovementsDataLayer is null)
            {
                throw new ArgumentNullException(nameof(verticalManualMovementsDataLayer));
            }

            if (loadingUnitsProvider is null)
            {
                throw new ArgumentNullException(nameof(loadingUnitsProvider));
            }

            this.dataContext = dataContext;
            this.panelControlDataLayer = panelControlDataLayer;
            this.horizontalManualMovementsDataLayer = horizontalManualMovementsDataLayer;
            this.horizontalMovementShorterPickupDataLayer = horizontalMovementShorterPickupDataLayer;
            this.horizontalMovementLongerPickupDataLayer = horizontalMovementLongerPickupDataLayer;
            this.horizontalMovementShorterDepositDataLayer = horizontalMovementShorterDepositDataLayer;
            this.horizontalMovementLongerDepositDataLayer = horizontalMovementLongerDepositDataLayer;
            this.horizontalAxisDataLayer = horizontalAxisDataLayer;
            this.resolutionCalibrationDataLayer = resolutionCalibrationDataLayer;
            this.offsetCalibrationDataLayer = offsetCalibrationDataLayer;
            this.verticalAxisDataLayer = verticalAxisDataLayer;
            this.bayPositionControl = bayPositionControl;
            this.cellControlDataLayer = cellControlDataLayer;
            this.setupStatusProvider = setupStatusProvider;
            this.weightControl = weightControl;
            this.machineConfigurationProvider = machineConfigurationProvider;
            this.sensorsProvider = sensorsProvider;
            this.verticalManualMovementsDataLayer = verticalManualMovementsDataLayer;
            this.loadingUnitsProvider = loadingUnitsProvider;
        }

        #endregion

        #region Methods

        public decimal? GetHorizontalPosition(BayNumber requestingBay)
        {
            var messageData = new RequestPositionMessageData(Axis.Horizontal, 0);

            void publishAction() => this.PublishCommand(
                messageData,
                "Request Horizontal position",
                MessageActor.FiniteStateMachines,
                MessageType.RequestPosition,
                requestingBay,
                BayNumber.ElevatorBay);

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);

            return notifyData.CurrentPosition ?? 0;
        }

        public decimal GetVerticalPosition(BayNumber bayNumber)
        {
            var messageData = new RequestPositionMessageData(Axis.Vertical, 0);

            void publishAction() => this.PublishCommand(
                messageData,
                "Request vertical position",
                MessageActor.FiniteStateMachines,
                MessageType.RequestPosition,
                bayNumber,
                BayNumber.ElevatorBay);

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);

            return notifyData.CurrentPosition ?? 0;
        }

        public void MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard, BayNumber requestingBay)
        {
            var sensors = this.sensorsProvider.GetAll(requestingBay);

            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSideBay1]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSideBay1];

            if (isStartedOnBoard != isLoadingUnitOnBoard)
            {
                throw new InvalidOperationException(
                    "Invalid " + (isStartedOnBoard ? "Deposit" : "Pickup") + " command for " + (isStartedOnBoard ? "empty" : "full") + " elevator");
            }

            var zeroSensor = this.machineConfigurationProvider.IsOneKMachine()
                ? IOMachineSensors.ZeroPawlSensorOneK
                : IOMachineSensors.ZeroPawlSensor;

            if ((!isStartedOnBoard && !sensors[(int)zeroSensor]) || (isStartedOnBoard && sensors[(int)zeroSensor]))
            {
                throw new InvalidOperationException("Invalid Zero Chain position");
            }

            // execute command
            var position = this.GetHorizontalPosition(requestingBay).Value;

            // if direction is Forwards height increments, else is decremented

            // the total length is splitted in two unequal distances
            var isLongerDistance =
                (isStartedOnBoard && direction == HorizontalMovementDirection.Forwards)
                ||
                (!isStartedOnBoard && direction == HorizontalMovementDirection.Backwards);

            decimal[] speed = new decimal[5];
            decimal[] acceleration = new decimal[5];
            decimal[] deceleration = new decimal[5];
            decimal[] switchPosition = new decimal[5];

            var directionMultiplier = direction == HorizontalMovementDirection.Forwards ? 1 : -1;
            if (isLongerDistance && isStartedOnBoard)
            {
                speed[0] = this.horizontalMovementLongerDepositDataLayer.P0SpeedV1LongerDeposit;
                speed[1] = this.horizontalMovementLongerDepositDataLayer.P1SpeedV2LongerDeposit;
                speed[2] = this.horizontalMovementLongerDepositDataLayer.P2SpeedV3LongerDeposit;
                speed[3] = this.horizontalMovementLongerDepositDataLayer.P3SpeedV4LongerDeposit;
                speed[4] = this.horizontalMovementLongerDepositDataLayer.P4SpeedV5LongerDeposit;

                acceleration[0] = this.horizontalMovementLongerDepositDataLayer.P0AccelerationLongerDeposit;
                acceleration[1] = this.horizontalMovementLongerDepositDataLayer.P1AccelerationLongerDeposit;
                acceleration[2] = this.horizontalMovementLongerDepositDataLayer.P2AccelerationLongerDeposit;
                acceleration[3] = this.horizontalMovementLongerDepositDataLayer.P3AccelerationLongerDeposit;
                acceleration[4] = this.horizontalMovementLongerDepositDataLayer.P4AccelerationLongerDeposit;

                deceleration[0] = this.horizontalMovementLongerDepositDataLayer.P0DecelerationLongerDeposit;
                deceleration[1] = this.horizontalMovementLongerDepositDataLayer.P1DecelerationLongerDeposit;
                deceleration[2] = this.horizontalMovementLongerDepositDataLayer.P2DecelerationLongerDeposit;
                deceleration[3] = this.horizontalMovementLongerDepositDataLayer.P3DecelerationLongerDeposit;
                deceleration[4] = this.horizontalMovementLongerDepositDataLayer.P4DecelerationLongerDeposit;

                switchPosition[0] = position + this.horizontalMovementLongerDepositDataLayer.P1QuoteLongerDeposit * directionMultiplier;
                switchPosition[1] = position + this.horizontalMovementLongerDepositDataLayer.P2QuoteLongerDeposit * directionMultiplier;
                switchPosition[2] = position + this.horizontalMovementLongerDepositDataLayer.P3QuoteLongerDeposit * directionMultiplier;
                switchPosition[3] = position + this.horizontalMovementLongerDepositDataLayer.P4QuoteLongerDeposit * directionMultiplier;
                switchPosition[4] = position + this.horizontalMovementLongerDepositDataLayer.P5QuoteLongerDeposit * directionMultiplier;
            }
            else if (isLongerDistance && !isStartedOnBoard)
            {
                speed[0] = this.horizontalMovementLongerPickupDataLayer.P0SpeedV1LongerPickup;
                speed[1] = this.horizontalMovementLongerPickupDataLayer.P1SpeedV2LongerPickup;
                speed[2] = this.horizontalMovementLongerPickupDataLayer.P2SpeedV3LongerPickup;
                speed[3] = this.horizontalMovementLongerPickupDataLayer.P3SpeedV4LongerPickup;
                speed[4] = this.horizontalMovementLongerPickupDataLayer.P4SpeedV5LongerPickup;

                acceleration[0] = this.horizontalMovementLongerPickupDataLayer.P0AccelerationLongerPickup;
                acceleration[1] = this.horizontalMovementLongerPickupDataLayer.P1AccelerationLongerPickup;
                acceleration[2] = this.horizontalMovementLongerPickupDataLayer.P2AccelerationLongerPickup;
                acceleration[3] = this.horizontalMovementLongerPickupDataLayer.P3AccelerationLongerPickup;
                acceleration[4] = this.horizontalMovementLongerPickupDataLayer.P4AccelerationLongerPickup;

                deceleration[0] = this.horizontalMovementLongerPickupDataLayer.P0DecelerationLongerPickup;
                deceleration[1] = this.horizontalMovementLongerPickupDataLayer.P1DecelerationLongerPickup;
                deceleration[2] = this.horizontalMovementLongerPickupDataLayer.P2DecelerationLongerPickup;
                deceleration[3] = this.horizontalMovementLongerPickupDataLayer.P3DecelerationLongerPickup;
                deceleration[4] = this.horizontalMovementLongerPickupDataLayer.P4DecelerationLongerPickup;

                switchPosition[0] = position + this.horizontalMovementLongerPickupDataLayer.P1QuoteLongerPickup * directionMultiplier;
                switchPosition[1] = position + this.horizontalMovementLongerPickupDataLayer.P2QuoteLongerPickup * directionMultiplier;
                switchPosition[2] = position + this.horizontalMovementLongerPickupDataLayer.P3QuoteLongerPickup * directionMultiplier;
                switchPosition[3] = position + this.horizontalMovementLongerPickupDataLayer.P4QuoteLongerPickup * directionMultiplier;
                switchPosition[4] = position + this.horizontalMovementLongerPickupDataLayer.P5QuoteLongerPickup * directionMultiplier;
            }
            else if (!isLongerDistance && isStartedOnBoard)
            {
                speed[0] = this.horizontalMovementShorterDepositDataLayer.P0SpeedV1ShorterDeposit;
                speed[1] = this.horizontalMovementShorterDepositDataLayer.P1SpeedV2ShorterDeposit;
                speed[2] = this.horizontalMovementShorterDepositDataLayer.P2SpeedV3ShorterDeposit;
                speed[3] = this.horizontalMovementShorterDepositDataLayer.P3SpeedV4ShorterDeposit;
                speed[4] = this.horizontalMovementShorterDepositDataLayer.P4SpeedV5ShorterDeposit;

                acceleration[0] = this.horizontalMovementShorterDepositDataLayer.P0AccelerationShorterDeposit;
                acceleration[1] = this.horizontalMovementShorterDepositDataLayer.P1AccelerationShorterDeposit;
                acceleration[2] = this.horizontalMovementShorterDepositDataLayer.P2AccelerationShorterDeposit;
                acceleration[3] = this.horizontalMovementShorterDepositDataLayer.P3AccelerationShorterDeposit;
                acceleration[4] = this.horizontalMovementShorterDepositDataLayer.P4AccelerationShorterDeposit;

                deceleration[0] = this.horizontalMovementShorterDepositDataLayer.P0DecelerationShorterDeposit;
                deceleration[1] = this.horizontalMovementShorterDepositDataLayer.P1DecelerationShorterDeposit;
                deceleration[2] = this.horizontalMovementShorterDepositDataLayer.P2DecelerationShorterDeposit;
                deceleration[3] = this.horizontalMovementShorterDepositDataLayer.P3DecelerationShorterDeposit;
                deceleration[4] = this.horizontalMovementShorterDepositDataLayer.P4DecelerationShorterDeposit;

                switchPosition[0] = position + this.horizontalMovementShorterDepositDataLayer.P1QuoteShorterDeposit * directionMultiplier;
                switchPosition[1] = position + this.horizontalMovementShorterDepositDataLayer.P2QuoteShorterDeposit * directionMultiplier;
                switchPosition[2] = position + this.horizontalMovementShorterDepositDataLayer.P3QuoteShorterDeposit * directionMultiplier;
                switchPosition[3] = position + this.horizontalMovementShorterDepositDataLayer.P4QuoteShorterDeposit * directionMultiplier;
                switchPosition[4] = position + this.horizontalMovementShorterDepositDataLayer.P5QuoteShorterDeposit * directionMultiplier;
            }
            else
            {
                speed[0] = this.horizontalMovementShorterPickupDataLayer.P0SpeedV1ShorterPickup;
                speed[1] = this.horizontalMovementShorterPickupDataLayer.P1SpeedV2ShorterPickup;
                speed[2] = this.horizontalMovementShorterPickupDataLayer.P2SpeedV3ShorterPickup;
                speed[3] = this.horizontalMovementShorterPickupDataLayer.P3SpeedV4ShorterPickup;
                speed[4] = this.horizontalMovementShorterPickupDataLayer.P4SpeedV5ShorterPickup;

                acceleration[0] = this.horizontalMovementShorterPickupDataLayer.P0AccelerationShorterPickup;
                acceleration[1] = this.horizontalMovementShorterPickupDataLayer.P1AccelerationShorterPickup;
                acceleration[2] = this.horizontalMovementShorterPickupDataLayer.P2AccelerationShorterPickup;
                acceleration[3] = this.horizontalMovementShorterPickupDataLayer.P3AccelerationShorterPickup;
                acceleration[4] = this.horizontalMovementShorterPickupDataLayer.P4AccelerationShorterPickup;

                deceleration[0] = this.horizontalMovementShorterPickupDataLayer.P0DecelerationShorterPickup;
                deceleration[1] = this.horizontalMovementShorterPickupDataLayer.P1DecelerationShorterPickup;
                deceleration[2] = this.horizontalMovementShorterPickupDataLayer.P2DecelerationShorterPickup;
                deceleration[3] = this.horizontalMovementShorterPickupDataLayer.P3DecelerationShorterPickup;
                deceleration[4] = this.horizontalMovementShorterPickupDataLayer.P4DecelerationShorterPickup;

                switchPosition[0] = position + this.horizontalMovementShorterPickupDataLayer.P1QuoteShorterPickup * directionMultiplier;
                switchPosition[1] = position + this.horizontalMovementShorterPickupDataLayer.P2QuoteShorterPickup * directionMultiplier;
                switchPosition[2] = position + this.horizontalMovementShorterPickupDataLayer.P3QuoteShorterPickup * directionMultiplier;
                switchPosition[3] = position + this.horizontalMovementShorterPickupDataLayer.P4QuoteShorterPickup * directionMultiplier;
                switchPosition[4] = position + this.horizontalMovementShorterPickupDataLayer.P5QuoteShorterPickup * directionMultiplier;
            }

            var targetPosition = switchPosition.Last();

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.TableTarget,
                MovementMode.Position,
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
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void MoveHorizontalManual(HorizontalMovementDirection direction, BayNumber requestingBay)
        {
            var setupStatus = this.setupStatusProvider.Get();

            var targetPosition = setupStatus.VerticalOriginCalibration.IsCompleted
                ? this.horizontalManualMovementsDataLayer.RecoveryTargetPositionHM
                : this.horizontalManualMovementsDataLayer.InitialTargetPositionHM;

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            decimal[] speed = { this.horizontalAxisDataLayer.MaxEmptySpeedHA * this.horizontalManualMovementsDataLayer.FeedRateHM / 10 };
            decimal[] acceleration = { this.horizontalAxisDataLayer.MaxEmptyAccelerationHA };
            decimal[] deceleration = { this.horizontalAxisDataLayer.MaxEmptyDecelerationHA };
            decimal[] switchPosition = { 0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.Position,
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
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void MoveToVerticalPosition(decimal targetPosition, FeedRateCategory feedRateCategory, BayNumber bayNumber)
        {
            var lowerBound = Math.Max(this.verticalAxisDataLayer.LowerBound, this.verticalAxisDataLayer.Offset);
            var upperBound = this.verticalAxisDataLayer.UpperBound;

            if (targetPosition < lowerBound || targetPosition > upperBound)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetPosition),
                    string.Format(Resources.Elevator.TargetPositionMustBeInRange, targetPosition, lowerBound, upperBound));
            }

            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (!homingDone)
            {
                throw new InvalidOperationException(
                   Resources.Elevator.VerticalOriginCalibrationMustBePerformed);
            }

            var feedRate = this.GetFeedRate(feedRateCategory);

            decimal[] speed = { this.verticalAxisDataLayer.MaxEmptySpeed * feedRate };
            decimal[] acceleration = { this.verticalAxisDataLayer.MaxEmptyAcceleration };
            decimal[] deceleration = { this.verticalAxisDataLayer.MaxEmptyDeceleration };
            decimal[] switchPosition = { 0 };

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                MovementMode.Position,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void MoveVertical(VerticalMovementDirection direction, BayNumber bayNumber)
        {
            var movementType = MovementType.Relative;

            decimal feedRate;
            decimal targetPosition;

            //INFO Absolute movement using the min and max reachable positions for limits
            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (homingDone)
            {
                feedRate = this.verticalManualMovementsDataLayer.FeedRateAfterZero;
                movementType = MovementType.Absolute;

                targetPosition = direction == VerticalMovementDirection.Up
                    ? this.verticalAxisDataLayer.UpperBound
                    : this.verticalAxisDataLayer.LowerBound;
            }
            else //INFO Before homing relative movements step by step
            {
                feedRate = this.verticalManualMovementsDataLayer.FeedRateVM;

                targetPosition = direction == VerticalMovementDirection.Up
                    ? this.verticalManualMovementsDataLayer.PositiveTargetDirection
                    : -this.verticalManualMovementsDataLayer.NegativeTargetDirection;
            }

            decimal[] speed = { this.verticalAxisDataLayer.MaxEmptySpeed * feedRate };
            decimal[] acceleration = { this.verticalAxisDataLayer.MaxEmptyAcceleration };
            decimal[] deceleration = { this.verticalAxisDataLayer.MaxEmptyDeceleration };
            decimal[] switchPosition = { 0 };

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                movementType,
                MovementMode.Position,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void MoveVerticalOfDistance(decimal distance, BayNumber bayNumber)
        {
            if (distance == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(distance),
                    Resources.Elevator.MovementDistanceCannotBeZero);
            }

            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (!homingDone)
            {
                throw new InvalidOperationException(Resources.Elevator.VerticalOriginCalibrationMustBePerformed);
            }

            decimal[] speed = { this.verticalAxisDataLayer.MaxEmptySpeed * this.verticalManualMovementsDataLayer.FeedRateAfterZero };
            decimal[] acceleration = { this.verticalAxisDataLayer.MaxEmptyAcceleration };
            decimal[] deceleration = { this.verticalAxisDataLayer.MaxEmptyDeceleration };
            decimal[] switchPosition = { 0 };

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                MovementMode.Position,
                distance,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void RunInMotionCurrentSampling(decimal displacement, decimal netWeight, BayNumber requestingBay)
        {
            throw new NotImplementedException();
        }

        public void RunInPlaceCurrentSampling(TimeSpan inPlaceSamplingDuration, decimal netWeight, BayNumber requestingBay)
        {
            throw new NotImplementedException();
        }

        public void RunTorqueCurrentSampling(decimal displacement, decimal netWeight, int? loadingUnitId, BayNumber requestingBay)
        {
            if (displacement <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(displacement),
                    Resources.Elevator.MovementDistanceCannotBeZero);
            }

            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (!homingDone)
            {
                throw new InvalidOperationException(Resources.Elevator.VerticalOriginCalibrationMustBePerformed);
            }

            decimal[] speed = { this.verticalAxisDataLayer.MaxEmptySpeed * this.verticalManualMovementsDataLayer.FeedRateAfterZero };
            decimal[] acceleration = { this.verticalAxisDataLayer.MaxEmptyAcceleration };
            decimal[] deceleration = { this.verticalAxisDataLayer.MaxEmptyDeceleration };
            decimal[] switchPosition = { 0 };

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                MovementMode.TorqueCurrentSampling,
                displacement,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition,
                HorizontalMovementDirection.Forwards)
            {
                LoadedNetWeight = netWeight,
                LoadingUnitId = loadingUnitId
            };

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.TorqueCurrentSampling,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void Stop(BayNumber bayNumber)
        {
            var messageData = new StopMessageData(StopRequestReason.Stop);
            this.PublishCommand(
                messageData,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void UpdateResolution(decimal newResolution)
        {
            if (newResolution <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newResolution));
            }

            this.verticalAxisDataLayer.Resolution = newResolution;

            this.setupStatusProvider.CompleteVerticalResolution();
        }

        private decimal GetFeedRate(FeedRateCategory feedRateCategory)
        {
            decimal feedRate;
            switch (feedRateCategory)
            {
                case FeedRateCategory.VerticalManualMovements:
                    feedRate = this.verticalManualMovementsDataLayer.FeedRateVM;
                    break;

                case FeedRateCategory.VerticalManualMovementsAfterZero:
                    feedRate = this.verticalManualMovementsDataLayer.FeedRateAfterZero;
                    break;

                case FeedRateCategory.HorizontalManualMovements:
                    feedRate = this.horizontalManualMovementsDataLayer.FeedRateHM;
                    break;

                case FeedRateCategory.VerticalResolutionCalibration:
                    feedRate = this.resolutionCalibrationDataLayer.FeedRate;
                    break;

                case FeedRateCategory.VerticalOffsetCalibration:
                    feedRate = this.offsetCalibrationDataLayer.FeedRateOC;
                    break;

                case FeedRateCategory.CellHeightCheck:
                    feedRate = this.cellControlDataLayer.FeedRateCC;
                    break;

                case FeedRateCategory.PanelHeightCheck:
                    feedRate = this.panelControlDataLayer.FeedRatePC;
                    break;

                case FeedRateCategory.LoadingUnitWeight:
                    feedRate = this.weightControl.FeedRateWC;
                    break;

                case FeedRateCategory.BayHeight:
                    feedRate = this.bayPositionControl.FeedRateBP;
                    break;

                case FeedRateCategory.LoadFirstDrawer:
                    throw new NotImplementedException(nameof(FeedRateCategory.LoadFirstDrawer));

                case FeedRateCategory.ShutterManualMovements:
                    throw new NotImplementedException(nameof(FeedRateCategory.ShutterManualMovements));

                case FeedRateCategory.ShutterHeightCheck:
                    throw new NotImplementedException(nameof(FeedRateCategory.ShutterHeightCheck));

                default:
                    throw new ArgumentOutOfRangeException(nameof(feedRateCategory));
            }

            return feedRate;
        }

        #endregion
    }
}
