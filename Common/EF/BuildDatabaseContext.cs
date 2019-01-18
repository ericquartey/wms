using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ferretto.Common.EF
{
    public class BuildDatabaseContext : DatabaseContext
    {
        #region Fields

        private const string buildNumberEnvVariable = "buildNumber";

        private static readonly System.Text.RegularExpressions.Regex ConnectionStringInitialCatalog =
            new System.Text.RegularExpressions.Regex(
                @"Initial Catalog=(?<dbname>[^;]+)",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        #endregion Fields

        #region Constructors

        public BuildDatabaseContext()
        {
        }

        public BuildDatabaseContext(DbContextOptions<DatabaseContext> options)
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

            var applicationSettingsFile = GetSettingFileFromEnvironment();

            var configurationBuilder = new ConfigurationBuilder()
                                       .SetBasePath(Directory.GetCurrentDirectory())
                                       .AddJsonFile(applicationSettingsFile, optional: false, reloadOnChange: false)
                                       .Build();

            var connectionString = configurationBuilder.GetConnectionString(ConnectionStringName);
            var databaseSuffix = Environment.GetEnvironmentVariable(buildNumberEnvVariable);
            Console.WriteLine($"Build Number: {databaseSuffix}");

            if (databaseSuffix == null)
            {
                throw new InvalidOperationException($"Environment variable {buildNumberEnvVariable} must be specified!");
            }

            var modifiedConnectionString = ConnectionStringInitialCatalog.Replace(
                connectionString,
                $"Initial Catalog=${{dbname}}_{databaseSuffix}");

            optionsBuilder.UseSqlServer(modifiedConnectionString);
            Console.WriteLine("Build db connection string:");
            Console.WriteLine(modifiedConnectionString);
        }

        #endregion Methods
    }
}
