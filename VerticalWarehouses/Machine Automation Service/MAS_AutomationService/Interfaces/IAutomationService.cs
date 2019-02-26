using System;
using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_AutomationService
{
    public interface IAutomationService
    {
        #region Methods

        Boolean AddMission(Mission mission);

        #endregion
    }
}
