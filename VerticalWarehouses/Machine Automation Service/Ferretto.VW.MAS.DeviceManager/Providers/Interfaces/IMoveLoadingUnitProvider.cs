using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ILoadingUnitMovementProvider
    {
        #region Methods

        bool FilterNotifications(NotificationMessage notification, MessageActor destination);

        double GetDestinationHeight(IMoveLoadingUnitMessageData messageData, out int? loadingUnitId);

        double GetSourceHeight(IMoveLoadingUnitMessageData messageData, out int? loadingUnitId);

        void MoveLoadingUnitToDestination(int? loadingUnitId, MessageActor sender, BayNumber requestingBay);

        MessageStatus MoveLoadingUnitToDestinationStatus(NotificationMessage message);

        void MoveLoadingUnitToElevator(int? loadingUnitId, MessageActor sender, BayNumber requestingBay);

        MessageStatus MoveLoadingUnitToElevatorStatus(NotificationMessage message);

        void PositionElevatorToPosition(double targetHeight, MessageActor sender, BayNumber requestingBay);

        MessageStatus PositionElevatorToPositionStatus(NotificationMessage message);

        void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        MessageStatus StopOperationStatus(NotificationMessage message);

        #endregion
    }
}
