﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Ferretto.WMS.Data.WebAPI
{
    public static class Program
    {
        #region Fields

        private const string ConsoleArgument = "--console";

        #endregion

        #region Methods

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost
            .CreateDefaultBuilder(args)
            .UseNLog()
            .UseApplicationInsights()
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

                host = CreateWebHostBuilder(webHostArgs)
                           .UseContentRoot(pathToContentRoot)
                           .Build();

                var logger = host.Services.GetService(typeof(ILogger<Startup>)) as ILogger<Startup>;
                logger.LogInformation($"Starting WMS Data service.");

                if (isService)
                {
                    host.RunAsService();
                }
                else
                {
                    Console.Title = "Ferretto Data Service";
                    host.Run();
                }

                logger.LogInformation($"WMS Data service stopped.");
            }
            catch (Exception ex)
            {
                var logger = host?.Services.GetService(typeof(ILogger<Startup>)) as ILogger<Startup>;
                logger?.LogError(ex, $"Service terminated due to unhandled exception.");
            }
        }

        #endregion
    }
}
