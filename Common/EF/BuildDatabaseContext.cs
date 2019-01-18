using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ferretto.Common.EF
{
    public class BuildDatabaseContext : DatabaseContext
    {
        private const string buildNumberEnvVariable = "buildNumber";
        private static readonly System.Text.RegularExpressions.Regex ConnectionStringInitialCatalog =
            new System.Text.RegularExpressions.Regex(
                @"Initial Catalog=(?<dbname>[^;]+)",
                System.Text.RegularExpressions.RegexOptions.Compiled);

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

            var configurationBuilder = new ConfigurationBuilder()
                                       .SetBasePath(Directory.GetCurrentDirectory())
                                       .AddJsonFile(DefaultApplicationSettingsFile, optional: false, reloadOnChange: false)
                                       .Build();

            var connectionString = configurationBuilder.GetConnectionString(ConnectionStringName);

            var databaseSuffix = Environment.GetEnvironmentVariable(buildNumberEnvVariable);
            var modifiedConnectionString = ConnectionStringInitialCatalog.Replace(
                connectionString,
                $"${{dbname}}_{databaseSuffix}");

            optionsBuilder.UseSqlServer(modifiedConnectionString);
            Console.WriteLine("Build db connection string:");
            Console.WriteLine(modifiedConnectionString);
        }

        #endregion Methods
    }
}
