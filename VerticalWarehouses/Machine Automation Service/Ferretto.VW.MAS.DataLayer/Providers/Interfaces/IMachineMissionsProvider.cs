using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Missions;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IMachineMissionsProvider
    {
        #region Methods

        bool AbortMachineMission(Guid missionId);

        IMission GetMissionById(Guid missionId);

        List<IMission> GetMissionsByType(MissionType type);

        bool PauseMachineMission(Guid missionId);

        bool ResumeMachineMission(Guid missionId);

        bool StartMachineMission(Guid missionId, CommandMessage command);

        bool StopMachineMission(Guid missionId, StopRequestReason reason);

        bool TryCreateMachineMission(MissionType missionType, CommandMessage command, out Guid missionId);

        #endregion
    }
}
