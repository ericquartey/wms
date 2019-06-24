using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    public class AutomationProvider : IAutomationProvider
    {
        #region Fields

        private readonly IBaysDataService baysDataService;

        private readonly IItemListsDataService listsDataService;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionOperationsDataService missionOperationsDataService;

        private readonly IMissionsDataService missionsDataService;

        #endregion

        #region Constructors

        public AutomationProvider(
          IMissionsDataService missionsDataService,
          IMissionOperationsDataService missionOperationsDataService,
          IMachinesDataService machinesDataService,
          IItemListsDataService listsDataService,
          IBaysDataService baysDataService)
        {
            this.missionsDataService = missionsDataService;
            this.missionOperationsDataService = missionOperationsDataService;
            this.machinesDataService = machinesDataService;
            this.baysDataService = baysDataService;
            this.listsDataService = listsDataService;
        }

        #endregion

        #region Methods

        public async Task<MissionOperation> AbortOperationAsync(int operationId)
        {
            return await this.missionOperationsDataService.AbortAsync(operationId);
        }

        public async Task ActivateBayAsync(int bayId)
        {
            await this.baysDataService.ActivateAsync(bayId);
        }

        public async Task<MissionInfo> CompleteLoadingUnitMissionAsync(int missionId)
        {
            return await this.missionsDataService.CompleteLoadingUnitAsync(missionId);
        }

        public async Task<MissionOperation> CompleteOperationAsync(int operationId, int quantity)
        {
            return await this.missionOperationsDataService.CompleteItemAsync(operationId, quantity);
        }

        public async Task ExecuteListAsync(int listId, int areaId, int bayId)
        {
            await this.listsDataService.ExecuteAsync(listId, areaId, bayId);
        }

        public async Task<MissionOperation> ExecuteOperationAsync(int operationId)
        {
            return await this.missionOperationsDataService.ExecuteAsync(operationId);
        }

        public async Task<Bay> GetBayAsync(int bayId)
        {
            return await this.baysDataService.GetByIdAsync(bayId);
        }

        public async Task<IEnumerable<Bay>> GetBaysAsync(int machineId)
        {
            var bays = await this.baysDataService.GetAllAsync();

            return bays.Where(b => b.MachineId == machineId).ToArray();
        }

        public async Task<IEnumerable<ItemList>> GetListsAsync()
        {
            return await this.listsDataService.GetAllAsync(
                    null,
                    null,
                    "([Status] == 'Waiting' || [Status] == 'Executing' || [Status] == 'Completed' || [Status] == 'Incomplete')",
                    "Priority Descending",
                    null);
        }

        public async Task<IEnumerable<Machine>> GetMachinesAsync()
        {
            return await this.machinesDataService.GetAllAsync();
        }

        public async Task<MissionInfo> GetMissionByIdAsync(int missionId)
        {
            return await this.missionsDataService.GetByIdAsync(missionId);
        }

        public async Task<IEnumerable<Mission>> GetMissionsAsync(int machineId)
        {
            return await this.machinesDataService.GetMissionsByIdAsync(machineId);
        }

        #endregion
    }
}
