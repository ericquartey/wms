using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.LaserDriver
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddLaserDriver(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHostedService<LaserDriverService>();

            services.AddScoped<ILaserProvider, LaserProvider>();

            return services;
        }

        #endregion
    }
}
