using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    public interface IAutomationProvider
    {
        #region Methods

        Task<MissionOperation> AbortOperationAsync(int operationId);

        Task ActivateBayAsync(int bayId);

        Task<Mission> CompleteLoadingUnitMissionAsync(int missionId);

        Task<MissionOperation> CompleteOperationAsync(int operationId, int quantity);

        Task ExecuteListAsync(int listId, int areaId, int bayId);

        Task<MissionOperation> ExecuteOperationAsync(int operationId);

        Task<Bay> GetBayAsync(int bayId);

        Task<IEnumerable<Bay>> GetBaysAsync(int machineId);

        Task<IEnumerable<ItemList>> GetListsAsync();

        Task<IEnumerable<Machine>> GetMachinesAsync();

        Task<MissionInfo> GetMissionByIdAsync(int missionId);

        Task<IEnumerable<MissionInfo>> GetMissionsAsync(int machineId);

        #endregion
    }
}
