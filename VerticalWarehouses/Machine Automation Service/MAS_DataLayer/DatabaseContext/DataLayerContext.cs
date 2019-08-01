using System;
using System.IO;
using Ferretto.VW.MAS.DataLayer.Configurations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Cells;
using Ferretto.VW.MAS.DataModels.Errors;
using Ferretto.VW.MAS.DataModels.LoadingUnits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.DataLayer.DatabaseContext
{
    public partial class DataLayerContext : DbContext
    {
        #region Fields

        private const string CONNECTION_STRING_NAME = "AutomationService";

        private const string DEFAULT_APPLICATION_SETTINGS_FILE = "appsettings.json";

        #endregion

        #region Constructors

        public DataLayerContext()
        {
        }

        public DataLayerContext(DbContextOptions<DataLayerContext> options)
            : base(options)
        {
            this.Options = options;
        }

        #endregion

        #region Properties

        public DbSet<Bay> Bays { get; set; }

        public DbSet<Cell> Cells { get; set; }

        public DbSet<ConfigurationValue> ConfigurationValues { get; set; }

        public DbSet<ErrorDefinition> ErrorDefinitions { get; set; }

        public DbSet<Error> Errors { get; set; }

        public DbSet<ErrorStatistic> ErrorStatistics { get; set; }

        public DbSet<FreeBlock> FreeBlocks { get; set; }

        public DbSet<LoadingUnit> LoadingUnits { get; set; }

        public DbSet<LogEntry> LogEntries { get; set; }

        public DbSet<MachineStatistics> MachineStatistics { get; set; }

        public DbSet<ServicingInfo> ServicingInfo { get; set; }

        public DbSet<User> Users { get; set; }

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

            modelBuilder.ApplyConfiguration(new BaysConfiguration());
            modelBuilder.ApplyConfiguration(new CellsConfiguration());
            modelBuilder.ApplyConfiguration(new ConfigurationValuesConfiguration());
            modelBuilder.ApplyConfiguration(new ErrorDefinitionConfiguration());
            modelBuilder.ApplyConfiguration(new ErrorStatisticConfiguration());
            modelBuilder.ApplyConfiguration(new LoadingUnitsConfiguration());
            modelBuilder.ApplyConfiguration(new MachineStatisticsConfiguration());
            modelBuilder.ApplyConfiguration(new ServicingInfoConfiguration());
            modelBuilder.ApplyConfiguration(new UsersConfiguration());
        }

        #endregion
    }
}
