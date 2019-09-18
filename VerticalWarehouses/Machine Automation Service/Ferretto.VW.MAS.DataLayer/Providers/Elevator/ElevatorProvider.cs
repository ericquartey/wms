using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

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

        private readonly IOffsetCalibrationDataLayer offsetCalibrationDataLayer;

        private readonly IPanelControlDataLayer panelControlDataLayer;

        private readonly IResolutionCalibrationDataLayer resolutionCalibrationDataLayer;

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
            IHorizontalAxisDataLayer horizontalAxisDataLayer,
            IResolutionCalibrationDataLayer resolutionCalibrationDataLayer,
            IOffsetCalibrationDataLayer offsetCalibrationDataLayer,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            IBayPositionControlDataLayer bayPositionControl,
            ICellControlDataLayer cellControlDataLayer,
            ISetupStatusProvider setupStatusProvider,
            IWeightControlDataLayer weightControl,
            IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer)
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

            if (verticalManualMovementsDataLayer is null)
            {
                throw new ArgumentNullException(nameof(verticalManualMovementsDataLayer));
            }

            this.dataContext = dataContext;
            this.panelControlDataLayer = panelControlDataLayer;
            this.horizontalManualMovementsDataLayer = horizontalManualMovementsDataLayer;
            this.horizontalAxisDataLayer = horizontalAxisDataLayer;
            this.resolutionCalibrationDataLayer = resolutionCalibrationDataLayer;
            this.offsetCalibrationDataLayer = offsetCalibrationDataLayer;
            this.verticalAxisDataLayer = verticalAxisDataLayer;
            this.bayPositionControl = bayPositionControl;
            this.cellControlDataLayer = cellControlDataLayer;
            this.setupStatusProvider = setupStatusProvider;
            this.weightControl = weightControl;
            this.verticalManualMovementsDataLayer = verticalManualMovementsDataLayer;
        }

        #endregion

        #region Methods

        public decimal GetHorizontalPosition(BayNumber bayNumber)
        {
            var messageData = new RequestPositionMessageData(Axis.Horizontal, 0);

            void publishAction() => this.PublishCommand(
                messageData,
                "Request Horizontal position",
                MessageActor.FiniteStateMachines,
                MessageType.RequestPosition,
                bayNumber,
                BayNumber.ElevatorBay);

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);

            return notifyData.CurrentPosition;
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

            return notifyData.CurrentPosition;
        }

        public void MoveHorizontal(HorizontalMovementDirection direction, BayNumber bayNumber)
        {
            var setupStatus = this.setupStatusProvider.Get();

            var targetPosition = setupStatus.VerticalOriginCalibration.IsCompleted
                ? this.horizontalManualMovementsDataLayer.RecoveryTargetPositionHM
                : this.horizontalManualMovementsDataLayer.InitialTargetPositionHM;

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var speed = this.horizontalAxisDataLayer.MaxEmptySpeedHA * this.horizontalManualMovementsDataLayer.FeedRateHM;

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.Position,
                targetPosition,
                speed,
                this.horizontalAxisDataLayer.MaxEmptyAccelerationHA,
                this.horizontalAxisDataLayer.MaxEmptyDecelerationHA,
                0,
                0,
                0,
                0);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                bayNumber,
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

            var speed = this.verticalAxisDataLayer.MaxEmptySpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                MovementMode.Position,
                targetPosition,
                speed,
                this.verticalAxisDataLayer.MaxEmptyAcceleration,
                this.verticalAxisDataLayer.MaxEmptyDeceleration,
                0,
                0,
                0,
                0);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
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

            var speed = this.verticalAxisDataLayer.MaxEmptySpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                movementType,
                MovementMode.Position,
                targetPosition,
                speed,
                this.verticalAxisDataLayer.MaxEmptyAcceleration,
                this.verticalAxisDataLayer.MaxEmptyDeceleration,
                0,
                0,
                0,
                0);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
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

            var speed = this.verticalAxisDataLayer.MaxEmptySpeed * this.verticalManualMovementsDataLayer.FeedRateAfterZero;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                MovementMode.Position,
                distance,
                speed,
                this.verticalAxisDataLayer.MaxEmptyAcceleration,
                this.verticalAxisDataLayer.MaxEmptyDeceleration,
                0,
                0,
                0,
                0);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void RunInMotionCurrentSampling(decimal displacement, decimal netWeight)
        {
            throw new NotImplementedException();
        }

        public void RunInPlaceCurrentSampling(TimeSpan inPlaceSamplingDuration, decimal netWeight)
        {
            throw new NotImplementedException();
        }

        public void Stop(BayNumber bayNumber)
        {
            this.PublishCommand(
                null,
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
