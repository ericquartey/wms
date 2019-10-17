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

        void MoveLoadingUnit(HorizontalMovementDirection direction, bool moveToCradle, MessageActor sender, BayNumber requestingBay);

        MessageStatus MoveLoadingUnitStatus(NotificationMessage message);

        List<MovementMode> PositionElevatorToPosition(double targetHeight, LoadingUnitLocation positionType, MessageActor sender, BayNumber requestingBay);

        MessageStatus PositionElevatorToPositionStatus(NotificationMessage message, List<MovementMode> movements);

        void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        MessageStatus StopOperationStatus(NotificationMessage message);

        #endregion
    }
}
