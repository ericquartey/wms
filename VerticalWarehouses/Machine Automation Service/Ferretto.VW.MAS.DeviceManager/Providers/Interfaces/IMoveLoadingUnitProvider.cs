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

        MessageStatus MoveLoadingUnitToElevatorStatus(NotificationMessage message);

        bool NeedOpenShutter(LoadingUnitLocation positionType);

        bool OpenShutter(LoadingUnitLocation positionType, MessageActor sender, BayNumber requestingBay);

        List<MovementMode> PositionElevatorToPosition(double targetHeight, LoadingUnitLocation sourceType, MessageActor sender, BayNumber requestingBay);

        MessageStatus PositionElevatorToPositionStatus(NotificationMessage message, List<MovementMode> movements);

        MessageStatus ShutterStatus(NotificationMessage message);

        void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        MessageStatus StopOperationStatus(NotificationMessage message);

        #endregion
    }
}
