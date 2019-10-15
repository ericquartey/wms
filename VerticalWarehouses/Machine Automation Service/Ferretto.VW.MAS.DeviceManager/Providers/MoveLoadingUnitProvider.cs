using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class LoadingUnitMovementProvider : BaseProvider, ILoadingUnitMovementProvider
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly IShutterProvider shutterProvider;

        #endregion

        #region Constructors

        protected LoadingUnitMovementProvider(
            IBaysProvider baysProvider,
            ICellsProvider cellsProvider,
            IElevatorProvider elevatorProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IShutterProvider shutterProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.elevatorProvider = elevatorProvider ?? throw new ArgumentNullException(nameof(elevatorProvider));
            this.shutterProvider = shutterProvider ?? throw new ArgumentNullException(nameof(shutterProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Methods

        public bool FilterNotifications(NotificationMessage notification, MessageActor destination)
        {
            return (notification.Destination == MessageActor.Any || notification.Destination == destination) &&
                (notification.Type == MessageType.Positioning);
        }

        public double GetDestinationHeight(IMoveLoadingUnitMessageData messageData)
        {
            double targetPosition = 0;
            switch (messageData.Destination)
            {
                case LoadingUnitLocation.LoadingUnit:
                    // Retrieve loading unit position
                    if (messageData.LoadingUnitId != null)
                    {
                        var cell = this.cellsProvider.GetCellByLoadingUnit(messageData.LoadingUnitId.Value);
                        if (cell != null && cell.Status == CellStatus.Occupied)
                        {
                            targetPosition = cell.Position;
                        }
                    }
                    break;

                case LoadingUnitLocation.Cell:
                    // Retrieve Cell height
                    if (messageData.DestinationCellId != null)
                    {
                        var cell = this.cellsProvider.GetCellById(messageData.DestinationCellId.Value);
                        if (cell != null && cell.Status == CellStatus.Occupied)
                        {
                            targetPosition = cell.Position;
                        }
                    }
                    break;

                default:
                    targetPosition = this.baysProvider.GetLoadingUnitDestinationHeight(messageData.Destination);
                    break;
            }
            return targetPosition;
        }

        public double GetSourceHeight(IMoveLoadingUnitMessageData messageData)
        {
            double targetPosition = 0;
            switch (messageData.Source)
            {
                case LoadingUnitLocation.LoadingUnit:
                    // Retrieve loading unit position
                    if (messageData.LoadingUnitId != null)
                    {
                        var cell = this.cellsProvider.GetCellByLoadingUnit(messageData.LoadingUnitId.Value);
                        if (cell != null && cell.Status == CellStatus.Occupied)
                        {
                            targetPosition = cell.Position;
                        }
                    }
                    break;

                case LoadingUnitLocation.Cell:
                    // Retrieve Cell height
                    if (messageData.SourceCellId != null)
                    {
                        var cell = this.cellsProvider.GetCellById(messageData.SourceCellId.Value);
                        if (cell != null && cell.Status == CellStatus.Occupied)
                        {
                            targetPosition = cell.Position;
                        }
                    }
                    break;

                default:
                    targetPosition = this.baysProvider.GetLoadingUnitDestinationHeight(messageData.Source);
                    break;
            }
            return targetPosition;
        }

        public void MoveLoadingUnitToDestination(int? loadingUnitId, MessageActor sender, BayNumber requestingBay)
        {
            throw new NotImplementedException();
        }

        public MessageStatus MoveLoadingUnitToDestinationStatus(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public void MoveLoadingUnitToElevator(int? loadingUnitId, HorizontalMovementDirection direction, MessageActor sender, BayNumber requestingBay)
        {
            this.elevatorProvider.MoveHorizontalAuto(direction, false, null, null, requestingBay, sender);
        }

        public MessageStatus MoveLoadingUnitToElevatorStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning)
            {
                return message.Status;
            }

            return MessageStatus.NoStatus;
        }

        public List<MovementMode> PositionElevatorToPosition(double targetHeight, LoadingUnitLocation positionType, MessageActor sender, BayNumber requestingBay)
        {
            var movements = new List<MovementMode>();
            if (positionType != LoadingUnitLocation.NoLocation)
            {
                var shutter = this.baysProvider.GetShutterPosition(positionType, out var bay);
                if (shutter != ShutterPosition.None)
                {
                    this.shutterProvider.MoveTo(shutter, bay, MessageActor.MissionsManager);
                    movements.Add(MovementMode.ShutterPosition);
                }
            }
            var parameters = this.setupProceduresDataProvider.GetVerticalManualMovements();
            this.elevatorProvider.MoveToVerticalPosition(targetHeight, parameters.FeedRateAfterZero, requestingBay, MessageActor.MissionsManager);
            movements.Add(MovementMode.Position);
            return movements;
        }

        public MessageStatus PositionElevatorToPositionStatus(NotificationMessage message, List<MovementMode> movements)
        {
            if (message.Type == MessageType.Positioning)
            {
                if (message.Status == MessageStatus.OperationError)
                {
                    return message.Status;
                }
                if (message.Status == MessageStatus.OperationEnd && movements.Contains(MovementMode.Position))
                {
                    movements.Remove(MovementMode.Position);
                }
            }
            else if (message.Type == MessageType.ShutterPositioning)
            {
                if (message.Status == MessageStatus.OperationError)
                {
                    return message.Status;
                }
                if (message.Status == MessageStatus.OperationEnd && movements.Contains(MovementMode.ShutterPosition))
                {
                    movements.Remove(MovementMode.ShutterPosition);
                }
            }
            if (movements.Count == 0)
            {
                return MessageStatus.OperationEnd;
            }
            return MessageStatus.NoStatus;
        }

        public void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay)
        {
            if (targetBay == BayNumber.All)
            {
                foreach (var bay in this.baysProvider.GetAll())
                {
                    this.PublishCommand(
                        messageData,
                        $"Requesting operation stop from bay {requestingBay} to bay {bay.Number} for reason {messageData.StopReason}",
                        MessageActor.FiniteStateMachines,
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
                    MessageActor.FiniteStateMachines,
                    sender,
                    MessageType.Stop,
                    requestingBay,
                    targetBay);
            }
        }

        public MessageStatus StopOperationStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Stop)
            {
                return message.Status;
            }

            if (message.Status == MessageStatus.OperationFaultStop ||
                message.Status == MessageStatus.OperationRunningStop)
            {
                return MessageStatus.OperationEnd;
            }

            return MessageStatus.NoStatus;
        }

        #endregion
    }
}
