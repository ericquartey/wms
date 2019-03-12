using System;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.AutomationServiceMock
{
    internal static class Program
    {
        #region Enums

        private enum UserSelection
        {
            Login,

            ListMissions,

            CompleteMission,

            ExecuteMission,

            Exit
        }

        #endregion

        #region Methods

        private static IAutomationService BuildConfiguration(string[] args)
        {
            var configuration = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json", optional: false)
             .AddCommandLine(args)
             .Build();

            var serviceProvider = ConfigureServices(configuration);

            var automationService = serviceProvider.GetService<IAutomationService>();

            return automationService;
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            services.AddSingleton(typeof(IConfiguration), configuration);
            services.AddLogging(b => b.AddConfiguration(configuration).AddConsole());

            services.AddSingleton<IAutomationService, AutomationService>();

            var schedulerUrl = configuration["Scheduler:Url"];
            var wakeUpPath = configuration["Hubs:WakeUp"];
            services.AddTransient<IWakeupHubClient>(s => new WakeupHubClient(new Uri(schedulerUrl), wakeUpPath));

            services.AddWebApiServices(new Uri(schedulerUrl));

            return services.BuildServiceProvider();
        }

        private static async Task<bool> ExecuteOperationAsync(IAutomationService automationService, UserSelection selection)
        {
            var exitRequested = false;
            switch (selection)
            {
                case UserSelection.CompleteMission:
                    var completeMissionId = GetMissionId();
                    if (completeMissionId > 0)
                    {
                        await automationService.CompleteMissionAsync(completeMissionId);
                        Console.WriteLine($"Request sent.");
                    }

                    break;

                case UserSelection.ExecuteMission:
                    var executeMissionId = GetMissionId();
                    if (executeMissionId > 0)
                    {
                        await automationService.ExecuteMissionAsync(executeMissionId);
                        Console.WriteLine($"Request sent.");
                    }

                    break;

                case UserSelection.ListMissions:
                    Console.WriteLine("Available missions:");
                    Console.WriteLine($"{nameof(Mission.Id), 3}, {nameof(Mission.Status), 10}, {nameof(Mission.ItemDescription), 20}, {nameof(Mission.RequiredQuantity), 3}");

                    var missions = await automationService.GetMissionsAsync();
                    foreach (var mission in missions)
                    {
                        Console.WriteLine($"{mission.Id, 3}, {mission.Status, 10}, {mission.ItemDescription,  20}, {mission.RequiredQuantity, 3}");
                    }

                    break;

                case UserSelection.Exit:
                    exitRequested = true;
                    break;

                case UserSelection.Login:
                    // notify the scheduler that a user logged in
                    // to the PanelPC associated to the specified bay
                    await automationService.NotifyUserLoginAsync(bayId: 1);
                    Console.WriteLine($"Request sent.");
                    break;

                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }

            return exitRequested;
        }

        private static int GetMissionId()
        {
            Console.WriteLine("Insert mission id: ");
            Console.Write("> ");
            var missionIdString = Console.ReadLine();

            if (int.TryParse(missionIdString, out var missionId))
            {
                return missionId;
            }
            else
            {
                Console.WriteLine("Unable to parse mission id");
            }

            return -1;
        }

        private static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("VertiMAG Panel PC");
                Console.WriteLine("-----------------");
                Console.WriteLine();

                var automationService = BuildConfiguration(args);

                var exitRequested = false;
                while (exitRequested == false)
                {
                    Console.WriteLine("Select option: ");
                    Console.WriteLine($"{(int)UserSelection.Login} - Login to PPC");
                    Console.WriteLine($"{(int)UserSelection.ListMissions} - List missions");
                    Console.WriteLine($"{(int)UserSelection.CompleteMission} - Complete mission");
                    Console.WriteLine($"{(int)UserSelection.ExecuteMission} - Execute mission");
                    Console.WriteLine($"{(int)UserSelection.Exit} - Exit");
                    Console.Write("> ");

                    var selectionString = Console.ReadLine();
                    if (int.TryParse(selectionString, out var selection) == false)
                    {
                        Console.WriteLine("Unable to parse selection.");
                    }
                    else
                    {
                        exitRequested = await ExecuteOperationAsync(automationService, (UserSelection)selection);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine();
            }
            finally
            {
                Console.WriteLine("Press <ENTER> to terminate the automation service.");
                Console.ReadLine();
            }
        }

        #endregion
    }
}
