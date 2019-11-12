using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IDbContextRedundancyService<TDbContext>
        where TDbContext : DbContext, IRedundancyDbContext<TDbContext>
    {
        #region Properties

        DbContextOptions<TDbContext> ActiveDbContextOptions { get; }

        bool IsActiveDbInhibited { get; }

        bool IsEnabled { get; set; }

        bool IsStandbyDbInhibited { get; }

        DbContextOptions<TDbContext> StandbyDbContextOptions { get; }

        #endregion

        #region Methods

        void HandleDbContextFault(
            DbContextOptions<TDbContext> dbContextOptions,
            System.Exception exception);

        void InhibitStandbyDb();

        #endregion
    }
}
