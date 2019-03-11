using System;
using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_AutomationService
{
    public interface IAutomationService
    {
        #region Methods

        bool AddMission(Mission mission);

        #endregion
    }
}
