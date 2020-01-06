using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces
{
    public interface IMissionMoveBase
    {
        #region Properties

        Mission Mission { get; set; }

        IServiceProvider ServiceProvider { get; }

        #endregion

        #region Methods

        void OnCommand(CommandMessage command);

        bool OnEnter(CommandMessage command);

        void OnNotification(NotificationMessage message);

        #endregion
    }
}
