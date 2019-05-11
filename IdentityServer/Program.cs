using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Ferretto.IdentityServer
{
    public class Program
    {
        #region Methods

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>();
        }

        public static void Main(string[] args)
        {
            Console.Title = "Ferretto Identity Server";

            CreateWebHostBuilder(args)
                .Build()
                .Run();
        }

        #endregion
    }
}
