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

        bool AbortMachineMission(Guid fsmId);

        IMission GetMissionById(Guid fsmId);

        List<IMission> GetMissionsByFsmType(FsmType fsmType);

        IEnumerable<IMission> GetMissionsByType(FsmType fsmType, MissionType type);

        bool PauseMachineMission(Guid fsmId);

        bool ResumeMachineMission(Guid fsmId, CommandMessage command);

        bool StartMachineMission(Guid fsmId, CommandMessage command);

        bool StopMachineMission(Guid fsmId, StopRequestReason reason);

        bool TryCreateMachineMission(FsmType fsmType, CommandMessage command, out Guid missionId);

        #endregion
    }
}
