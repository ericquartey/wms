using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ILoadingUnitMovementProvider
    {
        #region Methods

        bool FilterNotifications(NotificationMessage notification, MessageActor destination);

        void PositionElevatorToSource(IMoveLoadingUnitMessageData messageData, MessageActor sender, BayNumber requestingBay);

        MessageStatus PositionElevatorToSourceStatus(NotificationMessage message);

        void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        MessageStatus StopOperationStatus(NotificationMessage message);

        #endregion
    }
}
