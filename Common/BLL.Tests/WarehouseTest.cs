using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.Common.BLL.Tests
{
    [TestClass]
    public class WarehouseTest
    {
        #region Fields

        private DataModels.Aisle aisle1;
        private DataModels.Area area1;
        private DataModels.Bay bay1;
        private DataModels.Cell cell1;
        private DataModels.Item itemFifo;
        private DataModels.LoadingUnit loadingUnit1;

        #endregion Fields

        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            using (var context = this.CreateContext())
            {
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN a compartment in a specific area, associated to a specific item \
                AND a withdrawal request for the given item on a bay of the specified area \
               WHEN a new request for the same item and area is made \
               THEN the new request should be accepted")]
        public async Task CompartmentsInBay()
        {
            #region Arrange

            var now = System.DateTime.Now;

            var compartment1 = new DataModels.Compartment
            {
                Id = 1,
                ItemId = this.itemFifo.Id,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
                FirstStoreDate = now.AddDays(-1)
            };

            var compartment2 = new DataModels.Compartment
            {
                Id = 2,
                ItemId = this.itemFifo.Id,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
                FirstStoreDate = now.AddDays(-2)
            };

            var request1 = new DataModels.SchedulerRequest
            {
                Id = 1,
                ItemId = this.itemFifo.Id,
                IsInstant = true,
                RequestedQuantity = 20,
                OperationType = DataModels.OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SchedulerRequests.Add(request1);

                context.SaveChanges();
            }

            #endregion Arrange

            using (var context = this.CreateContext())
            {
                #region Act

                var warehouse = new Warehouse(
                    new DataProvider(context),
                    new SchedulerRequestProvider(context),
                    new Mock<ILogger<Warehouse>>().Object);

                var missions = await warehouse.DispatchRequests();

                #endregion Act

                #region Assert

                Assert.IsNotNull(missions);
                Assert.AreEqual(2, missions.Count());

                var missionsArray = missions.ToArray();
                Assert.AreEqual(compartment2.Id, missionsArray[0].CompartmentId);
                Assert.AreEqual(compartment1.Stock, missionsArray[0].Quantity);
                Assert.AreEqual(compartment2.Stock, missionsArray[1].Quantity);

                #endregion Assert
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            this.area1 = new DataModels.Area { Id = 1, Name = "Area #1" };
            this.aisle1 = new DataModels.Aisle { Id = 1, AreaId = this.area1.Id, Name = "Aisle #1" };
            this.cell1 = new DataModels.Cell { Id = 1, AisleId = this.aisle1.Id };
            this.loadingUnit1 = new DataModels.LoadingUnit { Id = 1, Code = "Loading Unit #1", CellId = this.cell1.Id };
            this.bay1 = new DataModels.Bay { Id = 1, Description = "Bay #1", AreaId = this.area1.Id, LoadingUnitsBufferSize = 2 };
            this.itemFifo = new DataModels.Item { Id = 1, Code = "Item #1", ManagementType = DataModels.ItemManagementType.FIFO };

            using (var context = this.CreateContext())
            {
                context.Areas.Add(this.area1);
                context.Aisles.Add(this.aisle1);
                context.Bays.Add(this.bay1);
                context.Cells.Add(this.cell1);
                context.LoadingUnits.Add(this.loadingUnit1);
                context.Items.Add(this.itemFifo);

                context.SaveChanges();
            }
        }

        private DatabaseContext CreateContext()
        {
            return new DatabaseContext(
                new DbContextOptionsBuilder<DatabaseContext>()
                    .UseInMemoryDatabase(databaseName: "test_database")
                    .Options
                );
        }

        #endregion Methods
    }
}
