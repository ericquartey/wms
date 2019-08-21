using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.AutomationService
{
    public interface IAutomationService
    {
        #region Methods

        bool AddMission(Mission mission);

        #endregion
    }
}
