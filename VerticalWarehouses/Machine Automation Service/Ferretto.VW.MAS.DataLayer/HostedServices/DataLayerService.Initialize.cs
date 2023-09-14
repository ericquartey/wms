using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
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

            // WARNING: 'using' removed to prevent following exception:
            // -System.ObjectDisposedException: Cannot access a disposed object.
            // -A common cause of this error is disposing a context that was resolved from dependency injection and then later trying to use the same context instance elsewhere in your application.
            // -This may occur if you are calling Dispose() on the context, or wrapping the context in a using statement.
            // -If you are using dependency injection, you should let the dependency injection container take care of disposing context instances.
            var activeDbContext = new DataLayerContext(redundancyService.ActiveDbContextOptions, redundancyService);
            var pendingMigrations = await activeDbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                this.Logger.LogInformation($"Applying {pendingMigrations.Count()} migrations to active database ...");
                await activeDbContext.Database.MigrateAsync();
            }

            var standbyDbContext = new DataLayerContext(redundancyService.StandbyDbContextOptions, redundancyService);
            var standbyPendingMigrations = await standbyDbContext.Database.GetPendingMigrationsAsync();
            if (standbyPendingMigrations.Any())
            {
                this.Logger.LogInformation($"Applying {standbyPendingMigrations.Count()} migrations to standby database ...");
                await standbyDbContext.Database.MigrateAsync();
            }

            var defaultDbContext = new DataLayerContext(redundancyService.DefaultDbContextOptions, redundancyService);
            var defaultPendingMigrations = await defaultDbContext.Database.GetPendingMigrationsAsync();
            if (defaultPendingMigrations.Any())
            {
                this.Logger.LogInformation($"Applying {defaultPendingMigrations.Count()} migrations to default database ...");
                await standbyDbContext.Database.MigrateAsync();
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
            var defaultDb = configuration.GetDataLayerDefaultConnectionString().Split('=')[1]?.Trim('\'');

            if (!string.IsNullOrEmpty(secondaryDb) && System.IO.File.Exists(secondaryDb))
            {
                // create a backup
                var currentDir = System.IO.Path.GetDirectoryName(secondaryDb);
                var currentName = System.IO.Path.GetFileNameWithoutExtension(secondaryDb);
                var currentExt = System.IO.Path.GetExtension(secondaryDb);
                var bakFile = $"{currentDir}/{currentName}.bak{currentExt}";
                try
                {
                    if (System.IO.File.Exists(bakFile))
                    {
                        System.IO.File.Delete(bakFile);
                    }
                    System.IO.File.Move(secondaryDb, bakFile);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex.Message);
                }
            }
            if (!string.IsNullOrEmpty(primaryDb) && System.IO.File.Exists(primaryDb) && !string.IsNullOrEmpty(secondaryDb) && !System.IO.File.Exists(secondaryDb))
            {
                this.Logger.LogWarning($"No standby database. Copy database from [{primaryDb}] to [{secondaryDb}].");
                var secondaryDir = System.IO.Path.GetDirectoryName(secondaryDb);
                try
                {
                    if (!System.IO.Directory.Exists(secondaryDir))
                    {
                        System.IO.Directory.CreateDirectory(secondaryDir);
                    }
                    System.IO.File.Copy(primaryDb, secondaryDb);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex.Message);
                }
            }
            if (!string.IsNullOrEmpty(defaultDb) && !System.IO.File.Exists(defaultDb))
            {
                // create a Db with all default value from ConfigurationFile
                this.Logger.LogWarning($"No default database. create database from [{configuration.GetDataLayerConfigurationFile()}].");

                var defaultDir = System.IO.Path.GetDirectoryName(defaultDb);
                try
                {
                    if (!System.IO.Directory.Exists(defaultDir))
                    {
                        System.IO.Directory.CreateDirectory(defaultDir);
                    }
                    System.IO.File.Copy(primaryDb, defaultDb);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex.Message);
                }
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
                    await this.LoadConfigurationAsync(configuration.GetDataLayerConfigurationFile(), dataContext);

                    var redundancyService = scope.ServiceProvider.GetRequiredService<IDbContextRedundancyService<DataLayerContext>>();
                    var defaultDataContext = new DataLayerContext(redundancyService.DefaultDbContextOptions, redundancyService);
                    await this.LoadConfigurationAsync(configuration.GetDataLayerDefaultConnectionString(), defaultDataContext, true);

                    this.GenerateInstructionDefinitions(dataContext);
                    this.GenerateAccessories(dataContext);
                    this.GenerateSetupProcedures(dataContext);
                    var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                    baysDataProvider.CheckBayFindZeroLimit();
                    baysDataProvider.CheckProfileConst();

                    var rotationClassSchedule = scope.ServiceProvider.GetRequiredService<IRotationClassScheduleProvider>();
                    rotationClassSchedule.CheckRotationClass();

                    var accessories = scope.ServiceProvider.GetRequiredService<IAccessoriesDataProvider>();
                    accessories.CheckAccessories();

                    // performance optimization
                    var elevatorDataProvider = scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
                    elevatorDataProvider.GetAxis(Orientation.Horizontal);
                    elevatorDataProvider.UpdateLastIdealPosition(-999999);
                    elevatorDataProvider.SetCurrentBayPosition(null);
                    elevatorDataProvider.SetCurrentCell(null);

                    elevatorDataProvider.GetAxis(Orientation.Vertical);
                    elevatorDataProvider.GetLoadingUnitOnBoard();
                    elevatorDataProvider.GetStructuralProperties();
                    var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                    machineProvider.CheckBackupServer();
                    baysDataProvider.AddElevatorPseudoBay();
                    var bays = baysDataProvider.GetAll();
                    var machineVolatileDataProvider = scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();

                    foreach (var bay in bays.Where(b => b.Carousel == null && b.External == null && b.Number != BayNumber.ElevatorBay))
                    {
                        machineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = true;
                    }
                    machineVolatileDataProvider.IsOneTonMachine = machineProvider.IsOneTonMachine();

                    scope.ServiceProvider.GetRequiredService<ISetupProceduresDataProvider>().GetAll();
                    var servicingInfo = scope.ServiceProvider.GetRequiredService<IServicingProvider>();
                    servicingInfo.CheckServicingInfo();

                    var loadUnitsDataProvider = scope.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
                    loadUnitsDataProvider.UpdateWeightStatistics();

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
