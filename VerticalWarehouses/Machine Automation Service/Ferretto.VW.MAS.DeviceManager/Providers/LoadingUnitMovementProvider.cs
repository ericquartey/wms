using System;
using System.Linq;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver;
using Ferretto.VW.MAS.Utils.Exceptions;
using Microsoft.Extensions.Logging;
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

        private readonly IErrorsProvider errorsProvider;

        private readonly IExternalBayProvider externalBayProvider;

        private readonly IInvertersProvider invertersProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<LoadingUnitMovementProvider> logger;

        private readonly IShutterProvider shutterProvider;

        #endregion

        #region Constructors

        public LoadingUnitMovementProvider(
            IBaysDataProvider baysDataProvider,
            ICarouselProvider carouselProvider,
            IExternalBayProvider externalBayProvider,
            ICellsProvider cellsProvider,
            IElevatorProvider elevatorProvider,
            IElevatorDataProvider elevatorDataProvider,
            IErrorsProvider errorsProvider,
            IShutterProvider shutterProvider,
            IEventAggregator eventAggregator,
            IInvertersProvider invertersProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            ILogger<LoadingUnitMovementProvider> logger)
            : base(eventAggregator)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.carouselProvider = carouselProvider ?? throw new ArgumentNullException(nameof(carouselProvider));
            this.externalBayProvider = externalBayProvider ?? throw new ArgumentNullException(nameof(externalBayProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.elevatorProvider = elevatorProvider ?? throw new ArgumentNullException(nameof(elevatorProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.shutterProvider = shutterProvider ?? throw new ArgumentNullException(nameof(shutterProvider));
            this.invertersProvider = invertersProvider ?? throw new ArgumentNullException(nameof(invertersProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        private bool IsHomingExecuted { get; set; }

        #endregion

        #region Methods

        public double AdjustHeightWithBayChainWeight(double height, double weightUp, double weightDown)
        {
            var newHeight = height;
            if (weightUp > 0 || weightDown > 0)
            {
                newHeight -= (0.004 * (weightUp + (weightDown / 4)));
                this.logger.LogInformation($"Vertical positioning to bay adjusted from {height:0.00} to {newHeight:0.00}. Upper weight {weightUp:0.00}, Down Weight {weightDown:0.00}");
            }
            return newHeight;
        }

        public MessageStatus CarouselStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning
                || message.Type == MessageType.Homing
                || message.Type == MessageType.ShutterPositioning
                )
            {
                return message.Status;
            }
            if (message.Status == MessageStatus.OperationError
                || message.Status == MessageStatus.OperationStop
                || message.Status == MessageStatus.OperationFaultStop
                || message.Status == MessageStatus.OperationRunningStop
                )
            {
                return message.Status;
            }
            if (message.Type == MessageType.RunningStateChanged)
            {
                if (message.Data is StateChangedMessageData data
                    && !data.CurrentState)
                {
                    return MessageStatus.OperationError;
                }
            }

            return MessageStatus.NotSpecified;
        }

        public MachineErrorCode CheckBaySensors(Bay bay, LoadingUnitLocation loadingUnitPosition, bool deposit)
        {
            var bayPosition = this.baysDataProvider.GetPositionByLocation(loadingUnitPosition);

            if (bay.Carousel != null)
            {
                if (deposit)
                {
                    return this.carouselProvider.CanElevatorDeposit(bayPosition);
                }
                else
                {
                    return this.carouselProvider.CanElevatorPickup(bayPosition);
                }
            }

            if (bay.External != null)
            {
                if (deposit)
                {
                    return this.externalBayProvider.CanElevatorDeposit(bay.Number);
                }
                else
                {
                    return this.externalBayProvider.CanElevatorPickup(bay.Number);
                }
            }

            return MachineErrorCode.NoError;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="requestingBay"></param>
        /// <param name="restore"></param>
        public void CloseShutter(MessageActor sender, BayNumber requestingBay, bool restore, ShutterPosition shutterPosition = ShutterPosition.Closed)
        {
            // The operation cannot be executed if shutter does not exist
            if (this.baysDataProvider.GetShutterInverterIndex(requestingBay) == InverterDriver.Contracts.InverterIndex.None)
            {
                return;
            }

            try
            {
                if (restore)
                {
                    this.shutterProvider.Move(ShutterMovementDirection.Down, bypassConditions: false, requestingBay, sender);
                }
                else
                {
                    this.shutterProvider.MoveTo(shutterPosition, requestingBay, sender);
                }
            }
            catch (InvalidOperationException ex)
            {
                this.errorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, requestingBay, ex.Message);
                throw new StateMachineException(ex.Message, requestingBay, sender);
            }
        }

        public void ContinuePositioning(MessageActor sender, BayNumber requestingBay)
        {
            this.elevatorProvider.ContinuePositioning(requestingBay, sender);
        }

        public MessageStatus ExternalBayStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning
                || message.Type == MessageType.Homing
                || message.Type == MessageType.ShutterPositioning
                )
            {
                return message.Status;
            }
            if (message.Status == MessageStatus.OperationError
                || message.Status == MessageStatus.OperationStop
                || message.Status == MessageStatus.OperationFaultStop
                || message.Status == MessageStatus.OperationRunningStop
                )
            {
                return message.Status;
            }
            if (message.Type == MessageType.RunningStateChanged)
            {
                if (message.Data is StateChangedMessageData data
                    && !data.CurrentState)
                {
                    return MessageStatus.OperationError;
                }
            }

            return MessageStatus.NotSpecified;
        }

        public bool FilterNotifications(NotificationMessage notification, MessageActor destination)
        {
            if (notification.Status == MessageStatus.OperationStart)
            {
                return false;
            }
            return (notification.Destination == MessageActor.Any || notification.Destination == destination) &&
                (notification.Type == MessageType.Positioning ||
                 notification.Type == MessageType.Stop ||
                 notification.Type == MessageType.InverterStop ||
                 notification.Type == MessageType.ShutterPositioning ||
                 notification.Type == MessageType.Homing ||
                 notification.Type == MessageType.RunningStateChanged ||
                 notification.Type == MessageType.CombinedMovements ||
                 notification.Type == MessageType.CheckIntrusion ||
                 notification.Status == MessageStatus.OperationStop ||
                 notification.Status == MessageStatus.OperationError ||
                 notification.Status == MessageStatus.OperationFaultStop ||
                 notification.Status == MessageStatus.OperationRunningStop);
        }

        public BayNumber GetBayByCell(int cellId)
        {
            var bayNumber = BayNumber.None;
            var cell = this.cellsProvider.GetById(cellId);
            if (cell != null)
            {
                var bay = this.baysDataProvider.GetByCell(cell);
                if (bay != null)
                {
                    bayNumber = bay.Number;
                }
            }
            return bayNumber;
        }

        public double GetCurrentHorizontalPosition()
        {
            return this.elevatorProvider.HorizontalPosition;
        }

        public double GetCurrentVerticalPosition()
        {
            return this.elevatorProvider.VerticalPosition;
        }

        public int GetCyclesFromCalibration()
        {
            return this.elevatorDataProvider.GetCyclesFromCalibration();
        }

        public double? GetDestinationHeight(Mission moveData, out int? targetBayPositionId, out int? targetCellId)
        {
            double? targetPosition = null;
            targetBayPositionId = null;
            targetCellId = null;

            switch (moveData.LoadUnitDestination)
            {
                case LoadingUnitLocation.LoadUnit:
                    // Retrieve loading unit position
                    if (moveData.LoadUnitId != 0)
                    {
                        var cell = this.cellsProvider.GetByLoadingUnitId(moveData.LoadUnitId);
                        if (cell != null && cell.IsFree)
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
                        if (cell != null && cell.IsFree)
                        {
                            targetPosition = cell.Position;
                            targetCellId = cell.Id;
                        }
                    }
                    break;

                case LoadingUnitLocation.Elevator:
                    targetPosition = this.elevatorProvider.VerticalPosition;
                    if (moveData.LoadUnitSource != LoadingUnitLocation.Cell)
                    {
                        targetBayPositionId = this.baysDataProvider.GetPositionByLocation(moveData.LoadUnitSource).Id;
                    }
                    break;

                default:
                    if (moveData.LoadUnitDestination != LoadingUnitLocation.NoLocation)
                    {
                        targetPosition = this.baysDataProvider.GetLoadingUnitDestinationHeight(moveData.LoadUnitDestination);
                        if (targetPosition.HasValue)
                        {
                            targetBayPositionId = this.baysDataProvider.GetPositionByLocation(moveData.LoadUnitDestination).Id;
                        }
                    }
                    break;
            }
            return targetPosition;
        }

        public double GetLastVerticalPosition()
        {
            var axis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);
            return axis.LastIdealPosition;
        }

        public ShutterPosition GetShutterClosedPosition(Bay bay, LoadingUnitLocation location)
        {
            var closeShutter = ShutterPosition.NotSpecified;

            if (bay.Shutter == null)
            {
                return closeShutter;
            }

            if (bay.Shutter.Type != ShutterType.NotSpecified)
            {
                if (bay.Shutter.Type == ShutterType.TwoSensors)
                {
                    closeShutter = ShutterPosition.Closed;
                }
                else
                {
                    var bayPosition = bay.Positions.FirstOrDefault(x => x.Location == location);
                    if (bayPosition is null)
                    {
                        // TODO: throw an exception?
                        closeShutter = ShutterPosition.NotSpecified;
                    }
                    else if (bay.Positions.Any(x => x.LoadingUnit != null))
                    {
                        closeShutter = ShutterPosition.Half;
                    }
                    else
                    {
                        closeShutter = ShutterPosition.Closed;
                    }
                }
            }
            return closeShutter;
        }

        public ShutterPosition GetShutterOpenPosition(Bay bay, LoadingUnitLocation location)
        {
            var openShutter = ShutterPosition.NotSpecified;

            if (bay.Shutter == null)
            {
                return openShutter;
            }

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

            switch (moveData.LoadUnitSource)
            {
                case LoadingUnitLocation.LoadUnit:
                    // Retrieve loading unit position
                    if (moveData.LoadUnitId != 0)
                    {
                        var cell = this.cellsProvider.GetByLoadingUnitId(moveData.LoadUnitId);
                        if (cell != null && !cell.IsFree)
                        {
                            targetPosition = cell.Position;
                            targetCellId = cell.Id;
                        }
                    }
                    break;

                case LoadingUnitLocation.Cell:
                    // Retrieve Cell height
                    if (moveData.LoadUnitCellSourceId != null)
                    {
                        var cell = this.cellsProvider.GetById(moveData.LoadUnitCellSourceId.Value);
                        if (cell != null && !cell.IsFree)
                        {
                            targetPosition = cell.Position;
                            targetCellId = cell.Id;
                        }
                    }
                    break;

                default:
                    targetPosition = this.baysDataProvider.GetLoadingUnitDestinationHeight(moveData.LoadUnitSource);
                    targetBayPositionId = this.baysDataProvider.GetPositionByLocation(moveData.LoadUnitSource).Id;
                    break;
            }
            return targetPosition;
        }

        public void Homing(Axis axis, Calibration calibration, int loadingUnitId, bool showErrors, BayNumber requestingBay, MessageActor sender)
        {
            if (axis == Axis.BayChain)
            {
                var bay = this.baysDataProvider.GetByNumber(requestingBay);
                if (bay.Carousel != null)
                {
                    this.carouselProvider.Homing(calibration, loadingUnitId, showErrors, requestingBay, sender);
                }
                if (bay.External != null)
                {
                    this.externalBayProvider.Homing(calibration, loadingUnitId, showErrors, requestingBay, sender);
                }
            }
            else
            {
                this.elevatorProvider.Homing(axis, calibration, loadingUnitId, showErrors, requestingBay, sender);
            }
        }

        public bool IsExternalPositionOccupied(BayNumber bayNumber)
        {
            return this.externalBayProvider.IsExternalPositionOccupied(bayNumber);
        }

        public bool IsInternalPositionOccupied(BayNumber bayNumber)
        {
            return this.externalBayProvider.IsInternalPositionOccupied(bayNumber);
        }

        public bool IsOnlyBottomPositionOccupied(BayNumber bayNumber)
        {
            return this.carouselProvider.IsOnlyBottomPositionOccupied(bayNumber);
        }

        public bool IsOnlyTopPositionOccupied(BayNumber bayNumber)
        {
            return this.carouselProvider.IsOnlyTopPositionOccupied(bayNumber);
        }

        public void MoveCarousel(int? loadUnitId, MessageActor sender, BayNumber requestingBay, bool restore)
        {
            if (restore)
            {
                var bay = this.baysDataProvider.GetByNumber(requestingBay);
                var distance = bay.Carousel.ElevatorDistance - (this.baysDataProvider.GetChainPosition(requestingBay) - bay.Carousel.LastIdealPosition);
                try
                {
                    this.carouselProvider.MoveManual(VerticalMovementDirection.Up, distance, loadUnitId, false, requestingBay, sender);
                }
                catch (InvalidOperationException ex)
                {
                    // we don't want to show errors here. It is managed by MissionMoveBayChainStep
                    throw new StateMachineException(ex.Message, requestingBay, sender);
                }
            }
            else
            {
                try
                {
                    this.carouselProvider.Move(VerticalMovementDirection.Up, loadUnitId, requestingBay, sender);
                }
                catch (InvalidOperationException ex)
                {
                    // we don't want to show errors here. It is managed by MissionMoveBayChainStep
                    throw new StateMachineException(ex.Message, requestingBay, sender);
                }
            }
        }

        public bool MoveExternalBay(int? loadUnitId, ExternalBayMovementDirection direction, MessageActor sender, BayNumber requestingBay, bool restore)
        {
            if (restore)
            {
                var bay = this.baysDataProvider.GetByNumber(requestingBay);
                var distance = bay.External.Race;
                switch (direction)
                {
                    case ExternalBayMovementDirection.TowardMachine:
                        distance = this.baysDataProvider.GetChainPosition(requestingBay) - bay.ChainOffset;
                        break;

                    case ExternalBayMovementDirection.TowardOperator:
                        distance -= this.baysDataProvider.GetChainPosition(requestingBay); //+ bay.ChainOffset;
                        break;
                }
                if (distance < Math.Abs(bay.ChainOffset / 2))
                {
                    return false;
                }

                try
                {
                    this.externalBayProvider.MoveManual(direction, distance, loadUnitId, bypassConditions: false, requestingBay, sender);
                }
                catch (InvalidOperationException ex)
                {
                    this.errorsProvider.RecordNew(MachineErrorCode.MoveExtBayNotAllowed, requestingBay, ex.Message);
                    throw new StateMachineException(ex.Message, requestingBay, sender);
                }
            }
            else
            {
                try
                {
                    this.externalBayProvider.Move(direction, loadUnitId, requestingBay, sender);
                }
                catch (InvalidOperationException ex)
                {
                    this.errorsProvider.RecordNew(MachineErrorCode.MoveExtBayNotAllowed, requestingBay, ex.Message);
                    throw new StateMachineException(ex.Message, requestingBay, sender);
                }
            }
            return true;
        }

        public void MoveLoadingUnit(HorizontalMovementDirection direction, bool moveToCradle, ShutterPosition moveShutter, bool measure, MessageActor sender, BayNumber requestingBay, int? loadUnitId, int? positionId, bool fastDeposit = true)
        {
            try
            {
                this.elevatorProvider.MoveHorizontalAuto(
                    direction,
                    !moveToCradle,
                    loadUnitId,
                    loadingUnitNetWeight: null,
                    (moveShutter != ShutterPosition.NotSpecified),
                    measure,
                    requestingBay,
                    sender,
                    sourceBayPositionId: positionId,
                    fastDeposit: fastDeposit);
            }
            catch (InvalidOperationException ex)
            {
                this.errorsProvider.RecordNew(MachineErrorCode.ConditionsNotMetForPositioning, requestingBay, ex.Message);
                throw new StateMachineException(ex.Message, requestingBay, sender);
            }
            if (moveShutter != ShutterPosition.NotSpecified)
            {
                try
                {
                    this.shutterProvider.MoveTo(moveShutter, requestingBay, sender);
                }
                catch (InvalidOperationException ex)
                {
                    this.errorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterInvalid, requestingBay, ex.Message);
                    throw new StateMachineException(ex.Message, requestingBay, sender);
                }
            }
        }

        public MessageStatus MoveLoadingUnitStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning
                || message.Type == MessageType.ShutterPositioning
                || message.Type == MessageType.Homing
                || message.Type == MessageType.CombinedMovements
                || message.Type == MessageType.CheckIntrusion
                )
            {
                return message.Status;
            }
            if (message.Status == MessageStatus.OperationError
                || message.Status == MessageStatus.OperationStop
                || message.Status == MessageStatus.OperationRunningStop
                || message.Status == MessageStatus.OperationFaultStop
                || message.Type == MessageType.Stop
                )
            {
                return MessageStatus.OperationError;
            }
            if (message.Type == MessageType.RunningStateChanged)
            {
                if (message.Data is StateChangedMessageData data
                    && !data.CurrentState)
                {
                    return MessageStatus.OperationError;
                }
            }

            return MessageStatus.NotSpecified;
        }

        public bool MoveManualLoadingUnitBackward(HorizontalMovementDirection direction, int? loadUnitId, MessageActor sender, BayNumber requestingBay)
        {
            // Horizontal
            var horizontalAxis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);
            var distance = Math.Abs(this.elevatorDataProvider.HorizontalPosition - horizontalAxis.LastIdealPosition);
            if (distance < Math.Abs(horizontalAxis.ChainOffset / 2))
            {
                return false;
            }
            if (distance > horizontalAxis.Profiles.First().TotalDistance)
            {
                this.errorsProvider.RecordNew(MachineErrorCode.AutomaticRestoreNotAllowed, requestingBay);
                throw new StateMachineException(ErrorDescriptions.AutomaticRestoreNotAllowed, requestingBay, MessageActor.MachineManager);
            }

            // Vertical
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

            var grossWeight = 0.0d;
            if (loadUnitId.HasValue)
            {
                var loadUnit = this.loadingUnitsDataProvider.GetById(loadUnitId.Value);
                grossWeight = loadUnit.GrossWeight;
            }
            var dTmp = this.invertersProvider.ComputeDisplacement(verticalAxis.LastIdealPosition, grossWeight);
            var factor = Math.Abs(this.elevatorDataProvider.VerticalPosition - verticalAxis.LastIdealPosition) / Math.Abs(dTmp);
            var verticalDisplacement = dTmp * factor;
            verticalDisplacement *= (this.elevatorDataProvider.GetLoadingUnitOnBoard() == null) ? -1.0d : +1.0d;
            var b = (this.elevatorDataProvider.GetLoadingUnitOnBoard() != null);

            this.logger.LogDebug($"Horizontal movement distance:{distance} mm - Vertical movement displacement:{verticalDisplacement} mm [total displacement:{dTmp} mm, percentile displacement performed:{factor * 100} %, loading unit on elevator:{b}]");

            this.elevatorProvider.MoveHorizontalManual(direction, distance, verticalDisplacement, false, loadUnitId, null, false, requestingBay, sender);
            return true;
        }

        public bool MoveManualLoadingUnitForward(HorizontalMovementDirection direction, bool isLoadingUnitOnBoard, bool measure, int? loadUnitId, int? positionId, MessageActor sender, BayNumber requestingBay)
        {
            // Horizontal
            var horizontalAxis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);
            var profileType = this.elevatorProvider.SelectProfileType(direction, isLoadingUnitOnBoard);
            var profileSteps = horizontalAxis.Profiles
                .Single(p => p.Name == profileType)
                .Steps
                .OrderBy(s => s.Number);
            var compensation = Math.Abs(this.elevatorDataProvider.HorizontalPosition - horizontalAxis.LastIdealPosition);
            var distance = profileSteps.Last().Position - compensation;
            if (distance > profileSteps.Last().Position)
            {
                distance = profileSteps.Last().Position;
            }
            else if (distance <= 0)
            {
                // already arrived at destination?
                distance = 1;
            }

            // Vertical
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

            var grossWeight = 0.0d;
            if (loadUnitId.HasValue)
            {
                var loadUnit = this.loadingUnitsDataProvider.GetById(loadUnitId.Value);
                grossWeight = loadUnit.GrossWeight;
            }
            var dTmp = this.invertersProvider.ComputeDisplacement(verticalAxis.LastIdealPosition, grossWeight);
            var factor = 1.0d - (Math.Abs(this.elevatorDataProvider.VerticalPosition - verticalAxis.LastIdealPosition) / Math.Abs(dTmp));
            var verticalDisplacement = dTmp * factor;
            verticalDisplacement *= (this.elevatorDataProvider.GetLoadingUnitOnBoard() == null) ? +1.0d : -1.0d;
            var b = (this.elevatorDataProvider.GetLoadingUnitOnBoard() != null);

            this.logger.LogDebug($"Horizontal movement distance:{distance:0.00} mm - Vertical movement displacement:{verticalDisplacement:0.00} mm [total displacement:{dTmp:0.00} mm, percentile displacement performed:{factor * 100:0.00} %, loading unit on elevator:{b}]");

            this.elevatorProvider.MoveHorizontalManual(direction, distance, verticalDisplacement, measure, loadUnitId, positionId, false, requestingBay, sender);
            return true;
        }

        public void NotifyAssignedMissionOperationChanged(BayNumber bayNumber, int missionId)
        {
            var data = new AssignedMissionChangedMessageData
            {
                BayNumber = bayNumber,
                MissionId = missionId
            };

            this.PublishNotification(
                data,
                $"Mission operation assigned to bay {bayNumber} has changed.",
                MessageActor.MissionManager,
                MessageActor.MachineManager,
                MessageType.AssignedMissionChanged,
                bayNumber,
                BayNumber.None,
                MessageStatus.OperationExecuting,
                ErrorLevel.None);
        }

        public void OpenShutter(MessageActor sender, ShutterPosition openShutter, BayNumber requestingBay, bool restore)
        {
            // The operation cannot be executed if shutter does not exist
            if (this.baysDataProvider.GetShutterInverterIndex(requestingBay) == InverterDriver.Contracts.InverterIndex.None)
            {
                return;
            }

            if (restore)
            {
                this.shutterProvider.Move(ShutterMovementDirection.Up, bypassConditions: false, requestingBay, sender);
            }
            else
            {
                try
                {
                    this.shutterProvider.MoveTo(openShutter, requestingBay, sender);
                }
                catch (InvalidOperationException ex)
                {
                    this.errorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed, requestingBay, ex.Message);
                    throw new StateMachineException(ex.Message, requestingBay, sender);
                }
            }
        }

        /// <summary>
        /// Moves elevator to targetHeight.
        /// At the same time if sourceType is a bay it closes the shutter (only for external bays)
        /// </summary>
        /// <param name="targetHeight"></param>
        /// <param name="shutterBay"></param>
        /// <param name="sender"></param>
        /// <param name="requestingBay"></param>
        public void PositionElevatorToPosition(double targetHeight,
            BayNumber shutterBay,
            ShutterPosition shutterPosition,
            bool measure,
            MessageActor sender,
            BayNumber requestingBay,
            bool restore,
            int? targetBayPositionId,
            int? targetCellId,
            bool waitContinue = false)
        {
            if (shutterBay != BayNumber.None)
            {
                this.CloseShutter(MessageActor.MachineManager, shutterBay, restore, shutterPosition);
            }

            try
            {
                if (targetBayPositionId != null)
                {
                    var bay = this.baysDataProvider.GetByBayPositionId(targetBayPositionId.Value);
                    if (bay.IsAdjustByWeight
                        && Math.Abs(targetHeight - bay.Positions.FirstOrDefault(p => p.IsUpper)?.Height ?? 0) < 5
                        )
                    {
                        targetHeight = this.AdjustHeightWithBayChainWeight(targetHeight,
                            bay.Positions.FirstOrDefault(p => p.IsUpper)?.LoadingUnit?.GrossWeight ?? 0,
                            bay.Positions.FirstOrDefault(p => !p.IsUpper)?.LoadingUnit?.GrossWeight ?? 0);
                    }
                }
                this.elevatorProvider.MoveToAbsoluteVerticalPosition(
                    manualMovment: false,
                    targetHeight,
                    computeElongation: true,
                    measure,
                    targetBayPositionId,
                    targetCellId,
                    checkHomingDone: false,
                    waitContinue,
                    isPickupMission: true,
                    requestingBay,
                    MessageActor.MachineManager);
            }
            catch (InvalidOperationException ex)
            {
                this.errorsProvider.RecordNew(MachineErrorCode.ConditionsNotMetForPositioning, requestingBay, ex.Message);
                throw new StateMachineException(ex.Message, requestingBay, sender);
            }
        }

        public MessageStatus PositionElevatorToPositionStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning ||
                message.Type == MessageType.ShutterPositioning ||
                message.Type == MessageType.MachineManagerException ||
                message.Type == MessageType.Homing ||
                message.Type == MessageType.CheckIntrusion
                )
            {
                return message.Status;
            }
            if (message.Type == MessageType.Stop && message.Status == MessageStatus.OperationEnd)
            {
                return MessageStatus.OperationStop;
            }
            if (message.Status == MessageStatus.OperationError
                || message.Status == MessageStatus.OperationStop
                || message.Status == MessageStatus.OperationRunningStop
                || message.Status == MessageStatus.OperationFaultStop
                )
            {
                return message.Status;
            }
            if (message.Type == MessageType.RunningStateChanged)
            {
                if (message.Data is StateChangedMessageData data
                    && !data.CurrentState)
                {
                    return MessageStatus.OperationError;
                }
            }

            return MessageStatus.NotSpecified;
        }

        public void ResumeOperation(int missionId, LoadingUnitLocation loadUnitSource, LoadingUnitLocation loadUnitDestination, int? wmsId, MissionType missionType, BayNumber targetBay, MessageActor sender)
        {
            var data = new MoveLoadingUnitMessageData(
                    missionType,
                    loadUnitSource,
                    loadUnitDestination,
                    sourceCellId: null,
                    destinationCellId: null,
                    loadUnitId: null,
                    insertLoadUnit: false,
                    missionId,
                    CommandAction.Resume);
            data.WmsId = wmsId;
            this.PublishCommand(
                data,
                $"Requesting operation resume from position {loadUnitDestination}",
                MessageActor.MachineManager,
                sender,
                MessageType.MoveLoadingUnit,
                targetBay,
                targetBay);
        }

        public MessageStatus ShutterStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.ShutterPositioning
                || message.Type == MessageType.Homing
                )
            {
                return message.Status;
            }
            if (message.Status == MessageStatus.OperationError
                || message.Status == MessageStatus.OperationStop
                || message.Status == MessageStatus.OperationRunningStop
                || message.Status == MessageStatus.OperationFaultStop
                )
            {
                return message.Status;
            }
            if (message.Type == MessageType.RunningStateChanged)
            {
                if (message.Data is StateChangedMessageData data
                    && !data.CurrentState)
                {
                    return MessageStatus.OperationError;
                }
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
                        $"Requesting operation LU stop from bay {requestingBay} to bay {bay.Number} for reason {messageData.StopReason}",
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
                    $"Requesting operation stop LU from bay {requestingBay} to bay {targetBay} for reason {messageData.StopReason}",
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
                message.Status == MessageStatus.OperationRunningStop ||
                message.Status == MessageStatus.OperationFaultStop)
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
