using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BackgroundService
{
    public class Program
    {
        #region Methods

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("hostconfig.json", optional: true)
               .Build();
            var host = CreateWebHostBuilder(args)
                    .UseConfiguration(config)
                    .Build();
            host.Run();
        }

        #endregion Methods
    }
}
