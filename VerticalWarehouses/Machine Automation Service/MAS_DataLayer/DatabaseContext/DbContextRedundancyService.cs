using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer.DatabaseContext
{
    public class DbContextRedundancyService<TDbContext> : IDbContextRedundancyService<TDbContext>
        where TDbContext : DbContext, IRedundancyDbContext<TDbContext>
    {
        #region Fields

        private readonly IDictionary<DbContextOptions<TDbContext>, bool> isInhibited
            = new Dictionary<DbContextOptions<TDbContext>, bool>();

        #endregion

        #region Constructors

        public DbContextRedundancyService(
           string activeDbConnectionString,
           string standbyDbConnectionString)
        {
            if (string.IsNullOrWhiteSpace(activeDbConnectionString))
            {
                throw new System.ArgumentException(
                    "Connection string cannot be null or whitespace.",
                    nameof(activeDbConnectionString));
            }

            if (string.IsNullOrWhiteSpace(standbyDbConnectionString))
            {
                throw new System.ArgumentException(
                    "Connection string cannot be null or whitespace.",
                    nameof(standbyDbConnectionString));
            }

            this.ActiveDbContextOptions = new DbContextOptionsBuilder<TDbContext>()
                .UseSqlite(activeDbConnectionString)
                .Options;

            this.StandbyDbContextOptions = new DbContextOptionsBuilder<TDbContext>()
                .UseSqlite(standbyDbConnectionString)
                .Options;

            this.isInhibited.Add(this.ActiveDbContextOptions, false);
            this.isInhibited.Add(this.StandbyDbContextOptions, false);
        }

        #endregion

        #region Properties

        public DbContextOptions<TDbContext> ActiveDbContextOptions { get; private set; }

        public bool IsActiveDbInhibited => this.isInhibited[this.ActiveDbContextOptions];

        public bool IsStandbyDbInhibited => this.isInhibited[this.StandbyDbContextOptions];

        public DbContextOptions<TDbContext> StandbyDbContextOptions { get; private set; }

        #endregion

        #region Methods

        public void InhibitStanbyDb()
        {
            this.isInhibited[this.StandbyDbContextOptions] = true;
        }

        public void SwapContexts(bool inhibitActive = true)
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
