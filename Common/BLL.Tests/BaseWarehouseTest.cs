using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.Common.BLL.Tests
{
    [TestClass]
    public abstract class BaseWarehouseTest
    {
        #region Fields

        protected DataModels.Aisle aisle1;
        protected DataModels.Area area1;
        protected DataModels.Bay bay1;
        protected DataModels.Cell cell1;
        protected DataModels.Item item1;
        protected DataModels.Item itemFifo;
        protected DataModels.Item itemVolume;
        protected DataModels.LoadingUnit loadingUnit1;

        #endregion Fields

        #region Methods

        [TestCleanup]
        protected virtual void CleanupDatabase()
        {
            using (var context = this.CreateContext())
            {
                context.Database.EnsureDeleted();
            }
        }

        protected DatabaseContext CreateContext()
        {
            return new DatabaseContext(
                new DbContextOptionsBuilder<DatabaseContext>()
                    .UseInMemoryDatabase(databaseName: this.GetType().FullName)
                    .Options
                );
        }

        [TestInitialize]
        protected virtual void InitializeDatabase()
        {
            this.area1 = new DataModels.Area { Id = 1, Name = "Area #1" };
            this.aisle1 = new DataModels.Aisle { Id = 1, AreaId = this.area1.Id, Name = "Aisle #1" };
            this.cell1 = new DataModels.Cell { Id = 1, AisleId = this.aisle1.Id };
            this.loadingUnit1 = new DataModels.LoadingUnit { Id = 1, Code = "Loading Unit #1", CellId = this.cell1.Id };
            this.bay1 = new DataModels.Bay { Id = 1, Description = "Bay #1", AreaId = this.area1.Id, LoadingUnitsBufferSize = 2 };
            this.item1 = new DataModels.Item { Id = 1, Code = "Item #1", ManagementType = DataModels.ItemManagementType.FIFO };
            this.itemFifo = new DataModels.Item { Id = 2, Code = "Item #2", ManagementType = DataModels.ItemManagementType.FIFO };
            this.itemVolume = new DataModels.Item { Id = 3, Code = "Item #3", ManagementType = DataModels.ItemManagementType.Volume };

            using (var context = this.CreateContext())
            {
                context.Areas.Add(this.area1);
                context.Aisles.Add(this.aisle1);
                context.Bays.Add(this.bay1);
                context.Cells.Add(this.cell1);
                context.LoadingUnits.Add(this.loadingUnit1);
                context.Items.Add(this.item1);
                context.Items.Add(this.itemFifo);
                context.Items.Add(this.itemVolume);

                context.SaveChanges();
            }
        }

        #endregion Methods
    }
}
