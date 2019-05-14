using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    internal interface IAutomationService
    {
        #region Methods

        Task CompleteMissionAsync(int missionId);

        Task CompleteMissionAsync(int missionId, int quantity);

        Task ExecuteListAsync(int listId);

        Task ExecuteMissionAsync(int missionId);

        Task<Bay> GetBayAsync();

        Task<IEnumerable<ItemList>> GetListsAsync();

        Task<IEnumerable<Mission>> GetMissionsAsync();

        Task InitializeAsync();

        Task<string> LoginAsync(string userName, string password);

        #endregion
    }
}
