using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IDbContextRedundancyService<TDbContext>
        where TDbContext : DbContext, IRedundancyDbContext<TDbContext>
    {
        #region Properties

        DbContextOptions<TDbContext> ActiveDbContextOptions { get; }

        bool IsActiveDbInhibited { get; }

        bool IsStandbyDbInhibited { get; }

        DbContextOptions<TDbContext> StandbyDbContextOptions { get; }

        #endregion

        #region Methods

        void HandleDbContextFault(TDbContext dbContext, System.Exception exception);

        #endregion
    }
}
