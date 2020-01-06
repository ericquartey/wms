using System;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.MachineManager.Providers.Interfaces
{
    public interface IMissionMoveProvider
    {
        #region Methods

        bool Start(int missionId, CommandMessage commandMessage);

        bool TryCreateMachineMission(CommandMessage command, out int? missionId);

        #endregion
    }
}
