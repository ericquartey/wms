using System;
using System.Collections.Generic;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_MachineManager;

namespace Ferretto.VW.MAS_MissionScheduler
{
    public class MissionsScheduler : IMissionsScheduler
    {
        #region Fields

        private readonly IMachineManager machineManager;

        private readonly Queue<Mission> missionsQueue = new Queue<Mission>();

        private readonly IWriteLogService writeLogService;

        #endregion

        #region Constructors

        public MissionsScheduler(IMachineManager machineManager, IWriteLogService writeLogService)
        {
            this.machineManager = machineManager;
            this.writeLogService = writeLogService;
        }

        #endregion

        #region Methods

        public void AddMission(Mission mission)
        {
            throw new NotImplementedException();
        }

        public void DoHoming()
        {
            this.machineManager.DoHoming();
        }

        public void Test01()
        {
            this.machineManager.SetParam(42);
        }

        public void Test02()
        {
            this.machineManager.GetParam();
        }

        #endregion
    }
}
