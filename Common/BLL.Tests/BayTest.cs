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
    public class BayTest
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

        [TestInitialize]
        public void Initialize()
        {
            this.area1 = new DataModels.Area { Id = 1 };
            this.aisle1 = new DataModels.Aisle { Id = 1, AreaId = this.area1.Id };
            this.cell1 = new DataModels.Cell { Id = 1, AisleId = this.aisle1.Id };
            this.loadingUnit1 = new DataModels.LoadingUnit { Id = 1, CellId = this.cell1.Id };
            this.bay1 = new DataModels.Bay { Id = 1, AreaId = this.area1.Id, LoadingUnitsBufferSize = 2 };

            this.itemFifo = new DataModels.Item { Id = 1, ManagementType = DataModels.ItemManagementType.FIFO };

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

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN a request for an item on an area and a bay \
                AND a compartment that can satisfy the request \
                AND a bay that has a mission already assigned, but enough buffer to accept another mission \
               WHEN the request is processed \
               THEN a single mission is successfully created on the bay")]
        public async Task OneAvailableBay()
        {
            #region Arrange

            var now = System.DateTime.Now;

            var mission1 = new DataModels.Mission
            {
                BayId = this.bay1.Id,
                RequiredQuantity = 1,
                Status = DataModels.MissionStatus.New
            };

            var compartment1 = new DataModels.Compartment
            {
                ItemId = this.itemFifo.Id,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
                ReservedForPick = mission1.RequiredQuantity
            };

            var request1 = new DataModels.SchedulerRequest
            {
                ItemId = this.itemFifo.Id,
                AreaId = this.area1.Id,
                BayId = this.bay1.Id,
                IsInstant = true,
                RequestedQuantity = 5,
                OperationType = DataModels.OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Missions.Add(mission1);
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

                var missions = await warehouse.CreateMissionsForPendingRequests();

                #endregion Act

                #region Assert

                Assert.AreEqual(1, missions.Count());
                Assert.AreEqual(this.bay1.Id, missions.First().BayId);

                #endregion Assert
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN a request for an item on a bay \
                AND a compartment that can satisfy the request \
                AND the specified bay has no more buffer availability to accept a new mission \
               WHEN the request is processed \
               THEN no new missions are created")]
        public async Task OneFullBay()
        {
            #region Arrange

            var now = System.DateTime.Now;

            var request1 = new DataModels.SchedulerRequest
            {
                Id = 1,
                ItemId = this.itemFifo.Id,
                AreaId = this.area1.Id,
                BayId = this.bay1.Id,
                IsInstant = true,
                RequestedQuantity = 5,
                OperationType = DataModels.OperationType.Withdrawal
            };

            var mission1 = new DataModels.Mission
            {
                Id = 1,
                BayId = this.bay1.Id,
                RequiredQuantity = 1,
                Status = DataModels.MissionStatus.New
            };

            var mission2 = new DataModels.Mission
            {
                Id = 2,
                BayId = this.bay1.Id,
                RequiredQuantity = 1,
                Status = DataModels.MissionStatus.New
            };

            var compartment1 = new DataModels.Compartment
            {
                Id = 1,
                ItemId = this.itemFifo.Id,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 100,
                ReservedForPick = mission1.RequiredQuantity + mission2.RequiredQuantity
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Missions.Add(mission1);
                context.Missions.Add(mission2);
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

                var missions = await warehouse.CreateMissionsForPendingRequests();

                #endregion Act

                #region Assert

                Assert.IsFalse(missions.Any());

                #endregion Assert
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
