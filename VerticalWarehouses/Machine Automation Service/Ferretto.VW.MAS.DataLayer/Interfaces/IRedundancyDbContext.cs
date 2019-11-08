using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IRedundancyDbContext<TDbContext>
        where TDbContext : DbContext
    {
        #region Properties

        DbContextOptions<TDbContext> Options { get; }

        #endregion
    }
}
