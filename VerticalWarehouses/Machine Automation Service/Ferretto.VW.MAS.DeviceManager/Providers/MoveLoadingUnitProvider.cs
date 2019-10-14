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

        #endregion

        #region Constructors

        protected LoadingUnitMovementProvider(
            IBaysProvider baysProvider,
            ICellsProvider cellsProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
        }

        #endregion

        #region Methods

        public bool FilterNotifications(NotificationMessage notification, MessageActor destination)
        {
            return (notification.Destination == MessageActor.Any || notification.Destination == destination) &&
                (notification.Type == MessageType.Positioning);
        }

        public double GetDestinationHeight(IMoveLoadingUnitMessageData messageData, out int? loadingUnitId)
        {
            throw new NotImplementedException();
        }

        public double GetSourceHeight(IMoveLoadingUnitMessageData messageData, out int? loadingUnitId)
        {
            loadingUnitId = null;
            double targetPosition = 0;
            switch (messageData.Source)
            {
                case LoadingUnitDestination.LoadingUnit:
                    // Retrieve loading unit position
                    if (messageData.LoadingUnitId != null)
                    {
                        var cell = this.cellsProvider.GetCellByLoadingUnit(messageData.LoadingUnitId.Value);
                        if (cell != null && cell.Status == CellStatus.Occupied)
                        {
                            targetPosition = cell.Position;
                            loadingUnitId = messageData.LoadingUnitId;
                        }
                    }
                    break;

                case LoadingUnitDestination.Cell:
                    // Retrieve Cell height
                    if (messageData.SourceCellId != null)
                    {
                        var cell = this.cellsProvider.GetCellById(messageData.SourceCellId.Value);
                        if (cell != null && cell.Status == CellStatus.Occupied)
                        {
                            targetPosition = cell.Position;
                            loadingUnitId = cell.LoadingUnit.Id;
                        }
                    }
                    break;

                default:
                    targetPosition = this.baysProvider.GetLoadingUnitDestinationHeight(messageData.Source, out loadingUnitId);
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

        public void MoveLoadingUnitToElevator(int? loadingUnitId, MessageActor sender, BayNumber requestingBay)
        {
            throw new NotImplementedException();
        }

        public MessageStatus MoveLoadingUnitToElevatorStatus(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public void PositionElevatorToPosition(double targetHeight, MessageActor sender, BayNumber requestingBay)
        {
            throw new NotImplementedException();
        }

        public MessageStatus PositionElevatorToPositionStatus(NotificationMessage message)
        {
            throw new NotImplementedException();
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
