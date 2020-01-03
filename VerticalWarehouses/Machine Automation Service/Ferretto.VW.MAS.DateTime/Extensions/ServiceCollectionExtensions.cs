using System;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.TimeManagement
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddTimeServices(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IEventAggregator, EventAggregator>();

            services.AddHostedService<SystemTimeSyncService>();
            services.AddSingleton<ISystemTimeSyncService>(s => s.GetRequiredService<SystemTimeSyncService>());

            services.AddTransient<ISystemTimeProvider, SystemTimeProvider>();

            return services;
        }

        #endregion
    }
}
