using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ILoadingUnitMovementProvider
    {
        #region Methods

        bool FilterNotifications(NotificationMessage notification, MessageActor destination);

        double GetDestinationHeight(IMoveLoadingUnitMessageData messageData);

        double GetSourceHeight(IMoveLoadingUnitMessageData messageData);

        void MoveLoadingUnitToDestination(int? loadingUnitId, MessageActor sender, BayNumber requestingBay);

        MessageStatus MoveLoadingUnitToDestinationStatus(NotificationMessage message);

        void MoveLoadingUnitToElevator(int? loadingUnitId, HorizontalMovementDirection direction, MessageActor sender, BayNumber requestingBay);

        MessageStatus MoveLoadingUnitToElevatorStatus(NotificationMessage message);

        List<MovementMode> PositionElevatorToPosition(double targetHeight, LoadingUnitLocation positionType, MessageActor sender, BayNumber requestingBay);

        MessageStatus PositionElevatorToPositionStatus(NotificationMessage message, List<MovementMode> movements);

        void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        MessageStatus StopOperationStatus(NotificationMessage message);

        #endregion
    }
}
