using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Ferretto.WMS.AutomationServiceMock
{
    public class AutomationService : IAutomationService
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IBaysDataService baysDataService;

        private readonly IConfiguration configuration;

        private readonly IItemListsDataService listsDataService;

        private readonly IMissionsDataService missionsDataService;

        private readonly ISchedulerHubClient schedulerHubClient;

        #endregion

        #region Constructors

        public AutomationService(
            ISchedulerHubClient wakeupHubClient,
            IConfiguration configuration,
            IMissionsDataService missionsDataService,
            IItemListsDataService listsDataService,
            IBaysDataService baysDataService,
            IAuthenticationService authenticationService)
        {
            this.missionsDataService = missionsDataService;
            this.baysDataService = baysDataService;
            this.listsDataService = listsDataService;
            this.schedulerHubClient = wakeupHubClient;
            this.authenticationService = authenticationService;
            this.configuration = configuration;

            this.schedulerHubClient.ConnectAsync();
        }

        #endregion

        #region Methods

        public async Task CompleteMissionAsync(int missionId, int quantity)
        {
            try
            {
                await this.missionsDataService.CompleteItemAsync(missionId, quantity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to complete mission with id={missionId}: {ex.Message}");
            }
        }

        public async Task CompleteMissionAsync(int missionId)
        {
            try
            {
                await this.missionsDataService.CompleteLoadingUnitAsync(missionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to complete mission with id={missionId}: {ex.Message}");
            }
        }

        public async Task ExecuteListAsync(int listId)
        {
            try
            {
                var bay = await this.GetBayAsync();

                await this.listsDataService.ExecuteAsync(listId, bay.AreaId, bay.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to execute list with id={listId}: {ex.Message}");
            }
        }

        public async Task ExecuteMissionAsync(int missionId)
        {
            try
            {
                await this.missionsDataService.ExecuteAsync(missionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to execute mission with id={missionId}: {ex.Message}");
            }
        }

        public async Task<Bay> GetBayAsync() => await this.baysDataService.GetByIdAsync(
                                    this.configuration.GetValue<int>("Warehouse:Bay:Id"));

        public async Task<IEnumerable<ItemList>> GetListsAsync()
        {
            try
            {
                return await this.listsDataService.GetAllAsync(
                    null,
                    null,
                    "([Status] == 'Waiting' || [Status] == 'Executing' || [Status] == 'Completed' || [Status] == 'Incomplete')",
                    "Priority Descending",
                    null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to display lists: {ex.Message}");
            }

            return new List<ItemList>();
        }

        public async Task<IEnumerable<Mission>> GetMissionsAsync()
        {
            try
            {
                return await this.missionsDataService.GetAllAsync(null, null, null, null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to retrieve the list of missions: {ex.Message}");
                return null;
            }
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("Connecting to service hub ...");

            await this.schedulerHubClient.ConnectAsync();

            Console.WriteLine("Automation service initialized.");
        }

        public async Task LoginAsync(string userName, string password)
        {
            await this.authenticationService.LoginAsync(userName, password);

            var bay = await this.GetBayAsync();

            Console.WriteLine($"Notifying the scheduler that bay '{bay.Description}' is operational.");

            await this.baysDataService.ActivateAsync(bay.Id);
        }

        #endregion
    }
}
