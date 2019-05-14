using System;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
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
            DisplayMissions = 1,

            ExecuteMission = 2,

            CompleteMission = 3,

            DisplayLists = 4,

            ExecuteList = 5,

            Exit = 6
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

            var schedulerUrl = configuration.GetValue<Uri>("Scheduler:Url");
            var hubPath = configuration.GetValue<Uri>("Hubs:Scheduler");
            var identityServerUrl = configuration.GetValue<Uri>("IdentityServer:Url");

            services
                 .AddWebApiServices(schedulerUrl)
                 .AddSchedulerHub(new Uri(schedulerUrl, hubPath))
                 .AddWebApiAuthenticationServices(identityServerUrl);

            return services.BuildServiceProvider();
        }

        private static async Task<bool> ExecuteOperationAsync(IAutomationService automationService, UserSelection selection)
        {
            var exitRequested = false;
            switch (selection)
            {
                case UserSelection.CompleteMission:
                    var completeMissionId = Views.GetMissionId();
                    var quantity = Views.GetQuantity();
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
                    var executeMissionId = Views.GetMissionId();
                    if (executeMissionId >= 0)
                    {
                        await automationService.ExecuteMissionAsync(executeMissionId);
                        Console.WriteLine($"Mission execution request sent.");
                    }

                    break;

                case UserSelection.ExecuteList:
                    var executeListId = Views.GetListId();
                    if (executeListId >= 0)
                    {
                        await automationService.ExecuteListAsync(executeListId);
                        Console.WriteLine($"List execution request sent.");
                    }

                    break;

                case UserSelection.DisplayMissions:

                    var missions = await automationService.GetMissionsAsync();

                    Views.PrintMissionsTable(missions);

                    break;

                case UserSelection.DisplayLists:

                    var lists = await automationService.GetListsAsync();

                    Views.PrintListsTable(lists);

                    break;

                case UserSelection.Exit:
                    exitRequested = true;
                    break;

                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }

            return exitRequested;
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
                Console.Title = "Ferretto VertiMAG Panel PC";
                Console.WriteLine("VertiMAG Panel PC");
                Console.WriteLine("-----------------");

                var automationService = BuildConfiguration(args);
                await PerformLoginAsync(automationService);

                var bay = await automationService.GetBayAsync();

                Console.WriteLine($"Serving bay '{bay.Description}'");
                Console.WriteLine("-----------------");

                var exitRequested = false;
                while (exitRequested == false)
                {
                    Console.WriteLine();
                    Console.WriteLine("Select option: ");
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

        private static async Task PerformLoginAsync(IAutomationService automationService)
        {
            try
            {
                Console.Write("Insert user name: ");
                var userName = Console.ReadLine();

                Console.Write("Insert password: ");
                var password = Views.GetConsoleSecurePassword();

                Console.WriteLine();

                var name = await automationService.LoginAsync(userName, password);

                Console.WriteLine($"Login successful.");
                Console.WriteLine($"Welcome {name}");
                Console.Title += " [logged in]";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failure: {ex.Message}");
            }
        }

        #endregion
    }
}
