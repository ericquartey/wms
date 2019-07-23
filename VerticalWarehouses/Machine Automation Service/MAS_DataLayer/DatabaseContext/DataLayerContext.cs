using System;
using System.IO;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Configurations;
using Ferretto.VW.MAS.DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.DataLayer
{
    public class DataLayerContext : DbContext
    {
        #region Fields

        private const string CONNECTION_STRING_NAME = "AutomationService";

        private const string DEFAULT_APPLICATION_SETTINGS_FILE = "appsettings.json";

        #endregion

        // TODO: use IConfiguration injection instead

        #region Constructors

        public DataLayerContext()
        {
        }

        public DataLayerContext(DbContextOptions<DataLayerContext> options)
            : base(options)
        {
        }

        #endregion

        #region Properties

        public DbSet<Cell> Cells { get; set; }

        public DbSet<ConfigurationValue> ConfigurationValues { get; set; }

        public DbSet<Error> Errors { get; set; }

        public DbSet<ErrorStatistic> ErrorStatistics { get; set; }

        public DbSet<FreeBlock> FreeBlocks { get; set; }

        public DbSet<LoadingUnit> LoadingUnits { get; set; }

        public DbSet<LogEntry> LogEntries { get; set; }

        public DbSet<MachineStatistics> MachineStatistics { get; set; }

        public DbSet<RuntimeValue> RuntimeValues { get; set; }

        public DbSet<ServicingInfo> ServicingInfo { get; set; }

        #endregion

        #region Methods

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder == null)
            {
                throw new ArgumentNullException(nameof(optionsBuilder));
            }

            if (optionsBuilder.IsConfigured)
            {
                return;
            }
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(DEFAULT_APPLICATION_SETTINGS_FILE, optional: false, reloadOnChange: false)
                .Build();

            var connectionString = configurationBuilder.GetConnectionString(CONNECTION_STRING_NAME);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"Unable to locate the connection string '{CONNECTION_STRING_NAME}'.");
            }

            optionsBuilder.UseSqlite(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.ApplyConfiguration(new MachineStatisticsConfiguration());

            modelBuilder.ApplyConfiguration(new ConfigurationValuesConfiguration());
            modelBuilder.ApplyConfiguration(new RuntimeValuesConfiguration());

            modelBuilder.ApplyConfiguration(new ErrorConfiguration());
            modelBuilder.ApplyConfiguration(new ErrorStatisticConfiguration());

            modelBuilder.ApplyConfiguration(new CellsConfiguration());

            modelBuilder.ApplyConfiguration(new LoadingUnitsConfiguration());
            modelBuilder.ApplyConfiguration(new ServicingInfoConfiguration());
        }

        #endregion
    }
}
