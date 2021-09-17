using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.TelemetryService.Data
{
    internal sealed class DataService : BackgroundService
    {
        #region Fields

        private readonly ILogger<DataService> logger;

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public DataService(ILogger<DataService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        #endregion

        #region Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                var pendingMigrations = await dataContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    this.logger.LogDebug($"Applying {pendingMigrations.Count()} migrations to database ...");
                    await dataContext.Database.MigrateAsync();
                }

                var dataServiceStatus = scope.ServiceProvider.GetRequiredService<IDataServiceStatus>();
                dataServiceStatus.IsReady = true;

                this.logger.LogInformation("Database is ready.");
            }
        }

        #endregion
    }
}
