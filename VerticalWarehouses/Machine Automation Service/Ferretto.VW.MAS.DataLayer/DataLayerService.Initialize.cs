using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Extensions;
using Ferretto.VW.MAS.DataLayer.Interfaces;
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

        private async Task ApplyMigrationsAsync()
        {
            this.EnsureFolderExistence();

            try
            {
                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    var redundancyService = scope
                        .ServiceProvider
                        .GetRequiredService<IDbContextRedundancyService<DataLayerContext>>();

                    redundancyService.IsEnabled = false;

                    using (var activeDbContext = new DataLayerContext(redundancyService.ActiveDbContextOptions))
                    {
                        var pendingMigrations = await activeDbContext.Database.GetPendingMigrationsAsync();
                        if (pendingMigrations.Any())
                        {
                            this.Logger.LogInformation($"Applying {pendingMigrations.Count()} migrations to active database ...");
                            await activeDbContext.Database.MigrateAsync();
                        }
                    }

                    using (var standbyDbContext = new DataLayerContext(redundancyService.StandbyDbContextOptions))
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
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error while migating databases.");
                this.SendErrorMessage(new DLExceptionMessageData(ex, "Error while migating databases.", 0, MessageVerbosity.Fatal));
            }
        }

        /*
        private async Task BackupDatabaseAsync()
        {
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var redundancyService = scope
                  .ServiceProvider
                  .GetRequiredService<IDbContextRedundancyService<DataLayerContext>>();

                redundancyService.IsEnabled = false;
                try
                {
                    using (var activeDbContext = new DataLayerContext(redundancyService.ActiveDbContextOptions))
                    {
                        activeDbContext.Database.ExecuteSqlCommand
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while backing up database.");
                }
                finally
                {
                    redundancyService.IsEnabled = true;
                }
            }
        }*/

        private void EnsureFolderExistence()
        {
            if (!System.IO.Directory.Exists("Database"))
            {
                System.IO.Directory.CreateDirectory("Database");
            }
        }

        private async Task InitializeAsync()
        {
            // await this.BackupDatabaseAsync();

            await this.ApplyMigrationsAsync();

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                try
                {
                    await this.LoadConfigurationAsync(
                        configuration.GetDataLayerConfigurationFile(),
                        scope.ServiceProvider.GetRequiredService<DataLayerContext>());

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
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while loading configuration values.");
                    this.SendErrorMessage(new DLExceptionMessageData(ex));
                }
            }
        }

        #endregion
    }
}
