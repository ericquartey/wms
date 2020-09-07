using System;
using Ferretto.VW.MAS.SocketLink.Providers;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.SocketLink
{
    public static class ServiceCollectionExtension
    {
        #region Methods

        public static IServiceCollection AddSocketLinkServices(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IEventAggregator, EventAggregator>();

            services.AddHostedService<SocketLinkSyncService>();
            services.AddScoped<ISocketLinkSyncProvider, SocketLinkProvider>();

            services.AddScoped<ISystemSocketLinkProvider, SystemSocketLinkProvider>();

            return services;
        }

        #endregion
    }
}
