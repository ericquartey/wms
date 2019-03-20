using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;

namespace Ferretto.WMS.AutomationServiceMock
{
    public class AutomationService : IAutomationService
    {
        #region Fields

        private readonly IBaysDataService baysDataService;

        private readonly IMissionsDataService missionsDataService;

        private readonly Random random = new Random();

        private readonly IWakeupHubClient wakeupHubClient;

        #endregion

        #region Constructors

        public AutomationService(
            IWakeupHubClient wakeupHubClient,
            IMissionsDataService missionsDataService,
            IBaysDataService baysDataService)
        {
            this.missionsDataService = missionsDataService;
            this.baysDataService = baysDataService;
            this.wakeupHubClient = wakeupHubClient;

            this.wakeupHubClient.WakeupReceived += WakeupReceived;
            this.wakeupHubClient.NewMissionReceived += this.NewMissionReceived;
        }

        #endregion

        #region Methods

        public async Task CompleteMissionAsync(int missionId)
        {
            try
            {
                await this.missionsDataService.CompleteAsync(missionId);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Unable to complete mission with id={missionId}: {ex.Message}");
            }
        }

        public async Task ExecuteMissionAsync(int missionId)
        {
            try
            {
                await this.missionsDataService.ExecuteAsync(missionId);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Unable to execute mission with id={missionId}: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Mission>> GetMissionsAsync()
        {
            try
            {
                return await this.missionsDataService.GetAllAsync(null, null, null, null, null);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Unable to retrieve the list of missions: {ex.Message}");
                return null;
            }
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("Connecting to service hub ...");

            await this.wakeupHubClient.ConnectAsync();

            Console.WriteLine("Automation service initialized.");
        }

        public async Task NotifyUserLoginAsync(int bayId)
        {
            Console.WriteLine($"Notifying the scheduler that bay '{bayId}' is operational.");
            await this.baysDataService.ActivateAsync(bayId);
        }

        private static void WakeupReceived(object sender, WakeUpEventArgs e)
        {
            Console.WriteLine($"Wakeup from Scheduler received.");
        }

        private async Task ExecuteMissionAsync(Mission mission)
        {
            Console.WriteLine($"Executing mission '{mission.Type}' on item {mission.ItemId}, quantity {mission.Quantity}");

            // simulate mission execution
            await Task.Delay(1000);

            var success = this.random.NextDouble() > 0.5;
            if (success)
            {
                Console.WriteLine($"Mission completed.");
                await this.missionsDataService.CompleteAsync(mission.Id);
            }
            else
            {
                Console.WriteLine($"Mission failed.");
                await this.missionsDataService.AbortAsync(mission.Id);
            }
        }

        private async void NewMissionReceived(object sender, MissionEventArgs e)
        {
            Console.WriteLine($"New mission received from Scheduler id={e.Mission.Id}.");

            // TODO CHECK
            // Cannot convert from "Ferretto.WMS.Scheduler.Core.Mission" => "Ferretto.WMS.Scheduler.WebAPI.Contracts.Mission"
            await this.ExecuteMissionAsync(e.Mission);
        }

        #endregion
    }
}
