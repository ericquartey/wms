using System;
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

        public LoadingUnitMovementProvider(
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

        public void CloseShutter(MessageActor sender, BayNumber requestingBay)
        {
            this.shutterProvider.MoveTo(ShutterPosition.Closed, requestingBay, sender);
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
                 notification.Type == MessageType.ShutterPositioning ||
                 notification.Status == MessageStatus.OperationStop ||
                 notification.Status == MessageStatus.OperationError ||
                 notification.Status == MessageStatus.OperationFaultStop ||
                 notification.Status == MessageStatus.OperationRunningStop);
        }

        public double? GetDestinationHeight(IMoveLoadingUnitMessageData messageData)
        {
            double? targetPosition = null;
            switch (messageData.Destination)
            {
                case LoadingUnitLocation.LoadingUnit:
                    // Retrieve loading unit position
                    if (messageData.LoadingUnitId != null)
                    {
                        var cell = this.cellsProvider.GetCellByLoadingUnit(messageData.LoadingUnitId.Value);
                        if (cell != null && cell.Status == CellStatus.Free)
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
                        if (cell != null && cell.Status == CellStatus.Free)
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

        public double? GetSourceHeight(IMoveLoadingUnitMessageData messageData)
        {
            double? targetPosition = null;
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

        public void MoveLoadingUnit(HorizontalMovementDirection direction, bool moveToCradle, bool openShutter, MessageActor sender, BayNumber requestingBay)
        {
            this.elevatorProvider.MoveHorizontalAuto(direction, !moveToCradle, null, null, openShutter, openShutter, requestingBay, sender);

            if (openShutter)
            {
                this.shutterProvider.MoveTo(ShutterPosition.Opened, requestingBay, sender);
            }
        }

        public MessageStatus MoveLoadingUnitStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning || message.Type == MessageType.ShutterPositioning)
            {
                return message.Status;
            }

            return MessageStatus.NotSpecified;
        }

        /// <summary>
        /// Moves elevator to targetHeight.
        /// At the same time if sourceType is a bay it closes the shutter (only for external bays)
        /// </summary>
        /// <param name="targetHeight"></param>
        /// <param name="closeShutter"></param>
        /// <param name="sender"></param>
        /// <param name="requestingBay"></param>
        public void PositionElevatorToPosition(double targetHeight, bool closeShutter, MessageActor sender, BayNumber requestingBay)
        {
            var parameters = this.setupProceduresDataProvider.GetVerticalManualMovements();

            if (closeShutter)
            {
                this.shutterProvider.MoveTo(ShutterPosition.Closed, requestingBay, sender);
            }
            this.elevatorProvider.MoveToVerticalPosition(targetHeight, parameters.FeedRateAfterZero, closeShutter, true, requestingBay, MessageActor.MachineManager);
        }

        public MessageStatus PositionElevatorToPositionStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning ||
                message.Type == MessageType.ShutterPositioning ||
                message.Type == MessageType.MachineManagerException)
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
                foreach (var bay in this.baysProvider.GetAll())
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
            if (message.Type == MessageType.Stop)
            {
                return message.Status;
            }

            if (message.Status == MessageStatus.OperationFaultStop ||
                message.Status == MessageStatus.OperationRunningStop)
            {
                return MessageStatus.OperationEnd;
            }

            return MessageStatus.NotSpecified;
        }

        #endregion
    }
}
