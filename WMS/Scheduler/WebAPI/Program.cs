using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.WebAPI
{
    public static class Program
    {
        #region Fields

        private const string ConsoleArgument = "--console";

        private const string HostConfigurationFile = "hostconfig.json";

        #endregion Fields

        #region Methods

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost
            .CreateDefaultBuilder(args)
             .ConfigureLogging((context, logBuilder) =>
             {
                 logBuilder.AddEventLog(new Microsoft.Extensions.Logging.EventLog.EventLogSettings { SourceName = "WMSSchedulerService" });
             })
            .UseStartup<Startup>();

        public static void Main(string[] args)
        {
            IWebHost host = null;

            try
            {
                var pathToContentRoot = Directory.GetCurrentDirectory();

                var isService = !(Debugger.IsAttached || args.Contains(ConsoleArgument));
                if (isService)
                {
                    var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                    pathToContentRoot = Path.GetDirectoryName(pathToExe);
                }

                var webHostArgs = args.Where(arg => arg != ConsoleArgument).ToArray();

                var config = new ConfigurationBuilder()
                    .SetBasePath(pathToContentRoot)
                    .AddJsonFile(HostConfigurationFile, optional: true)
                    .Build();

                host = CreateWebHostBuilder(webHostArgs)
                        .UseContentRoot(pathToContentRoot)
                        .UseConfiguration(config)
                        .Build();

                var logger = host.Services.GetService(typeof(ILogger<Startup>)) as ILogger<Startup>;
                logger.LogInformation($"Starting WMS Scheduler service.");

                if (isService)
                {
                    host.RunAsService();
                }
                else
                {
                    host.Run();
                }

                logger.LogInformation($"WMS Scheduler service stopped.");
            }
            catch (Exception ex)
            {
                var logger = host?.Services.GetService(typeof(ILogger<Startup>)) as ILogger<Startup>;
                logger?.LogError(ex, $"Service terminated due to unhandled exception.");
            }
        }

        #endregion Methods
    }
}
