using System;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.TimeManagement
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddTimeServices(this IServiceCollection services, bool useWms)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IEventAggregator, EventAggregator>();

            if (useWms)
            {
                services.AddHostedService<SystemTimeSyncService>();
                services.AddScoped<ISystemTimeProvider, WmsSystemTimeProvider>();
            }
            else
            {
                services.AddScoped<ISystemTimeProvider, LocalSystemTimeProvider>();
            }

            services.AddScoped(s => s.GetRequiredService<ISystemTimeProvider>() as IInternalSystemTimeProvider);

            return services;
        }

        #endregion
    }
}
