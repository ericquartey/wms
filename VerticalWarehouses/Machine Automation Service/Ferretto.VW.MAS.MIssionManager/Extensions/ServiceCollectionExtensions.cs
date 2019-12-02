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
#if MOCK
                .AddTransient<IMissionSchedulingProvider, MockedMissionSchedulingProvider>()
#else
                .AddTransient<IMissionSchedulingProvider, MissionSchedulingProvider>()
#endif
                .AddTransient<IMissionOperationsProvider, MissionOperationsProvider>();

            return services;
        }

        #endregion
    }
}
