using System;
using Ferretto.VW.MAS_MachineManager;
using Ferretto.Common.Common_Utils;
using System.Collections.Generic;
using MAS_DataLayer;
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

            this.eventAggregator.GetEvent<TestHomingEvent>().Subscribe(this.HandleHoming);
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

        private void HandleHoming(string s)
        {
            this.DoHoming();
        }

        #endregion
    }
}
