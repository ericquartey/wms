using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class LoadingUnitMovementProvider : BaseProvider, ILoadingUnitMovementProvider
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        #endregion

        #region Constructors

        protected LoadingUnitMovementProvider(
            IBaysProvider baysProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
        }

        #endregion

        #region Methods

        public bool FilterNotifications(NotificationMessage notification, MessageActor destination)
        {
            return (notification.Destination == MessageActor.Any || notification.Destination == destination) &&
                (notification.Type == MessageType.Positioning);
        }

        public void PositionElevatorToSource(IMoveLoadingUnitMessageData messageData, MessageActor sender, BayNumber requestingBay)
        {
            double targetPosition = 0;
            switch (messageData.Source)
            {
                case LoadingUnitDestination.LoadingUnit:
                    // Retrieve loading unit position
                    break;

                case LoadingUnitDestination.Cell:
                    // Retrieve Cell height
                    break;

                default:
                    targetPosition = this.baysProvider.GetLoadingUnitDestinationHeight(messageData.Source);
                    break;
            }
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
