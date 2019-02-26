using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Ferretto.VW.MAS_AutomationService
{
    public class Program
    {
        #region Methods

        public static IWebHostBuilder CreateWebHostBuilder(String[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }

        public static void Main(String[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();
            //var automationService = webHost.Services.GetService<IAutomationService>();
            webHost.Run();
        }

        #endregion
    }
}
