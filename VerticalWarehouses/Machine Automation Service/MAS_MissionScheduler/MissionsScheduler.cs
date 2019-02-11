using System;
using System.Collections.Generic;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_MachineManager;
using Prism.Events;

namespace Ferretto.VW.MAS_MissionScheduler
{
    public class MissionsScheduler : IMissionsScheduler
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineManager machineManager;

        private readonly Queue<Mission> missionsQueue = new Queue<Mission>();

        private readonly IWriteLogService writeLogService;

        #endregion

        #region Constructors

        public MissionsScheduler(IMachineManager machineManager, IWriteLogService writeLogService, IEventAggregator eventAggregator)
        {
            this.machineManager = machineManager;
            this.writeLogService = writeLogService;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public bool AddMission(Mission mission)
        {
            if (mission == null) throw new ArgumentNullException("Mission is null, cannot add a null item to the Mission Queue.\n");
            this.missionsQueue.Enqueue(mission);
            return true;
        }

        #endregion
    }
}
