using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Ferretto.WMS.Scheduler.WebAPI.Host
{
    public class Program
    {
        #region Methods

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();

        public static void Main(string[] args)
        {
        }

        #endregion Methods
    }
}
