using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal partial class DataLayerService
    {
        #region Methods

        private static string GetSeedFileName(string environmentName)
        {
            return environmentName is null
                ? "configuration/seeds/seed.sql"
                : $"configuration/seeds/seed.{environmentName}.sql";
        }

        private async Task ApplyMigrationsAsync(IServiceScope scope)
        {
            this.EnsureFolderExistence(scope);

            var redundancyService = scope
                .ServiceProvider
                .GetRequiredService<IDbContextRedundancyService<DataLayerContext>>();

            redundancyService.IsEnabled = false;

            using (var activeDbContext = new DataLayerContext(redundancyService.ActiveDbContextOptions, redundancyService))
            {
                var pendingMigrations = await activeDbContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    this.Logger.LogInformation($"Applying {pendingMigrations.Count()} migrations to active database ...");
                    await activeDbContext.Database.MigrateAsync();
                }
            }

            using (var standbyDbContext = new DataLayerContext(redundancyService.StandbyDbContextOptions, redundancyService))
            {
                var pendingMigrations = await standbyDbContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    this.Logger.LogInformation($"Applying {pendingMigrations.Count()} migrations to standby database ...");
                    await standbyDbContext.Database.MigrateAsync();
                }
            }

            redundancyService.IsEnabled = true;
        }

        private void EnsureFolderExistence(IServiceScope scope)
        {
            if (!System.IO.Directory.Exists("Database"))
            {
                System.IO.Directory.CreateDirectory("Database");
            }

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var primaryDb = configuration.GetDataLayerPrimaryConnectionString().Split('=')[1]?.Trim('\'');
            var secondaryDb = configuration.GetDataLayerSecondaryConnectionString().Split('=')[1]?.Trim('\'');
            if (!string.IsNullOrEmpty(primaryDb)
                && !string.IsNullOrEmpty(secondaryDb)
                && !System.IO.File.Exists(secondaryDb)
                )
            {
                this.Logger.LogWarning($"No standby database. Copy database from [{primaryDb}] to [{secondaryDb}].");
                System.IO.File.Copy(primaryDb, secondaryDb);
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    await this.ApplyMigrationsAsync(scope);

                    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var dataContext = scope.ServiceProvider.GetRequiredService<DataLayerContext>();

                    var model = dataContext.Model;

                    await this.LoadConfigurationAsync(configuration.GetDataLayerConfigurationFile(), dataContext);
                    this.GenerateInstructionDefinitions(dataContext);
                    this.GenerateAccessories(dataContext);

                    this.IsReady = true;

                    var message = new NotificationMessage(
                        null,
                        "DataLayer initialization complete.",
                        MessageActor.Any,
                        MessageActor.DataLayer,
                        MessageType.DataLayerReady,
                        BayNumber.None);

                    this.EventAggregator
                        .GetEvent<NotificationEvent>()
                        .Publish(message);

                    this.Logger.LogInformation("Data layer service initialized.");
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error while initializing database.");
                this.SendErrorMessage(new DLExceptionMessageData(ex, null, 0, MessageVerbosity.Fatal));
            }
        }

        #endregion
    }
}
