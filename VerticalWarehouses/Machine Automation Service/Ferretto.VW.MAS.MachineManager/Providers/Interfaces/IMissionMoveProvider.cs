using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.MachineManager.Providers.Interfaces
{
    public interface IMissionMoveProvider
    {
        #region Methods

        void OnCommand(CommandMessage message, IServiceProvider serviceProvider);

        void OnNotification(NotificationMessage message, IServiceProvider serviceProvider);

        bool ResumeMission(Guid missionId, CommandMessage command, IServiceProvider serviceProvider);

        bool StartMission(Mission mission, CommandMessage command, IServiceProvider serviceProvider);

        bool StopMission(Guid missionId, StopRequestReason stopRequest, IServiceProvider serviceProvider);

        bool TryCreateMachineMission(CommandMessage command, IServiceProvider serviceProvider, out Mission mission);

        #endregion
    }
}
