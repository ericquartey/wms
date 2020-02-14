using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.AutomationService
{
    public static class HealthChecksExtensions
    {
        #region Fields

        private const string LiveHealthCheckTag = "live";

        private const string ReadyHealthCheckTag = "ready";

        #endregion

        #region Methods

        public static IServiceCollection AddMasHealthChecks(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddHealthChecks()
                .AddCheck<LivelinessHealthCheck>("liveliness-check", null, tags: new[] { LiveHealthCheckTag })
                .AddCheck<ReadinessHealthCheck>("readiness-check", null, tags: new[] { ReadyHealthCheckTag });

            return services;
        }

        public static IApplicationBuilder UseMasHealthChecks(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseHealthChecks("/health/ready", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains(ReadyHealthCheckTag),
            });

            app.UseHealthChecks("/health/live", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains(LiveHealthCheckTag),
            });

            return app;
        }

        #endregion
    }
}
