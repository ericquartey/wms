using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.TelemetryService.Data
{
    public class DataContext : DbContext, IDataContext
    {
        #region Fields

        internal const string ConnectionStringName = "Database";

        internal const string DefaultConnectionString = "Data Source=Database.db";

        private const string DefaultApplicationSettingsFile = "appsettings.json";

        #endregion

        #region Constructors

        public DataContext()
        { }

        public DataContext(DbContextOptions options)
            : base(options)
        { }

        #endregion

        #region Properties

        public DbSet<ErrorLog> ErrorLogs { get; set; }

        public DbSet<IOLog> IOLogs { get; set; }

        public DbSet<Machine> Machines { get; set; }

        public DbSet<MissionLog> MissionLogs { get; set; }

        public DbSet<ScreenShot> ScreenShots { get; set; }

        public DbSet<ServicingInfo> ServicingInfos { get; set; }

        #endregion

        #region Methods

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder is null)
            {
                throw new ArgumentNullException(nameof(optionsBuilder));
            }

            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile(DefaultApplicationSettingsFile, optional: false, reloadOnChange: false)
                .Build();

            var connectionString =
                configurationBuilder.GetConnectionString(ConnectionStringName)?.Replace("%localappdata%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StringComparison.OrdinalIgnoreCase)
                ??
                DefaultConnectionString;

            optionsBuilder
                .UseSqlite(connectionString)
                .EnableSensitiveDataLogging();
        }

        #endregion
    }
}
