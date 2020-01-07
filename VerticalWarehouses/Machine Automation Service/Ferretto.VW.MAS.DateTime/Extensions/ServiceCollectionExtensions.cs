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

            services.AddTransient<ISystemTimeProvider, SystemTimeProvider>();
            services.AddTransient(s => s.GetRequiredService<ISystemTimeProvider>() as IInternalSystemTimeProvider);

            return services;
        }

        #endregion
    }
}
