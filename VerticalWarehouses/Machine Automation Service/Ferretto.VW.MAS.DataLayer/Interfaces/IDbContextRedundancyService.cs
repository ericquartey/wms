using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IDbContextRedundancyService<TDbContext>
        where TDbContext : DbContext, IRedundancyDbContext<TDbContext>
    {
        #region Properties

        DbContextOptions<TDbContext> ActiveDbContextOptions { get; }

        DbContextOptions<TDbContext> StandbyDbContextOptions { get; }

        DbContextOptions<TDbContext> DefaultDbContextOptions { get; }

        bool IsActiveDbInhibited { get; }

        bool IsStandbyDbInhibited { get; }

        bool IsDefaultDbInhibited { get; }

        bool IsEnabled { get; set; }

        #endregion

        #region Methods

        void HandleDbContextFault(
            DbContextOptions<TDbContext> dbContextOptions,
            System.Exception exception);

        void InhibitStandbyDb();

        #endregion
    }
}
