using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IMissionsProvider
    {
        #region Methods

        bool StartMachineMission(Guid missionId, CommandMessage command);

        bool StopMachineMission(Guid missionId, StopRequestReason reason);

        bool TryCreateMachineMission(MissionType missionType, out Guid missionId, bool forceClose = false);

        #endregion
    }
}
