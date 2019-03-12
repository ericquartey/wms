using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.AutomationServiceMock
{
    internal interface IAutomationService
    {
        #region Methods

        Task CompleteMission(int missionId);

        Task ExecuteMission(int missionId);

        Task<IEnumerable<Mission>> GetMissions();

        Task InitializeAsync();

        Task NotifyUserLoginAsync(int bayId);

        #endregion
    }
}
