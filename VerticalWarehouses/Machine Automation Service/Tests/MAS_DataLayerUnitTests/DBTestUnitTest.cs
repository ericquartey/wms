using Ferretto.VW.MAS_DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MAS_DataLayerUnitTests
{
    [TestClass]
    public abstract class DBUnitTest
    {
        #region Properties

        protected StatusLog StatusLog1 { get; set; }

        protected StatusLog StatusLog2 { get; set; }

        #endregion

        #region Methods

        protected void CleanupDatabase()
        {
            using (var context = this.CreateContext()) context.Database.EnsureDeleted();
        }

        protected DataLayerContext CreateContext()
        {
            return new DataLayerContext(
                new DbContextOptionsBuilder<DataLayerContext>()
                    .UseInMemoryDatabase(this.GetType().FullName)
                    .Options);
        }

        protected virtual void InitializeDatabase()
        {
            this.StatusLog1 = new StatusLog {StatusLogId = 1, LogMessage = "Test Message #1"};
            this.StatusLog2 = new StatusLog {StatusLogId = 2, LogMessage = "Test Message #2"};

            using (var context = this.CreateContext())
            {
                context.StatusLogs.Add(this.StatusLog1);
                context.StatusLogs.Add(this.StatusLog2);

                context.SaveChanges();
            }
        }

        #endregion
    }
}
