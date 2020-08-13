using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Messages;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces
{
    public interface IMissionMoveBase
    {
        #region Properties

        IEventAggregator EventAggregator { get; }

        Mission Mission { get; set; }

        IServiceProvider ServiceProvider { get; }

        #endregion

        #region Methods

        void DepositUnitEnd(bool restore = false);

        void LoadUnitEnd(bool restore = false);
        void NotifyAssignedMissionChanged(BayNumber bayNumber, int? missionId);
        void OnCommand(CommandMessage command);

        bool OnEnter(CommandMessage command, bool showErrors = true);

        void OnNotification(NotificationMessage message);

        void OnResume(CommandMessage command);

        void OnStop(StopRequestReason reason, bool moveBackward = false);

        void SendMoveNotification(BayNumber targetBay, string description, MessageStatus messageStatus);

        void SendPositionNotification(string description);

        bool UpdateResponseList(MessageType type);

        #endregion
    }
}
