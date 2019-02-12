using Ferretto.VW.MAS_DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MAS_DataLayerUnitTests
{
    [TestClass]
    public abstract class DBTestUnitTest
    {
        protected StatusLog StatusLog1 { get; set; }
        protected StatusLog StatusLog2 { get; set; }

        protected void CleanupDatabase()
        {
            using (var context = this.CreateContext())
            {
                context.Database.EnsureDeleted();
            }
        }

        protected DataLayerContext CreateContext()
        {
            return new DataLayerContext(
                new DbContextOptionsBuilder<DataLayerContext>()
                    .UseInMemoryDatabase(databaseName: this.GetType().FullName)
                    .Options);
        }

        protected virtual void InitializeDatabase()
        {
            this.StatusLog1 = new StatusLog { StatusLogId = 1, LogMessage = "Test Message #1" };
            this.StatusLog2 = new StatusLog { StatusLogId = 2, LogMessage = "Test Message #2" };

            using (var context = this.CreateContext())
            {
                context.StatusLogs.Add(this.StatusLog1);
                context.StatusLogs.Add(this.StatusLog2);

                context.SaveChanges();
            }
        }
    }
}
