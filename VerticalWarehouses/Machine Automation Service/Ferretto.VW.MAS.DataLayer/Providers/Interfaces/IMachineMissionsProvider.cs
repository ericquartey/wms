using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Data;
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

        List<IMission> GetMissionsByFSMType(FSMType fsmType);

        IEnumerable<IMission> GetMissionsByType(FSMType fsmType, MissionType type);

        bool PauseMachineMission(Guid missionId);

        bool ResumeMachineMission(Guid missionId);

        bool StartMachineMission(Guid missionId, CommandMessage command);

        bool StopMachineMission(Guid missionId, StopRequestReason reason);

        bool TryCreateMachineMission(FSMType fsmType, CommandMessage command, out Guid missionId);

        bool TryCreateWmsMission(FSMType fsmType, MoveLoadingUnitMessageData command, out Guid missionId);

        #endregion
    }
}
