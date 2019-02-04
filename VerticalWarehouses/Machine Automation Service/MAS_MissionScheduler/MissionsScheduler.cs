using System;
using Ferretto.VW.MAS_MachineManager;
using Ferretto.Common.Common_Utils;
using System.Collections.Generic;
using MAS_DataLayer;

namespace Ferretto.VW.MAS_MissionScheduler
{
    public class MissionsScheduler : IMissionsScheduler
    {
        #region Fields

        private readonly IMachineManager machineManager;

        private readonly Queue<Mission> missionsQueue = new Queue<Mission>();

        private readonly IWriteLogService writeLogService;

        #endregion Fields

        #region Constructors

        public MissionsScheduler(IMachineManager machineManager, IWriteLogService writeLogService)
        {
            this.machineManager = machineManager;
            this.writeLogService = writeLogService;
        }

        #endregion Constructors

        #region Methods

        public void AddMission(Mission mission)
        {
            throw new NotImplementedException();
        }

        public void DoHoming()
        {
            this.machineManager.DoHoming();
        }

        #endregion Methods
    }
}
