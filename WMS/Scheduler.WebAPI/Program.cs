using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace Ferretto.WMS.Scheduler.WebAPI
{
    public class Program
    {
        #region Fields

        private const string ConsoleArgument = "--console";

        #endregion Fields

        #region Methods

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
            var host = WebHost.CreateDefaultBuilder(webHostArgs)
                .UseContentRoot(pathToContentRoot)
                .UseStartup<Startup>()
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
