using Microsoft.EntityFrameworkCore;

namespace MAS_DataLayer
{
    public class DataLayerContext : DbContext
    {
        #region Properties

        public DbSet<Operation> Operations { get; set; }

        public DbSet<Step> Steps { get; set; }

        #endregion Properties

        #region Methods

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=MachineAutomationService.db");
        }

        #endregion Methods
    }
}
