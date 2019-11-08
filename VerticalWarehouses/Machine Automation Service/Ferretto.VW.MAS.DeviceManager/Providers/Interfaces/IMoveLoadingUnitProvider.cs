using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ILoadingUnitMovementProvider
    {
        #region Methods

        void CloseShutter(MessageActor sender, BayNumber requestingBay);

        void ContinuePositioning(MessageActor sender, BayNumber requestingBay);

        bool FilterNotifications(NotificationMessage notification, MessageActor destination);

        double? GetDestinationHeight(IMoveLoadingUnitMessageData messageData);

        double? GetSourceHeight(IMoveLoadingUnitMessageData messageData);

        void MoveLoadingUnit(HorizontalMovementDirection direction, bool moveToCradle, bool openShutter, MessageActor sender, BayNumber requestingBay, int? loadUnitId);

        MessageStatus MoveLoadingUnitStatus(NotificationMessage message);

        void PositionElevatorToPosition(double targetHeight, bool closeShutter, MessageActor sender, BayNumber requestingBay);

        MessageStatus PositionElevatorToPositionStatus(NotificationMessage message);

        MessageStatus ShutterStatus(NotificationMessage message);

        void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        MessageStatus StopOperationStatus(NotificationMessage message);

        #endregion
    }
}
