using Ferretto.VW.Common_Utils;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS_AutomationService
{
    public interface IAutomationService
    {
        #region Methods

        bool AddMission(Mission mission);

        #endregion
    }
}
