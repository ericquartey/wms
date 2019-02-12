using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            var asd = webHost.Services.GetService<IConfiguration>();
            var automationService = webHost.Services.GetService<IAutomationService>();
            webHost.Run();
        }

        #endregion
    }
}
