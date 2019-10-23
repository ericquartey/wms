using System;
using Ferretto.VW.MAS.MissionManager.BackgroundService;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.MissionManager.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddMissionManager(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddHostedService<MissionManagerService>();

            return services;
        }

        #endregion
    }
}
