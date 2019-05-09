using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    public interface IAutomationProvider
    {
        #region Methods

        Task ActivateBayAsync(int bayId);

        Task<Mission> CompleteLoadingUnitMissionAsync(int missionId);

        Task<Mission> CompleteMissionAsync(int missionId, int quantity);

        Task ExecuteListAsync(int listId, int areaId, int bayId);

        Task<Mission> ExecuteMissionAsync(int missionId);

        Task<Bay> GetBayAsync(int bayId);

        Task<IEnumerable<ItemList>> GetListsAsync();

        Task<int?> GetLoadingUnitIdFromMissionAsync(Mission mission);

        Task<IEnumerable<Machine>> GetMachinesAsync();

        Task<IEnumerable<Mission>> GetMissionsAsync();

        Task<IEnumerable<Bay>> GetBaysAsync(int machineId);

        #endregion
    }
}
