using Ferretto.VW.MAS_DataLayer;
using Microsoft.EntityFrameworkCore;

namespace MAS_DataLayerUnitTests
{
    public abstract class DBTest
    {

        protected DataLayerContext CreateContext()
        {
            return new DataLayerContext(
                new DbContextOptionsBuilder<DataLayerContext>()
                    .UseInMemoryDatabase(databaseName: this.GetType().FullName)
                    .Options);
        }

        protected virtual void InitializeDatabase()
        {

            using (var context = this.CreateContext())
            {

                context.SaveChanges();
            }
        }
    }
}
