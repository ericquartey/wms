using System;
using Ferretto.VW.MAS_MissionScheduler;

namespace Ferretto.VW.MAS_AutomationService
{
    public class AutomationService : IAutomationService
    {
        #region Fields

        private readonly IMissionsScheduler missionScheduler;

        #endregion Fields

        #region Constructors

        public AutomationService(IMissionsScheduler missionScheduler)
        {
            this.missionScheduler = missionScheduler;
        }

        #endregion Constructors

        #region Methods

        public void ExecuteHoming()
        {
            try
            {
                this.missionScheduler.DoHoming();
            }
            catch
            {
                throw new Exception();
            }
        }

        #endregion Methods
    }
}
