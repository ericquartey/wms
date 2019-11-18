using System;
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

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public CarouselProvider(
            IBaysProvider baysProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILoadingUnitsProvider loadingUnitsProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.loadingUnitsProvider = loadingUnitsProvider ?? throw new ArgumentNullException(nameof(loadingUnitsProvider));
        }

        #endregion

        #region Properties

        public double HorizontalPosition { get; set; }

        #endregion

        #region Methods

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

        public void Move(HorizontalMovementDirection direction, int? loadingUnitId, BayNumber bayNumber, MessageActor sender)
        {
            var bay = this.baysProvider.GetByNumber(bayNumber);
            if (bay.Carousel is null)
            {
                throw new InvalidOperationException($"Cannot operate carousel on bay {bayNumber} because it has no carousel.");
            }

            var targetPosition = bay.Carousel.ElevatorDistance;

            targetPosition *= (direction == HorizontalMovementDirection.Forwards) ? 1 : -1;

            double scalingFactor = 1;
            if (loadingUnitId.HasValue)
            {
                var loadUnit = this.loadingUnitsProvider.GetById(loadingUnitId.Value);
                if (loadUnit.MaxNetWeight + loadUnit.Tare > 0)
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
                direction);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
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

            targetPosition *= ((direction == HorizontalMovementDirection.Forwards) ? 1 : -1);

            var procedureParameters = this.setupProceduresDataProvider.GetHorizontalManualMovements();
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
                direction);

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
                "Stop Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Stop,
                bayNumber,
                BayNumber.None);
        }

        public void UpdateRealTimePosition(BayNumber bayNumber, double position)
        {
            var bay = this.baysProvider.GetByNumber(bayNumber);
            if (bay is null)
            {
                throw new ArgumentOutOfRangeException(nameof(bayNumber));
            }
            this.baysProvider.UpdateRealTimePosition(bay, position);
        }

        #endregion
    }
}
