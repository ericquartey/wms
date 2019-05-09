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

        private readonly ICompartmentsDataService compartmentsDataService;

        private readonly IItemListsDataService listsDataService;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionsDataService missionsDataService;

        #endregion

        #region Constructors

        public AutomationProvider(
          IMissionsDataService missionsDataService,
          IMachinesDataService machinesDataService,
          ICompartmentsDataService compartmentsDataService,
          IItemListsDataService listsDataService,
          IBaysDataService baysDataService)
        {
            this.missionsDataService = missionsDataService;
            this.machinesDataService = machinesDataService;
            this.compartmentsDataService = compartmentsDataService;
            this.baysDataService = baysDataService;
            this.listsDataService = listsDataService;
        }

        #endregion

        #region Methods

        public async Task ActivateBayAsync(int bayId)
        {
            await this.baysDataService.ActivateAsync(bayId);
        }

        public async Task<Mission> CompleteLoadingUnitMissionAsync(int missionId)
        {
            return await this.missionsDataService.CompleteLoadingUnitAsync(missionId);
        }

        public async Task<Mission> CompleteMissionAsync(int missionId, int quantity)
        {
            return await this.missionsDataService.CompleteItemAsync(missionId, quantity);
        }

        public async Task ExecuteListAsync(int listId, int areaId, int bayId)
        {
            await this.listsDataService.ExecuteAsync(listId, areaId, bayId);
        }

        public async Task<Mission> ExecuteMissionAsync(int missionId)
        {
            return await this.missionsDataService.ExecuteAsync(missionId);
        }

        public async Task<IEnumerable<Bay>> GetBaysAsync(int machineId)
        {
            var bays = await this.baysDataService.GetAllAsync();

            return bays.Where(b => b.MachineId == machineId).ToArray();
        }

        public async Task<Bay> GetBayAsync(int bayId)
        {
            return await this.baysDataService.GetByIdAsync(bayId);
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

        public async Task<int?> GetLoadingUnitIdFromMissionAsync(Mission mission)
        {
            if (mission == null)
            {
                throw new System.ArgumentNullException(nameof(mission));
            }

            if (mission.CompartmentId.HasValue)
            {
                var compartment = await this.compartmentsDataService.GetByIdAsync(mission.CompartmentId.Value);
                return compartment.LoadingUnitId;
            }

            return null;
        }

        public async Task<IEnumerable<Machine>> GetMachinesAsync()
        {
            return await this.machinesDataService.GetAllAsync(null, null, null, null, null);
        }

        public async Task<IEnumerable<Mission>> GetMissionsAsync()
        {
            return await this.missionsDataService.GetAllAsync(null, null, null, null, null);
        }

        #endregion
    }
}
