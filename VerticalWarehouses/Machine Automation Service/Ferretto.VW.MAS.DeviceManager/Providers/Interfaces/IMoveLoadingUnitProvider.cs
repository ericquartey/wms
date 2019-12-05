using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ILoadingUnitMovementProvider
    {
        #region Methods

        void CloseShutter(MessageActor sender, BayNumber requestingBay);

        void ContinuePositioning(MessageActor sender, BayNumber requestingBay);

        bool FilterNotifications(NotificationMessage notification, MessageActor destination);

        double? GetDestinationHeight(Mission moveData);

        double? GetSourceHeight(Mission moveData);

        void MoveLoadingUnit(HorizontalMovementDirection direction, bool moveToCradle, bool openShutter, bool measure, MessageActor sender, BayNumber requestingBay, int? loadUnitId);

        MessageStatus MoveLoadingUnitStatus(NotificationMessage message);

        void NotifyAssignedMissionOperationChanged(BayNumber bayNumber, int missionId);

        void PositionElevatorToPosition(double targetHeight, bool closeShutter, bool measure, MessageActor sender, BayNumber requestingBay);

        MessageStatus PositionElevatorToPositionStatus(NotificationMessage message);

        MessageStatus ShutterStatus(NotificationMessage message);

        void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        MessageStatus StopOperationStatus(NotificationMessage message);

        #endregion
    }
}
