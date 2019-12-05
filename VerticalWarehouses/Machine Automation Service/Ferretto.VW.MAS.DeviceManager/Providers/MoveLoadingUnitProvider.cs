using System;
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

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly IShutterProvider shutterProvider;

        #endregion

        #region Constructors

        public LoadingUnitMovementProvider(
            IBaysDataProvider baysDataProvider,
            ICellsProvider cellsProvider,
            IElevatorProvider elevatorProvider,
            IElevatorDataProvider elevatorDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IShutterProvider shutterProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.elevatorProvider = elevatorProvider ?? throw new ArgumentNullException(nameof(elevatorProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.shutterProvider = shutterProvider ?? throw new ArgumentNullException(nameof(shutterProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Methods

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

        public double? GetDestinationHeight(Mission moveData)
        {
            double? targetPosition = null;
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
                        }
                    }
                    break;

                default:
                    targetPosition = this.baysDataProvider.GetLoadingUnitDestinationHeight(moveData.LoadingUnitDestination);
                    break;
            }
            return targetPosition;
        }

        public double? GetSourceHeight(Mission moveData)
        {
            double? targetPosition = null;
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
                        }
                    }
                    break;

                default:
                    targetPosition = this.baysDataProvider.GetLoadingUnitDestinationHeight(moveData.LoadingUnitSource);
                    break;
            }
            return targetPosition;
        }

        public void MoveLoadingUnit(HorizontalMovementDirection direction, bool moveToCradle, bool openShutter, bool measure, MessageActor sender, BayNumber requestingBay, int? loadUnitId)
        {
            //TODO***********REFACTOR THIS
            this.elevatorProvider.MoveHorizontalAuto(direction, !moveToCradle, loadUnitId, null, openShutter, measure, requestingBay, sender);

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

        public bool MoveManualLoadingUnit(HorizontalMovementDirection direction, MessageActor sender, BayNumber requestingBay)
        {
            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);
            var distance = Math.Abs(this.elevatorDataProvider.HorizontalPosition - axis.LastIdealPosition);
            if (distance < Math.Abs(axis.ChainOffset / 2))
            {
                return false;
            }
            this.elevatorProvider.MoveHorizontalManual(direction, distance, requestingBay, sender);
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

        public void OpenShutter(MessageActor sender, BayNumber requestingBay, bool restore)
        {
            try
            {
                this.shutterProvider.MoveTo(ShutterPosition.Opened, requestingBay, sender);
            }
            catch (InvalidOperationException ex)
            {
                if (restore)
                {
                    this.shutterProvider.Move(ShutterMovementDirection.Up, requestingBay, sender);
                }
                else
                {
                    throw;
                }
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
        public void PositionElevatorToPosition(double targetHeight, bool closeShutter, bool measure, MessageActor sender, BayNumber requestingBay, bool restore)
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
                measure,
                true,
                requestingBay,
                MessageActor.MachineManager);
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

        #endregion
    }
}
