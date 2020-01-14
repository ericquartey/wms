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

        internal const string ServiceConsoleArgument = "--service";

        private const int NoError = 0;

        private const string PasswordConsoleArgument = "--password";

        private const string RegisterAsServiceConsoleArgument = "--register-as-service";

        private const string ServiceName = "Ferretto Machine Automation Service";

        private const string UserConsoleArgument = "--user";

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

                if (args.Contains(RegisterAsServiceConsoleArgument))
                {
                    return new ServiceBuilder(ServiceName)
                        .Register(
                            userName: GetArgumentValue(args, UserConsoleArgument),
                            password: GetArgumentValue(args, PasswordConsoleArgument));
                }

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
                    Console.Title = "Vertimag Automation Service";
                    host.Run();
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Application terminated unexpectedly.");

                return -1;
            }

            logger.LogInformation("Application terminated.");

            return NoError;
        }

        private static string GetArgumentValue(string[] args, string argumentName)
        {
            return args.SkipWhile(a => a != argumentName).Skip(1).First();
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
