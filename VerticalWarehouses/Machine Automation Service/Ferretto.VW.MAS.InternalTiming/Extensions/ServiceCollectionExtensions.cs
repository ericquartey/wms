using System;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.InternalTiming
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddInternalTimingServices(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IEventAggregator, EventAggregator>();

            services.AddHostedService<InternalTimingService>();

            return services;
        }

        #endregion
    }
}
