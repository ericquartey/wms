using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;

namespace Ferretto.WMS.Scheduler.WebAPI
{
    public class Program
    {
        #region Fields

        private const string ConsoleArgument = "--console";

        #endregion Fields

        #region Methods

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>();

        public static void Main(string[] args)
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
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hostconfig.json", optional: true)
                .Build();

            var host = CreateWebHostBuilder(webHostArgs)
                    .UseContentRoot(pathToContentRoot)
                    .UseConfiguration(config)
                    .Build();

            if (isService)
            {
                host.RunAsService();
            }
            else
            {
                host.Run();
            }
        }

        #endregion Methods
    }
}
