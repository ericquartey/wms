using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MAS_DataLayer
{
    public class DataLayerContext : DbContext
    {
        private const string DefaultApplicationSettingsFile = "appsettings.json";
        private const string ConnectionStringName = "AutomationService";

        #region Properties

        public DbSet<Operation> Operations { get; set; }

        public DbSet<StatusLog> StatusLogs { get; set; }

        public DbSet<Step> Steps { get; set; }

        #endregion Properties

        #region Constructors

        public DataLayerContext()
        {
        }

        public DataLayerContext(DbContextOptions<DataLayerContext> options)
            : base(options)
        {
        }

        #endregion Constructors

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

            if(string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"Unable to locate the connection string '{ConnectionStringName}'.");
            }

            optionsBuilder.UseSqlite(connectionString);
        }

        #endregion Methods
    }
}
