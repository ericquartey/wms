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

        void OnCommand(CommandMessage command);

        bool OnEnter(CommandMessage command);

        void OnNotification(NotificationMessage message);

        void OnResume(CommandMessage command);

        void OnStop(StopRequestReason reason);

        void UpdateResponseList(MessageType type);

        #endregion
    }
}
