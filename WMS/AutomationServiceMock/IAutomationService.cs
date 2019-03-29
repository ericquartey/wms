using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.WMS.AutomationServiceMock
{
    internal interface IAutomationService
    {
        #region Methods

        Task CompleteMissionAsync(int missionId, int quantity);

        Task ExecuteListAsync(int listId);

        Task ExecuteMissionAsync(int missionId);

        Task<Bay> GetBayAsync();

        Task<IEnumerable<Mission>> GetMissionsAsync();

        Task InitializeAsync();

        Task NotifyUserLoginAsync();

        #endregion
    }
}
