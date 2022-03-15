using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.TelemetryService.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.TelemetryService.Data
{
    internal sealed class DataService : BackgroundService
    {
        #region Fields

        private const string ConnectionStringName = "Database";

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
                this.EnsureFolderExistence(scope);
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

                await this.CheckProxyAsync();
            }
        }

        private async Task CheckProxyAsync()
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var proxy = scope.ServiceProvider.GetRequiredService<IProxyProvider>().Get();
            if (proxy != null
                && !string.IsNullOrEmpty(proxy.Url))
            {
                var telemetryWebHubClient = scope.ServiceProvider.GetRequiredService<ITelemetryWebHubClient>();
                var webProxy = new WebProxy(proxy.Url)
                {
                    Credentials = new NetworkCredential(proxy.User, proxy.PasswordHash)
                };
                await telemetryWebHubClient.SetProxy(webProxy);
            }
        }

        private void EnsureFolderExistence(IServiceScope scope)
        {
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString(ConnectionStringName);
            var fileName = connectionString.Split('=')[1]?.Trim('\'');
            var dirName = Path.GetDirectoryName(fileName);
            if (dirName != "" && !Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }

        #endregion
    }
}
