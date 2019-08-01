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

        public void HandleDbContextFault(TDbContext dbContext, Exception exception)
        {
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

            if (dbContext.Options == this.ActiveDbContextOptions)
            {
                this.SwapContexts();
                this.logger.LogError(exception, "Operation failed on active database. Active database swapped with standby.");
            }
            else if (dbContext.Options == this.StandbyDbContextOptions)
            {
                this.logger.LogError(exception, "Operation failed on standby database.");
                if (!this.IsStandbyDbInhibited)
                {
                    this.InhibitStanbyDb();
                    this.logger.LogError(exception, "Standby database inhibited.");
                }
            }
            if (this.IsActiveDbInhibited)
            {
                this.logger.LogCritical(exception, "Active database operations inhibited because of previous errors.");
            }
        }

        private void InhibitStanbyDb()
        {
            this.isInhibited[this.StandbyDbContextOptions] = true;
        }

        private void SwapContexts(bool inhibitActive = true)
        {
            if (inhibitActive)
            {
                this.isInhibited[this.ActiveDbContextOptions] = true;
            }

            var newActiveDbContextOptions = this.StandbyDbContextOptions;

            this.StandbyDbContextOptions = this.ActiveDbContextOptions;
            this.ActiveDbContextOptions = newActiveDbContextOptions;
        }

        #endregion
    }
}
