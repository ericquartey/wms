using System;
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

        public void ExecuteHoming()
        {
            try
            {
                this.eventAggregator.GetEvent<TestHomingEvent>().Publish("Qualcosa\n");
            }
            catch
            {
                throw new Exception();
            }
        }

        #endregion
    }
}
