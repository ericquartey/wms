using System;
using Ferretto.VW.MAS.InverterDriver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.NordDriver
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddNordDriver(
            this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHostedService<NordDriverService>();

            services.AddScoped<INordProvider, NordProvider>();

            services.AddSingleton(s =>
            {
                var configuration = s.GetRequiredService<IConfiguration>();

                return new SocketTransport(configuration) as ISocketTransport;
            });

            return services;
        }

        #endregion
    }
}
