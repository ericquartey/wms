using System;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.AutomationServiceMock
{
    internal static class Program
    {
        #region Methods

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            services.AddSingleton(typeof(IConfiguration), configuration);
            services.AddLogging(b => b.AddConfiguration(configuration).AddConsole());

            services.AddSingleton<IAutomationService, AutomationService>();

            var schedulerUrl = configuration["Scheduler:Url"];
            var wakeUpPath = configuration["Hubs:WakeUp"];
            services.AddTransient<IWakeupHubClient>(s => new WakeupHubClient(schedulerUrl, wakeUpPath));

            services.AddWebAPIServices(schedulerUrl);

            return services.BuildServiceProvider();
        }

        private static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Press <ENTER> to start the automation service.");
                Console.ReadLine();
                var configuration = SetupConfiguration(args);

                var serviceProvider = ConfigureServices(configuration);

                var automationService = serviceProvider.GetService<IAutomationService>();

                await automationService.InitializeAsync();

                // notify the scheduler that a user logged in
                // to the PanelPC associated to the specified bay
                await automationService.NotifyUserLoginAsync(bayId: 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine();
            }
            finally
            {
                Console.WriteLine("Press <ENTER> to terminate the automation service.");
                Console.ReadLine();
            }
        }

        private static IConfigurationRoot SetupConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddCommandLine(args)
                .Build();
        }

        #endregion Methods
    }
}
