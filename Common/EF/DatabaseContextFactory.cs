using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
#if NET4
using System.Configuration;
#else
using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
#endif

namespace Ferretto.Common.EF
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        #region Fields

        private const string ConnectionStringName = "WmsConnectionString";

        private const string DefaultApplicationSettingsFile = "appsettings.json";

        private const string NetcoreEnvironmentEnvVariable = "ASPNETCORE_ENVIRONMENT";

        private const string ParametrizedApplicationSettingsFile = "appsettings.{0}.json";

#if !NET4
        private const string cloudInitialCatalogEnvVariable = "cloudInitialCatalog";

        private static readonly System.Text.RegularExpressions.Regex ConnectionStringInitialCatalog =
            new System.Text.RegularExpressions.Regex(
                @"Initial Catalog=(?<dbname>[^;]+)",
                System.Text.RegularExpressions.RegexOptions.Compiled);
#endif

        #endregion

        #region Methods

        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

#if NET4
            var configuration = ConfigurationManager.ConnectionStrings[ConnectionStringName];

            if (configuration == null)
            {
                throw new ConfigurationErrorsException($"Connection string '{ConnectionStringName}' not found.");
            }

            optionsBuilder.UseSqlServer(configuration.ConnectionString);
#else
            var applicationSettingsFile = GetSettingFileFromEnvironment();

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(applicationSettingsFile, optional: false, reloadOnChange: false)
                .Build();

            var connectionString = configurationBuilder.GetConnectionString(ConnectionStringName);
            var cloudInitialCatalog = Environment.GetEnvironmentVariable(cloudInitialCatalogEnvVariable);

            if (cloudInitialCatalog != null)
            {
                connectionString = ConnectionStringInitialCatalog.Replace(
                    connectionString,
                    $"Initial Catalog={cloudInitialCatalog}");
            }

            optionsBuilder.UseSqlServer(connectionString);
#endif
            return new DatabaseContext(optionsBuilder.Options);
        }

        protected static string GetSettingFileFromEnvironment()
        {
            var netcoreEnvironment = System.Environment.GetEnvironmentVariable(NetcoreEnvironmentEnvVariable);

            return netcoreEnvironment != null
                       ? string.Format(ParametrizedApplicationSettingsFile, netcoreEnvironment)
                       : DefaultApplicationSettingsFile;
        }

        #endregion
    }
}
