using System;
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
            var isService = !Debugger.IsAttached && args.Contains(ServiceConsoleArgument);

            ILogger logger = null;
            try
            {
                var pathToContentRoot = isService
                    ? Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
                    : Directory.GetCurrentDirectory();

                var webHostArgs = args.Where(arg => arg != ServiceConsoleArgument).ToArray();

                var host = CreateWebHostBuilder(webHostArgs)
                    .UseContentRoot(pathToContentRoot)
                    .Build();

                logger = host.Services.GetRequiredService<ILogger<Startup>>();
                logger.LogInformation($"VertiMag Automation Service version {GetVersion()}");

                logger.LogInformation($"Working directory is '{pathToContentRoot}'.");

                if (isService)
                {
                    Directory.SetCurrentDirectory(pathToContentRoot);
                    host.RunAsService();
                }
                else
                {
                    Console.Title = "MAS";
                    host.Run();
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Service terminated unexpectedly.");

                throw;
            }

            logger?.LogInformation("Service exited with no error.");

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
