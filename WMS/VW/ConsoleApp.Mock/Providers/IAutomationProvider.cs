using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    public interface IAutomationProvider
    {
        #region Methods

        Task ActivateBayAsync(int bayId);

        Task<MissionExecution> CompleteLoadingUnitMissionAsync(int missionId);

        Task<MissionExecution> CompleteMissionAsync(int missionId, int quantity);

        Task ExecuteListAsync(int listId, int areaId, int bayId);

        Task<MissionExecution> ExecuteMissionAsync(int missionId);

        Task<Bay> GetBayAsync(int bayId);

        Task<IEnumerable<Bay>> GetBaysAsync(int machineId);

        Task<IEnumerable<ItemList>> GetListsAsync();

        Task<int?> GetLoadingUnitIdFromMissionAsync(MissionExecution mission);

        Task<IEnumerable<Machine>> GetMachinesAsync();

        Task<IEnumerable<Mission>> GetMissionsAsync();

        #endregion
    }
}
