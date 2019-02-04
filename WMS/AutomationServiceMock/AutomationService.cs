using System;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.AutomationServiceMock
{
    public class AutomationService : IAutomationService
    {
        #region Fields

        private readonly IBaysSchedulerService baysSchedulerService;

        private readonly ILogger logger;

        private readonly IMissionsSchedulerService missionsSchedulerService;

        private readonly Random random = new Random();

        private readonly IWakeupHubClient wakeupHubClient;

        #endregion

        #region Constructors

        public AutomationService(
            ILogger<AutomationService> logger,
            IWakeupHubClient wakeupHubClient,
            IMissionsSchedulerService missionsSchedulerService,
            IBaysSchedulerService baysSchedulerService)
        {
            this.logger = logger;
            this.missionsSchedulerService = missionsSchedulerService;
            this.baysSchedulerService = baysSchedulerService;
            this.wakeupHubClient = wakeupHubClient;

            this.wakeupHubClient.WakeupReceived += this.WakeupReceived;
            this.wakeupHubClient.NewMissionReceived += this.NewMissionReceived;
        }

        #endregion

        #region Methods

        public async Task InitializeAsync()
        {
            await this.wakeupHubClient.ConnectAsync();

            this.logger.LogInformation("Automation service initialized");
        }

        public async Task NotifyUserLoginAsync(int bayId)
        {
            this.logger.LogInformation($"Notifying the scheduler that bay '{bayId}' is operational.");
            await this.baysSchedulerService.MarkAsOperationalAsync(bayId);
        }

        private async Task ExecuteMissionAsync(Mission mission)
        {
            this.logger.LogInformation($"Executing mission '{mission.Type}' on item {mission.ItemId}, quantity {mission.Quantity}");

            // simulate mission execution
            await Task.Delay(1000);

            var success = this.random.NextDouble() > 0.5;
            if (success)
            {
                this.logger.LogInformation($"Mission completed.");
                await this.missionsSchedulerService.CompleteAsync(new Mission());
            }
            else
            {
                this.logger.LogWarning($"Mission failed.");
                await this.missionsSchedulerService.AbortAsync(new Mission());
            }
        }

        private async void NewMissionReceived(object sender, MissionEventArgs e)
        {
            this.logger.LogInformation($"New mission received from Scheduler id={e.Mission.Id}.");

            // TODO CHECK
            // Cannot convert from "Ferretto.WMS.Scheduler.Core.Mission" => "Ferretto.WMS.Scheduler.WebAPI.Contracts.Mission"
            await this.ExecuteMissionAsync(e.Mission);
        }

        private void WakeupReceived(object sender, WakeUpEventArgs e)
        {
            this.logger.LogInformation($"Wakeup from Scheduler received.");
        }

        #endregion
    }
}
