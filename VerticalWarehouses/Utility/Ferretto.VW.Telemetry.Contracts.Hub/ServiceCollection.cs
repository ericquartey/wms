using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.Telemetry.Contracts.Hub
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddTelemetryHub(this IServiceCollection services, Uri url)
        {
            if (url is null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<ITelemetryHubClient>(s => new TelemetryHubClient(url));

            return services;
        }

      

        #endregion
    }
}
