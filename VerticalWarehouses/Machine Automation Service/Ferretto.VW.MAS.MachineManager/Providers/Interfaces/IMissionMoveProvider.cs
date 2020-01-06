using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.MachineManager.Providers.Interfaces
{
    public interface IMissionMoveProvider
    {
        #region Methods

        void OnCommand(CommandMessage message, IServiceProvider serviceProvider);

        void OnNotification(NotificationMessage message, IServiceProvider serviceProvider);

        bool Start(int missionId, CommandMessage commandMessage, IServiceProvider serviceProvider);

        bool TryCreateMachineMission(CommandMessage command, IServiceProvider serviceProvider, out int? missionId);

        #endregion
    }
}
