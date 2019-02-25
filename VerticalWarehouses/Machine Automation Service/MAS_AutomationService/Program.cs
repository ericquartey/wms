using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Ferretto.VW.MAS_AutomationService
{
    public class Program
    {
        #region Methods

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        public static void Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();
            webHost.Run();
        }

        #endregion
    }
}
