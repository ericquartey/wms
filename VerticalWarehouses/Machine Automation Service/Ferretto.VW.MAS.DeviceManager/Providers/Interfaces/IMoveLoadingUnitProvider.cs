using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ILoadingUnitMovementProvider
    {
        #region Methods

        MessageStatus CarouselStatus(NotificationMessage message);

        void CloseShutter(MessageActor sender, BayNumber requestingBay, bool restore);

        void ContinuePositioning(MessageActor sender, BayNumber requestingBay);

        bool FilterNotifications(NotificationMessage notification, MessageActor destination);

        double GetCurrentVerticalPosition();

        double? GetDestinationHeight(Mission moveData);

        double? GetSourceHeight(Mission moveData);

        bool MoveCarousel(int? loadUnitId, MessageActor sender, BayNumber requestingBay);

        void MoveLoadingUnit(HorizontalMovementDirection direction, bool moveToCradle, bool openShutter, bool measure, MessageActor sender, BayNumber requestingBay, int? loadUnitId);

        MessageStatus MoveLoadingUnitStatus(NotificationMessage message);

        bool MoveManualLoadingUnitBack(HorizontalMovementDirection direction, MessageActor sender, BayNumber requestingBay);

        bool MoveManualLoadingUnitForward(HorizontalMovementDirection direction, bool isLoadingUnitOnBoard, MessageActor sender, BayNumber requestingBay);

        void NotifyAssignedMissionOperationChanged(BayNumber bayNumber, int missionId);

        void OpenShutter(MessageActor sender, BayNumber requestingBay, bool restore);

        void PositionElevatorToPosition(double targetHeight, bool closeShutter, bool measure, MessageActor sender, BayNumber requestingBay, bool restore);

        MessageStatus PositionElevatorToPositionStatus(NotificationMessage message);

        MessageStatus ShutterStatus(NotificationMessage message);

        void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        MessageStatus StopOperationStatus(NotificationMessage message);

        void UpdateLastIdealPosition(HorizontalMovementDirection direction, bool isLoadingUnitOnBoard);

        #endregion
    }
}
