using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog.Web;

namespace Ferretto.VW.TelemetryService
{
    public class Program
    {
        #region Fields

        private const string ServiceConsoleArgument = "--service";

        #endregion

        #region Methods

        public static IHostBuilder CreateHostBuilder(string[] args, bool isService)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseNLog();

            if (isService)
            {
                builder.UseWindowsService();
            }

            return builder;
        }

        public static void Main(string[] args)
        {
            var isService = !System.Diagnostics.Debugger.IsAttached && args.Contains(ServiceConsoleArgument);

            var pathToContentRoot = isService
                    ? System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                    : System.IO.Directory.GetCurrentDirectory();

            var webHostArgs = args.Where(arg => arg != ServiceConsoleArgument).ToArray();

            CreateHostBuilder(webHostArgs, isService).Build().Run();
        }

        #endregion
    }
}
