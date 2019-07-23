using System;
using System.IO;
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
            modelBuilder.Entity<ConfigurationValue>().HasKey(cv => new { cv.CategoryName, cv.VarName });
            modelBuilder.Entity<RuntimeValue>().HasKey(cv => new { cv.CategoryName, cv.VarName });
        }

        #endregion
    }
}
