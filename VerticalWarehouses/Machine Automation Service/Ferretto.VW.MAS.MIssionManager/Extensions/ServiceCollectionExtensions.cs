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
                .AddHostedService<WmsMissionProxyService>();

            services
                .AddTransient<IMissionSchedulingProvider, MissionSchedulingProvider>()
                .AddTransient<IMissionOperationsProvider, MissionOperationsProvider>();

            return services;
        }

        #endregion
    }
}
