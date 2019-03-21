using System;
using System.IO;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS_DataLayer
{
    public class DataLayerContext : DbContext
    {
        #region Fields

        private const string ConnectionStringName = "AutomationService";

        private const string DefaultApplicationSettingsFile = "appsettings.json";

        #endregion

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

        public DbSet<FreeBlock> FreeBlocks { get; set; }

        public DbSet<LoadingUnit> LoadingUnits { get; set; }

        public DbSet<LogEntry> LogEntries { get; set; }

        public DbSet<RuntimeValue> RuntimeValues { get; set; }

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
                .AddJsonFile(DefaultApplicationSettingsFile, optional: false, reloadOnChange: false)
                .Build();

            var connectionString = configurationBuilder.GetConnectionString(ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"Unable to locate the connection string '{ConnectionStringName}'.");
            }

            optionsBuilder.UseSqlite(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConfigurationValue>().HasKey(cv => new { cv.CategoryName, cv.VarName });

            modelBuilder.Entity<ConfigurationValue>().HasData(new ConfigurationValue { VarName = (long)SetupNetwork.Inverter1, VarType = DataType.IPAddressType, VarValue = "169.254.231.248" });
            modelBuilder.Entity<ConfigurationValue>().HasData(new ConfigurationValue { VarName = (long)SetupNetwork.Inverter1Port, VarType = DataType.Integer, VarValue = "17221" });
            modelBuilder.Entity<ConfigurationValue>().HasData(new ConfigurationValue { VarName = (long)SetupNetwork.IOExpansion1, VarType = DataType.IPAddressType, VarValue = "169.254.231.10" });
            modelBuilder.Entity<ConfigurationValue>().HasData(new ConfigurationValue { VarName = (long)SetupNetwork.IOExpansion1Port, VarType = DataType.Integer, VarValue = "502" });
        }

        #endregion
    }
}
