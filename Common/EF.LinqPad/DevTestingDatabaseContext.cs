using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.EF.LinqPad
{
    public class DevTestingDatabaseContext : DatabaseContext
    {
        #region Constructors

        public DevTestingDatabaseContext(string connectionString)
            : base(new DbContextOptionsBuilder<DatabaseContext>().UseSqlServer(connectionString).Options)
        {
        }

        #endregion
    }
}
