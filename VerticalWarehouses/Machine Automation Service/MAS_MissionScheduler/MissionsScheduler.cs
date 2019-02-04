using System;
using Ferretto.VW.MAS_MachineManager;
using Ferretto.Common.Common_Utils;
using System.Collections.Generic;

namespace Ferretto.VW.MAS_MissionScheduler
{
    public class MissionsScheduler : IMissionsScheduler
    {
        #region Fields

        private readonly MachineManager machineManager = Singleton<MachineManager>.UniqueInstance;

        private readonly Queue<Mission> missionsQueue = new Queue<Mission>();

        #endregion Fields

        #region Methods

        public void AddMission(Mission mission)
        {
            throw new NotImplementedException();
        }

        public void DoHoming(BroadcastDelegate broadcastDelegate)
        {
            this.machineManager.DoHoming(broadcastDelegate);
        }

        #endregion Methods
    }
}
