using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Ferretto.VW.MAS.AutomationService
{
    public static class Program
    {
        #region Fields

        private const int NoError = 0;

        private const string ServiceConsoleArgument = "--service";

        #endregion

        #region Methods

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseNLog()
                .UseStartup<Startup>();

        public static int Main(string[] args)
        {
            ILogger<Startup> logger = null;
            try
            {
                var pathToContentRoot = Directory.GetCurrentDirectory();

                var isService = !Debugger.IsAttached && args.Contains(ServiceConsoleArgument);
                if (isService)
                {
                    var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                    pathToContentRoot = Path.GetDirectoryName(pathToExe);
                }

                var webHostArgs = args.Where(arg => arg != ServiceConsoleArgument).ToArray();

                var host = CreateWebHostBuilder(webHostArgs)
                    .UseContentRoot(pathToContentRoot)
                    .Build();

                logger = host.Services.GetRequiredService<ILogger<Startup>>();
                var versionString = GetVersion();

                logger.LogInformation($"VertiMag Automation Service version {versionString}");

                if (isService)
                {
                    host.RunAsService();
                }
                else
                {
                    System.Console.Title = "Vertimag Automation Service";
                    host.Run();
                }
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex, "Application terminated unexpectedly.");

                return -1;
            }

            logger.LogInformation("Application terminated.");

            return NoError;
        }

        private static string GetVersion()
        {
            var versionAttribute = typeof(Program).Assembly
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true)
                .FirstOrDefault() as AssemblyInformationalVersionAttribute;

            return versionAttribute?.InformationalVersion ?? typeof(Program).Assembly.GetName().Version.ToString();
        }

        #endregion
    }
}
