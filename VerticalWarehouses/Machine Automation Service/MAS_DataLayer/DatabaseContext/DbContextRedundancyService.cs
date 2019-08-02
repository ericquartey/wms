using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer.DatabaseContext
{
    public class DbContextRedundancyService<TDbContext> : IDbContextRedundancyService<TDbContext>
        where TDbContext : DbContext, IRedundancyDbContext<TDbContext>
    {
        #region Fields

        private readonly IDictionary<DbContextOptions<TDbContext>, bool> isInhibited
            = new Dictionary<DbContextOptions<TDbContext>, bool>();

        private readonly ILogger logger;

        private Exception lastSeenException;

        #endregion

        #region Constructors

        public DbContextRedundancyService(
           string activeDbConnectionString,
           string standbyDbConnectionString,
           ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(activeDbConnectionString))
            {
                throw new ArgumentException(
                    "Connection string cannot be null or whitespace.",
                    nameof(activeDbConnectionString));
            }

            if (string.IsNullOrWhiteSpace(standbyDbConnectionString))
            {
                throw new ArgumentException(
                    "Connection string cannot be null or whitespace.",
                    nameof(standbyDbConnectionString));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.ActiveDbContextOptions = new DbContextOptionsBuilder<TDbContext>()
                .UseSqlite(activeDbConnectionString)
                .Options;

            this.StandbyDbContextOptions = new DbContextOptionsBuilder<TDbContext>()
                .UseSqlite(standbyDbConnectionString)
                .Options;

            this.isInhibited.Add(this.ActiveDbContextOptions, false);
            this.isInhibited.Add(this.StandbyDbContextOptions, false);
            this.logger = logger;
        }

        #endregion

        #region Properties

        public DbContextOptions<TDbContext> ActiveDbContextOptions { get; private set; }

        public bool IsActiveDbInhibited => this.isInhibited[this.ActiveDbContextOptions];

        public bool IsStandbyDbInhibited => this.isInhibited[this.StandbyDbContextOptions];

        public DbContextOptions<TDbContext> StandbyDbContextOptions { get; private set; }

        #endregion

        #region Methods

        public void HandleDbContextFault(DbContextOptions<TDbContext> dbContextOptions, Exception exception)
        {
            if (dbContextOptions == null)
            {
                throw new ArgumentNullException(nameof(dbContextOptions));
            }

            if (exception != null)
            {
                if (exception == this.lastSeenException)
                {
                    this.logger.LogWarning("Exception already seen, skipping ...");
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
            this.isInhibited[this.StandbyDbContextOptions] = true;
        }

        private void SwapContexts(Exception exception)
        {
            this.isInhibited[this.ActiveDbContextOptions] = true;

            if (this.isInhibited[this.StandbyDbContextOptions])
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
