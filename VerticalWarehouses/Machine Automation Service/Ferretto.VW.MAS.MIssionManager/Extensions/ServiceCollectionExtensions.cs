using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.MissionManager
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddMissionManager(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddHostedService<MissionSchedulingService>()
                .AddScoped<IMissionSchedulingProvider, MissionSchedulingProvider>();

            return services;
        }

        public static IServiceCollection AddWmsMissionManager(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddHostedService<WmsMissionProxyService>()
                .AddScoped<IMissionOperationsProvider, MissionOperationsProvider>();

            return services;
        }

        #endregion
    }
}
