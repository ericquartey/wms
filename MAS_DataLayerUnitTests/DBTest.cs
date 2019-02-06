using Microsoft.EntityFrameworkCore;

namespace MAS_DataLayerUnitTests
{
    public abstract class DBTest
    {

        protected DatabaseContext CreateContext()
        {
            return new DatabaseContext(
                new DbContextOptionsBuilder<DatabaseContext>()
                    .UseInMemoryDatabase(databaseName: this.GetType().FullName)
                    .Options);
        }

        protected virtual void InitializeDatabase()
        {

            using (var context = this.CreateContext())
            {
                context.Areas.Add(this.Area1);

                context.SaveChanges();
            }
        }
    }
}
