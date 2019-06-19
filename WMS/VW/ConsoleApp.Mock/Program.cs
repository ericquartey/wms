using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    internal static class Program
    {
        #region Methods

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
           WebHost
           .CreateDefaultBuilder(args)
           .UseStartup<Startup>();

        private static int Main(string[] args)
        {
            Views.DisplayHeader();

            try
            {
                var pathToContentRoot = Directory.GetCurrentDirectory();

                var host = CreateWebHostBuilder(args)
                    .UseContentRoot(pathToContentRoot)
                    .Build();

                Console.WriteLine("Press <ENTER> to boot.");
                Console.Write("> ");
                Console.ReadLine();

                host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                Console.WriteLine("Press <ENTER> to terminate the application.");
                Console.ReadLine();

                return -1;
            }

            return 0;
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
