using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class DbContextRedundancyService<TDbContext> : IDbContextRedundancyService<TDbContext>
        where TDbContext : DbContext, IRedundancyDbContext<TDbContext>
    {
        #region Fields

        private readonly string activeDbConnectionString;

        private readonly IDictionary<DbContextOptions<TDbContext>, bool> isInhibited
                            = new Dictionary<DbContextOptions<TDbContext>, bool>();

        private readonly ILogger logger;

        private readonly string standbyDbConnectionString;

        private Exception lastSeenException;

        #endregion

        #region Constructors

        public DbContextRedundancyService(
           IConfiguration configuration,
           ILogger<DataLayerContext> logger)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = logger;

            this.activeDbConnectionString = configuration.GetDataLayerPrimaryConnectionString();
            this.standbyDbConnectionString = configuration.GetDataLayerSecondaryConnectionString();

            this.Initialize();
        }

        #endregion

        #region Properties

        public DbContextOptions<TDbContext> ActiveDbContextOptions { get; private set; }

        public bool IsActiveDbInhibited { get; private set; }

        public bool IsEnabled { get; set; }

        public bool IsStandbyDbInhibited { get; private set; }

        public DbContextOptions<TDbContext> StandbyDbContextOptions { get; private set; }

        private bool IsRedundancyConfigured => this.StandbyDbContextOptions != null;

        #endregion

        #region Methods

        public void HandleDbContextFault(DbContextOptions<TDbContext> dbContextOptions, Exception exception)
        {
            if (!this.IsRedundancyConfigured)
            {
                return;
            }

            if (dbContextOptions == null)
            {
                throw new ArgumentNullException(nameof(dbContextOptions));
            }

            if (exception != null)
            {
                if (exception == this.lastSeenException)
                {
                    return;
                }
                else
                {
                    this.lastSeenException = exception;
                }
            }

            if (dbContextOptions == this.ActiveDbContextOptions)
            {
                this.SwapContexts(exception);
            }
            else if (dbContextOptions == this.StandbyDbContextOptions)
            {
                this.logger.LogError(exception, "Operation failed on standby database.");
                if (!this.IsStandbyDbInhibited)
                {
                    this.InhibitStandbyDb();
                    this.logger.LogError("Standby database inhibited.");
                }
            }
            else
            {
                throw new ArgumentException(
                    "The specified database context options do not relate to the acrive nor to the standby database.",
                    nameof(dbContextOptions));
            }

            if (this.IsActiveDbInhibited)
            {
                this.logger.LogCritical(exception, "Active database operations inhibited because of previous errors.");
            }
        }

        public void InhibitStandbyDb()
        {
            if (!this.IsRedundancyConfigured)
            {
                return;
            }

            this.IsStandbyDbInhibited = true;
        }

        private void Initialize()
        {
            this.ActiveDbContextOptions = new DbContextOptionsBuilder<TDbContext>()
              .UseSqlite(this.activeDbConnectionString)
              .Options;
            this.IsActiveDbInhibited = false;

            if (string.IsNullOrWhiteSpace(this.standbyDbConnectionString))
            {
                this.IsStandbyDbInhibited = true;
                this.logger.LogWarning("No connection string specified for standby database. Database redundancy disabled.");
            }
            else
            {
                this.StandbyDbContextOptions = new DbContextOptionsBuilder<TDbContext>()
                    .UseSqlite(this.standbyDbConnectionString)
                    .Options;

                this.IsStandbyDbInhibited = false;
            }
        }

        private void SwapContexts(Exception exception)
        {
            this.IsActiveDbInhibited = true;

            if (this.IsStandbyDbInhibited)
            {
                this.logger.LogCritical(exception, "Unable to swap active with standby, because standby is inhibited (not usable).");
                return;
            }

            this.logger.LogError(exception, "Operation failed on active database. Active database swapped with standby.");

            var newActiveDbContextOptions = this.StandbyDbContextOptions;

            this.StandbyDbContextOptions = this.ActiveDbContextOptions;
            this.ActiveDbContextOptions = newActiveDbContextOptions;
        }

        #endregion
    }
}
