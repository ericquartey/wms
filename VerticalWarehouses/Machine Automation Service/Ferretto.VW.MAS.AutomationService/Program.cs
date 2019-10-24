using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Ferretto.VW.MAS.AutomationService
{
    public class Program
    {
        #region Fields

        private const string ServiceConsoleArgument = "--service";

        #endregion

        #region Methods

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseNLog()
                .UseStartup<Startup>();

        public static int Main(string[] args)
        {
            try
            {
                var pathToContentRoot = Directory.GetCurrentDirectory();

                var isService = !Debugger.IsAttached && !args.Contains(ServiceConsoleArgument);
                if (isService)
                {
                    var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                    pathToContentRoot = Path.GetDirectoryName(pathToExe);
                }

                var webHostArgs = args.Where(arg => arg != ServiceConsoleArgument).ToArray();

                var host = CreateWebHostBuilder(webHostArgs)
                    .UseContentRoot(pathToContentRoot)
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
            catch
            {
                return -1;
            }

            return 0;
        }

        #endregion
    }
}
