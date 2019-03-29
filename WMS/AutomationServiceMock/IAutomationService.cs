using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.WMS.AutomationServiceMock
{
    internal interface IAutomationService
    {
        #region Methods

        Task CompleteMissionAsync(int missionId, int quantity);

        Task ExecuteMissionAsync(int missionId);

        Task<IEnumerable<Mission>> GetMissionsAsync();

        Task InitializeAsync();

        Task NotifyUserLoginAsync(int bayId);

        #endregion
    }
}
