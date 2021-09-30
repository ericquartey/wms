using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.TelemetryService.Data
{
    public static class ServiceCollectionExtensions
    {
        #region Methods

        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHostedService<DataService>();

            services.AddScoped<IDataContext, DataContext>();
            services.AddSingleton<IDataServiceStatus, DataServiceStatus>();

            services.AddDbContext<DataContext>((provider, options) =>
            {
                var connectionString = provider
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString(DataContext.ConnectionStringName);

                options.UseSqlite(connectionString);

#if DEBUG
                options
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
#endif
            });

            return services;
        }

        #endregion
    }
}
