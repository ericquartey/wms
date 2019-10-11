using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IMissionsProvider
    {
        #region Methods

        bool StartMachineMission(Guid missionId, CommandMessage command, IFiniteStateMachineData machineData);

        bool StopMachineMission(Guid missionId, StopRequestReason reason);

        bool TryCreateMachineMission(MissionType missionType, CommandMessage command, out Guid missionId);

        #endregion
    }
}
