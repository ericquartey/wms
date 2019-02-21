using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.Core.Services
{
    public class SchedulerService : BackgroundService
    {
        private readonly ILogger<SchedulerService> logger;

        private readonly IServiceScopeFactory scopeFactory;

        public SchedulerService(
            ILogger<SchedulerService> logger,
            IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.scopeFactory = scopeFactory;
        }

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.CheckDatabaseStatusAsync(cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                this.logger.LogDebug("Checking for pending scheduler requests to process ...");

                using (var scope = this.scopeFactory.CreateScope())
                {
                    var requestsProvider = scope.ServiceProvider.GetRequiredService<ISchedulerRequestProvider>();
                    var missionsProvider = scope.ServiceProvider.GetRequiredService<IMissionSchedulerProvider>();

                    var requests = await requestsProvider.GetRequestsToProcessAsync();
                    await missionsProvider.CreateForRequestsAsync(requests);
                }

                this.logger.LogDebug("Done processing pending requests.");
            }
            catch
            {
                this.logger.LogWarning("Scheduler start-up request processing failed.");
                await this.StopAsync(stoppingToken);
            }
        }

        private async Task CheckDatabaseStatusAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (var scope = this.scopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    var database = databaseContext.Database;

                    this.logger.LogDebug("Checking database structure ...");

                    var pendingMigrations = await database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        this.logger.LogInformation($"A total of {pendingMigrations.Count()} pending migrations found.");
#if DEBUG
                        this.logger.LogDebug($"Applying migrations ...");

                        await database.MigrateAsync();

                        this.logger.LogDebug($"Reseeding database ...");

                        var cleanAll = await System.IO.File.ReadAllTextAsync(@"bin\Debug\netcoreapp2.1\Seeds\Dev.CleanAll.sql");
                        var initDbScript = await System.IO.File.ReadAllTextAsync(@"bin\Debug\netcoreapp2.1\Seeds\Dev.InitDb.sql");
                        var itemsScript = await System.IO.File.ReadAllTextAsync(@"bin\Debug\netcoreapp2.1\Seeds\Dev.Items.sql");

                        await database.ExecuteSqlCommandAsync(cleanAll);
                        await database.ExecuteSqlCommandAsync(initDbScript);
                        await database.ExecuteSqlCommandAsync(itemsScript);
#else
                        this.logger.LogCritical("Database is not up to date. Please apply the migrations and restart the service.");
                        await this.StopAsync(stoppingToken);
#endif
                    }
                    else
                    {
                        this.logger.LogDebug($"Database is up to date.");
                    }
                }
            }
            catch
            {
                this.logger.LogCritical("Unable to check database structure.");
                await this.StopAsync(stoppingToken);
            }
        }

        #endregion
    }
}
