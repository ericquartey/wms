using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class LoadingUnitMovementProvider : BaseProvider, ILoadingUnitMovementProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICarouselProvider carouselProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly IShutterProvider shutterProvider;

        #endregion

        #region Constructors

        public LoadingUnitMovementProvider(
            IBaysDataProvider baysDataProvider,
            ICarouselProvider carouselProvider,
            ICellsProvider cellsProvider,
            IElevatorProvider elevatorProvider,
            IElevatorDataProvider elevatorDataProvider,
            ISensorsProvider sensorsProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IShutterProvider shutterProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.carouselProvider = carouselProvider ?? throw new ArgumentNullException(nameof(carouselProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.elevatorProvider = elevatorProvider ?? throw new ArgumentNullException(nameof(elevatorProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.shutterProvider = shutterProvider ?? throw new ArgumentNullException(nameof(shutterProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Properties

        private bool IsHomingExecuted { get; set; }

        #endregion

        #region Methods

        public MessageStatus CarouselStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning
                || message.Type == MessageType.Homing
                )
            {
                return message.Status;
            }
            if (message.Status == MessageStatus.OperationError
                || message.Status == MessageStatus.OperationStop
                || message.Status == MessageStatus.OperationRunningStop
                )
            {
                return message.Status;
            }

            return MessageStatus.NotSpecified;
        }

        public MachineErrorCode CheckBaySensors(Bay bay, LoadingUnitLocation loadingUnitPosition, bool deposit)
        {
            var bayPosition = this.baysDataProvider.GetPositionByLocation(loadingUnitPosition);
            if (deposit)
            {
                return this.carouselProvider.CanElevatorDeposit(bayPosition);
            }
            else
            {
                return this.carouselProvider.CanElevatorPickup(bayPosition);
            }
        }

        public void CloseShutter(MessageActor sender, BayNumber requestingBay, bool restore)
        {
            try
            {
                this.shutterProvider.MoveTo(ShutterPosition.Closed, requestingBay, sender);
            }
            catch (InvalidOperationException ex)
            {
                if (restore)
                {
                    this.shutterProvider.Move(ShutterMovementDirection.Down, requestingBay, sender);
                }
                else
                {
                    throw;
                }
            }
        }

        public void ContinuePositioning(MessageActor sender, BayNumber requestingBay)
        {
            this.elevatorProvider.ContinuePositioning(requestingBay, sender);
        }

        public bool FilterNotifications(NotificationMessage notification, MessageActor destination)
        {
            return (notification.Destination == MessageActor.Any || notification.Destination == destination) &&
                (notification.Type == MessageType.Positioning ||
                 notification.Type == MessageType.Stop ||
                 notification.Type == MessageType.InverterStop ||
                 notification.Type == MessageType.ShutterPositioning ||
                 notification.Status == MessageStatus.OperationStop ||
                 notification.Status == MessageStatus.OperationError ||
                 notification.Status == MessageStatus.OperationFaultStop ||
                 notification.Status == MessageStatus.OperationRunningStop);
        }

        public double GetCurrentVerticalPosition()
        {
            return this.elevatorProvider.VerticalPosition;
        }

        public double? GetDestinationHeight(Mission moveData, out int? targetBayPositionId, out int? targetCellId)
        {
            double? targetPosition = null;
            targetBayPositionId = null;
            targetCellId = null;

            switch (moveData.LoadingUnitDestination)
            {
                case LoadingUnitLocation.LoadingUnit:
                    // Retrieve loading unit position
                    if (moveData.LoadingUnitId != 0)
                    {
                        var cell = this.cellsProvider.GetByLoadingUnitId(moveData.LoadingUnitId);
                        if (cell != null && cell.Status == CellStatus.Free)
                        {
                            targetPosition = cell.Position;
                            targetCellId = cell.Id;
                        }
                    }
                    break;

                case LoadingUnitLocation.Cell:
                    // Retrieve Cell height
                    if (moveData.DestinationCellId != null)
                    {
                        var cell = this.cellsProvider.GetById(moveData.DestinationCellId.Value);
                        if (cell != null && cell.Status == CellStatus.Free)
                        {
                            targetPosition = cell.Position;
                            targetCellId = cell.Id;
                        }
                    }
                    break;

                default:
                    targetPosition = this.baysDataProvider.GetLoadingUnitDestinationHeight(moveData.LoadingUnitDestination);
                    targetBayPositionId = this.baysDataProvider.GetPositionByLocation(moveData.LoadingUnitDestination).Id;
                    break;
            }
            return targetPosition;
        }

        public ShutterPosition GetShutterOpenPosition(Bay bay, LoadingUnitLocation location)
        {
            var openShutter = ShutterPosition.NotSpecified;
            if (bay.Shutter.Type != ShutterType.NotSpecified)
            {
                if (bay.Shutter.Type == ShutterType.TwoSensors)
                {
                    openShutter = ShutterPosition.Opened;
                }
                else
                {
                    var bayPosition = bay.Positions.FirstOrDefault(x => x.Location == location);
                    if (bayPosition is null)
                    {
                        // TODO: throw an exception?
                        openShutter = ShutterPosition.NotSpecified;
                    }
                    else if (bayPosition.IsUpper)
                    {
                        openShutter = ShutterPosition.Opened;
                    }
                    else
                    {
                        openShutter = ShutterPosition.Half;
                    }
                }
            }
            return openShutter;
        }

        public double? GetSourceHeight(Mission moveData, out int? targetBayPositionId, out int? targetCellId)
        {
            double? targetPosition = null;
            targetBayPositionId = null;
            targetCellId = null;

            switch (moveData.LoadingUnitSource)
            {
                case LoadingUnitLocation.LoadingUnit:
                    // Retrieve loading unit position
                    if (moveData.LoadingUnitId != 0)
                    {
                        var cell = this.cellsProvider.GetByLoadingUnitId(moveData.LoadingUnitId);
                        if (cell != null && cell.Status == CellStatus.Occupied)
                        {
                            targetPosition = cell.Position;
                            targetCellId = cell.Id;
                        }
                    }
                    break;

                case LoadingUnitLocation.Cell:
                    // Retrieve Cell height
                    if (moveData.LoadingUnitCellSourceId != null)
                    {
                        var cell = this.cellsProvider.GetById(moveData.LoadingUnitCellSourceId.Value);
                        if (cell != null && cell.Status == CellStatus.Occupied)
                        {
                            targetPosition = cell.Position;
                            targetCellId = cell.Id;
                        }
                    }
                    break;

                default:
                    targetPosition = this.baysDataProvider.GetLoadingUnitDestinationHeight(moveData.LoadingUnitSource);
                    targetBayPositionId = this.baysDataProvider.GetPositionByLocation(moveData.LoadingUnitSource).Id;
                    break;
            }
            return targetPosition;
        }

        public void Homing(Axis axis, Calibration calibration, int loadingUnitId, BayNumber requestingBay, MessageActor sender)
        {
            if (axis == Axis.BayChain)
            {
                this.carouselProvider.Homing(calibration, loadingUnitId, requestingBay, sender);
            }
            else
            {
                this.elevatorProvider.Homing(axis, calibration, loadingUnitId, requestingBay, sender);
            }
        }

        public bool IsOnlyBottomPositionOccupied(BayNumber bayNumber)
        {
            return this.carouselProvider.IsOnlyBottomPositionOccupied(bayNumber);
        }

        public bool IsOnlyTopPositionOccupied(BayNumber bayNumber)
        {
            return this.carouselProvider.IsOnlyTopPositionOccupied(bayNumber);
        }

        public bool MoveCarousel(int? loadUnitId, MessageActor sender, BayNumber requestingBay, bool restore)
        {
            if (restore)
            {
                var bay = this.baysDataProvider.GetByNumber(requestingBay);
                var distance = bay.Carousel.ElevatorDistance - (this.baysDataProvider.GetChainPosition(requestingBay) - bay.Carousel.LastIdealPosition);
                try
                {
                    this.carouselProvider.MoveManual(VerticalMovementDirection.Up, distance, requestingBay, sender);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    this.carouselProvider.Move(VerticalMovementDirection.Up, loadUnitId, requestingBay, sender);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
        }

        public void MoveLoadingUnit(HorizontalMovementDirection direction, bool moveToCradle, ShutterPosition moveShutter, bool measure, MessageActor sender, BayNumber requestingBay, int? loadUnitId)
        {
            //TODO***********REFACTOR THIS
            this.elevatorProvider.MoveHorizontalAuto(direction, !moveToCradle, loadUnitId, null, (moveShutter != ShutterPosition.NotSpecified), measure, requestingBay, sender);

            if (moveShutter != ShutterPosition.NotSpecified)
            {
                this.shutterProvider.MoveTo(moveShutter, requestingBay, sender);
            }
        }

        public MessageStatus MoveLoadingUnitStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning
                || message.Type == MessageType.ShutterPositioning
                || message.Type == MessageType.Homing
                )
            {
                return message.Status;
            }

            return MessageStatus.NotSpecified;
        }

        public bool MoveManualLoadingUnitBack(HorizontalMovementDirection direction, int? loadUnitId, MessageActor sender, BayNumber requestingBay)
        {
            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);
            var distance = Math.Abs(this.elevatorDataProvider.HorizontalPosition - axis.LastIdealPosition);
            if (distance < Math.Abs(axis.ChainOffset / 2))
            {
                return false;
            }
            this.elevatorProvider.MoveHorizontalManual(direction, distance, false, loadUnitId, requestingBay, sender);
            return true;
        }

        public bool MoveManualLoadingUnitForward(HorizontalMovementDirection direction, bool isLoadingUnitOnBoard, bool measure, int? loadUnitId, MessageActor sender, BayNumber requestingBay)
        {
            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);
            var profileType = this.elevatorProvider.SelectProfileType(direction, isLoadingUnitOnBoard);
            var profileSteps = axis.Profiles
                .Single(p => p.Name == profileType)
                .Steps
                .OrderBy(s => s.Number);
            var compensation = Math.Abs(this.elevatorDataProvider.HorizontalPosition - axis.LastIdealPosition);
            var distance = profileSteps.Last().Position - compensation;
            if (distance > profileSteps.Last().Position)
            {
                distance = profileSteps.Last().Position;
            }
            this.elevatorProvider.MoveHorizontalManual(direction, distance, measure, loadUnitId, requestingBay, sender);
            return true;
        }

        public void NotifyAssignedMissionOperationChanged(BayNumber bayNumber, int missionId)
        {
            var data = new AssignedMissionOperationChangedMessageData
            {
                BayNumber = bayNumber,
                MissionId = missionId
            };

            this.PublishNotification(
                data,
                $"Mission operation assigned to bay {bayNumber} has changed.",
                MessageActor.MissionManager,
                MessageActor.MachineManager,
                MessageType.AssignedMissionOperationChanged,
                bayNumber,
                BayNumber.None,
                MessageStatus.OperationExecuting,
                ErrorLevel.None);
        }

        public void OpenShutter(MessageActor sender, ShutterPosition openShutter, BayNumber requestingBay, bool restore)
        {
            if (restore)
            {
                this.shutterProvider.Move(ShutterMovementDirection.Up, requestingBay, sender);
            }
            else
            {
                this.shutterProvider.MoveTo(openShutter, requestingBay, sender);
            }
        }

        /// <summary>
        /// Moves elevator to targetHeight.
        /// At the same time if sourceType is a bay it closes the shutter (only for external bays)
        /// </summary>
        /// <param name="targetHeight"></param>
        /// <param name="closeShutter"></param>
        /// <param name="sender"></param>
        /// <param name="requestingBay"></param>
        public void PositionElevatorToPosition(double targetHeight, bool closeShutter, bool measure, MessageActor sender, BayNumber requestingBay, bool restore, int? targetBayPositionId, int? targetCellId)
        {
            if (closeShutter)
            {
                try
                {
                    this.shutterProvider.MoveTo(ShutterPosition.Closed, requestingBay, sender);
                }
                catch (InvalidOperationException ex)
                {
                    if (restore)
                    {
                        this.shutterProvider.Move(ShutterMovementDirection.Down, requestingBay, sender);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            this.elevatorProvider.MoveToAbsoluteVerticalPosition(
                false,
                targetHeight,
                true,
                measure,
                targetBayPositionId,
                targetCellId,
                requestingBay,
                MessageActor.MachineManager);
        }

        public MessageStatus PositionElevatorToPositionStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning ||
                message.Type == MessageType.ShutterPositioning ||
                message.Type == MessageType.MachineManagerException ||
                message.Type == MessageType.Homing)
            {
                return message.Status;
            }

            return MessageStatus.NotSpecified;
        }

        public MessageStatus ShutterStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.ShutterPositioning)
            {
                return message.Status;
            }
            return MessageStatus.NotSpecified;
        }

        public void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay)
        {
            if (targetBay == BayNumber.All)
            {
                foreach (var bay in this.baysDataProvider.GetAll())
                {
                    this.PublishCommand(
                        messageData,
                        $"Requesting operation stop from bay {requestingBay} to bay {bay.Number} for reason {messageData.StopReason}",
                        MessageActor.DeviceManager,
                        sender,
                        MessageType.Stop,
                        requestingBay,
                        bay.Number);
                }
            }
            else
            {
                this.PublishCommand(
                    messageData,
                    $"Requesting operation stop from bay {requestingBay} to bay {targetBay} for reason {messageData.StopReason}",
                    MessageActor.DeviceManager,
                    sender,
                    MessageType.Stop,
                    requestingBay,
                    targetBay);
            }
        }

        public MessageStatus StopOperationStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Stop
                || message.Type == MessageType.PowerEnable
                || message.Type == MessageType.InverterStop
                || message.Type == MessageType.InverterPowerEnable)
            {
                return message.Status;
            }

            if (message.Status == MessageStatus.OperationFaultStop ||
                message.Status == MessageStatus.OperationStop ||
                message.Status == MessageStatus.OperationRunningStop)
            {
                return MessageStatus.OperationEnd;
            }

            return MessageStatus.NotSpecified;
        }

        public void UpdateLastBayChainPosition(BayNumber requestingBay)
        {
            var position = this.baysDataProvider.GetChainPosition(requestingBay);
            this.baysDataProvider.UpdateLastIdealPosition(position, requestingBay);
        }

        public void UpdateLastIdealPosition(HorizontalMovementDirection direction, bool isLoadingUnitOnBoard)
        {
            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);
            var profileType = this.elevatorProvider.SelectProfileType(direction, isLoadingUnitOnBoard);
            var profileSteps = axis.Profiles
                .Single(p => p.Name == profileType)
                .Steps
                .OrderBy(s => s.Number);
            var directionMultiplier = (direction == HorizontalMovementDirection.Forwards ? 1 : -1);
            this.elevatorDataProvider.UpdateLastIdealPosition(axis.LastIdealPosition + (profileSteps.Last().Position * directionMultiplier));
        }

        #endregion
    }
}
