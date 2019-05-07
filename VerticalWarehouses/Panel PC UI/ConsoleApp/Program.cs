using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.AutomationServiceMock
{
    internal static class Program
    {
        #region Fields

        private const string DefaultApplicationSettingsFile = "appsettings.json";

        private const string NetcoreEnvironmentEnvVariable = "ASPNETCORE_ENVIRONMENT";

        private const string ParametrizedApplicationSettingsFile = "appsettings.{0}.json";

        #endregion

        #region Enums

        private enum UserSelection
        {
            Login = 1,

            DisplayMissions = 2,

            ExecuteMission = 3,

            CompleteMission = 4,

            DisplayLists = 5,

            ExecuteList = 6,

            Exit = 7
        }

        #endregion

        #region Methods

        private static IAutomationService BuildConfiguration(string[] args)
        {
            var applicationSettingsFile = GetSettingFileFromEnvironment();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(DefaultApplicationSettingsFile, false, false)
                .AddJsonFile(applicationSettingsFile, true, false)
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

            var schedulerUrl = new Uri(configuration["Scheduler:Url"]);
            var hubPath = configuration["Hubs:Scheduler"];

            services
                .AddWebApiServices(schedulerUrl)
                .AddSchedulerHub(new Uri(schedulerUrl, hubPath));

            return services.BuildServiceProvider();
        }

        private static async Task<bool> ExecuteOperationAsync(IAutomationService automationService, UserSelection selection)
        {
            var exitRequested = false;
            switch (selection)
            {
                case UserSelection.CompleteMission:
                    var completeMissionId = GetMissionId();
                    var quantity = GetQuantity();
                    if (completeMissionId >= 0)
                    {
                        if (quantity > 0)
                        {
                            await automationService.CompleteMissionAsync(completeMissionId, quantity);
                        }
                        else
                        {
                            await automationService.CompleteMissionAsync(completeMissionId);
                        }

                        Console.WriteLine($"Request sent.");
                    }

                    break;

                case UserSelection.ExecuteMission:
                    var executeMissionId = GetMissionId();
                    if (executeMissionId >= 0)
                    {
                        await automationService.ExecuteMissionAsync(executeMissionId);
                        Console.WriteLine($"Mission execution request sent.");
                    }

                    break;

                case UserSelection.ExecuteList:
                    var executeListId = GetListId();
                    if (executeListId >= 0)
                    {
                        await automationService.ExecuteListAsync(executeListId);
                        Console.WriteLine($"List execution request sent.");
                    }

                    break;

                case UserSelection.DisplayMissions:

                    var missions = await automationService.GetMissionsAsync();

                    PrintMissionsTable(missions);

                    break;

                case UserSelection.DisplayLists:

                    var lists = await automationService.GetListsAsync();

                    PrintListsTable(lists);

                    break;

                case UserSelection.Exit:
                    exitRequested = true;
                    break;

                case UserSelection.Login:
                    // notify the scheduler that a user logged in
                    // to the PanelPC associated to the specified bay
                    await automationService.NotifyUserLoginAsync();
                    Console.WriteLine($"Request sent.");
                    break;

                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }

            return exitRequested;
        }

        private static int GetListId()
        {
            Console.Write("Insert list id: ");
            var listIdString = Console.ReadLine();

            if (int.TryParse(listIdString, out var listId))
            {
                return listId;
            }
            else
            {
                Console.WriteLine("Unable to parse list id.");
            }

            return -1;
        }

        private static int GetMissionId()
        {
            Console.Write("Insert mission id: ");
            var missionIdString = Console.ReadLine();

            if (int.TryParse(missionIdString, out var missionId))
            {
                return missionId;
            }
            else
            {
                Console.WriteLine("Unable to parse mission id.");
            }

            return -1;
        }

        private static int GetQuantity()
        {
            Console.Write("Insert quantity: ");
            var quantityString = Console.ReadLine();

            if (int.TryParse(quantityString, out var quantity))
            {
                return quantity;
            }
            else
            {
                Console.WriteLine("Unable to parse mission quantity.");
            }

            return -1;
        }

        private static string GetSettingFileFromEnvironment()
        {
            var netcoreEnvironment = System.Environment.GetEnvironmentVariable(NetcoreEnvironmentEnvVariable);

            return string.Format(ParametrizedApplicationSettingsFile, netcoreEnvironment);
        }

        private static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("VertiMAG Panel PC");
                Console.WriteLine("-----------------");
                Console.WriteLine("Press <ENTER> to boot.");
                Console.Write(">");
                Console.ReadLine();

                var automationService = BuildConfiguration(args);
                var bay = await automationService.GetBayAsync();

                Console.WriteLine($"Serving bay '{bay.Description}'");
                Console.WriteLine("-----------------");

                var exitRequested = false;
                while (exitRequested == false)
                {
                    Console.WriteLine();
                    Console.WriteLine("Select option: ");
                    Console.WriteLine($"{(int)UserSelection.Login} - Login to PPC");
                    Console.WriteLine($"{(int)UserSelection.DisplayMissions} - Display missions");
                    Console.WriteLine($"{(int)UserSelection.ExecuteMission} - Execute mission");
                    Console.WriteLine($"{(int)UserSelection.CompleteMission} - Complete mission");
                    Console.WriteLine($"{(int)UserSelection.DisplayLists} - Display Lists");
                    Console.WriteLine($"{(int)UserSelection.ExecuteList} - Execute List");
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
            catch (System.Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine();
            }
            finally
            {
                Console.WriteLine("Press <ENTER> to shut down the Panel PC.");
                Console.ReadLine();
            }
        }

        private static void PrintListsTable(IEnumerable<ItemList> lists)
        {
            if (!lists.Any())
            {
                Console.WriteLine("No lists are available.");

                return;
            }

            Console.WriteLine("Lists (by priority):");

            Console.WriteLine(
                $"| {nameof(ItemList.Priority), 8} " +
                $"| {nameof(ItemList.Id), 3} " +
                $"| {nameof(ItemList.Status), -10} " +
                $"| Quantities |");

            Console.WriteLine($"|----------|-----|------------|");

            foreach (var list in lists)
            {
                PrintListTableRow(list);
            }

            Console.WriteLine($"|__________|_____|____________|");
        }

        private static void PrintListTableRow(ItemList list)
        {
            Console.WriteLine(
                $"| {list.Priority, 8} | {list.Id, 3} | {list.Status, -10} |");
        }

        private static void PrintMissionsTable(IEnumerable<Mission> missions)
        {
            if (!missions.Any())
            {
                Console.WriteLine("No missions are available.");

                return;
            }

            Console.WriteLine("Available missions (by priority, then by creation date):");

            Console.WriteLine(
                $"| {nameof(Mission.Priority), 8} " +
                $"| {nameof(Mission.Id), 3} " +
                $"| {nameof(Mission.Status), -10} " +
                $"| {nameof(Mission.ItemDescription), -40} " +
                $"| Quantities |");

            Console.WriteLine($"|----------|-----|------------|------------------------------------------|------------|");

            var completedMissions = missions
                   .Where(m => m.Status == MissionStatus.Completed || m.Status == MissionStatus.Incomplete)
                   .OrderBy(m => m.Priority)
                   .ThenBy(m => m.CreationDate);

            foreach (var mission in missions
                                        .Except(completedMissions)
                                        .OrderBy(m => m.Priority)
                                        .ThenBy(m => m.CreationDate))
            {
                PrintMissionTableRow(mission);
            }

            if (completedMissions.Any())
            {
                Console.WriteLine($"|----------|-----|------------|------------------------------------------|------------|");

                foreach (var mission in completedMissions)
                {
                    PrintMissionTableRow(mission);
                }
            }

            Console.WriteLine($"|__________|_____|____________|__________________________________________|____________|");
        }

        private static void PrintMissionTableRow(Mission mission)
        {
            string trimmedDescription = null;
            if (mission.ItemDescription != null)
            {
                trimmedDescription = mission.ItemDescription?.Substring(0, Math.Min(40, mission.ItemDescription.Length));
            }

            var quantities = $"{mission.DispatchedQuantity, 2} / {mission.RequestedQuantity, 2}";

            Console.WriteLine(
                $"| {mission.Priority, 8} | {mission.Id, 3} | {mission.Status, -10} | {trimmedDescription, -40} | {quantities, 10} |");
        }

        #endregion
    }
}
