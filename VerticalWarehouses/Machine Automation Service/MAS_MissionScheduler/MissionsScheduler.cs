using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.MAS_MachineManager;

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

        public async Task DoHoming(BroadcastDelegate broadcastDelegate)
        {
            this.machineManager.DoHoming(broadcastDelegate);
        }

        #endregion Methods
    }
}
