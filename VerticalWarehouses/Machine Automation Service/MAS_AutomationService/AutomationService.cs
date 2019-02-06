using System;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.MAS_MissionScheduler;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService
{
    public class AutomationService : IAutomationService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMissionsScheduler missionScheduler;

        #endregion

        #region Constructors

        public AutomationService(IMissionsScheduler missionScheduler, IEventAggregator eventAggregator)
        {
            this.missionScheduler = missionScheduler;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public bool AddMission(Mission mission)
        {
            if (mission == null) throw new ArgumentNullException();
            this.missionScheduler.AddMission(mission);
            return true;
        }

        #endregion
    }
}
