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
            @"GIVEN a request for an item on an area \
                AND a compartment that can satisfy the request \
                AND two bays, one of which has a mission already assigned, while the other one has none \
               WHEN the request is processed \
               THEN a new mission is created on the bay that has no missions on it")]
        public async Task MultipleBays()
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
                IsInstant = true,
                RequestedQuantity = 5,
                OperationType = DataModels.OperationType.Withdrawal
            };

            var bay2 = new DataModels.Bay
            {
                Id = 2,
                AreaId = this.area1.Id,
                LoadingUnitsBufferSize = 2
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Bays.Add(bay2);
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

                var missions = await warehouse.DispatchRequests();

                #endregion Act

                #region Assert

                Assert.AreEqual(1, missions.Count());
                Assert.AreEqual(bay2.Id, missions.First().BayId);

                #endregion Assert
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN a new request for an item on a bay \
                AND another request that was already completed \
               WHEN the new request is processed \
               THEN ")]
        public async Task OneCompletedRequest()
        {
            #region Arrange

            var now = System.DateTime.Now;

            var compartment1 = new DataModels.Compartment
            {
                ItemId = this.itemFifo.Id,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10
            };

            var request1 = new DataModels.SchedulerRequest
            {
                ItemId = this.itemFifo.Id,
                AreaId = this.area1.Id,
                BayId = this.bay1.Id,
                IsInstant = true,
                RequestedQuantity = 15,
                DispatchedQuantity = 15,
                OperationType = DataModels.OperationType.Withdrawal
            };

            var request2 = new DataModels.SchedulerRequest
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
                context.SchedulerRequests.Add(request1);
                context.SchedulerRequests.Add(request2);

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

                Assert.AreEqual(1, missions.Count());
                Assert.AreEqual(request2.RequestedQuantity, missions.First().Quantity);

                #endregion Assert
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN a request for an item on a bay \
                AND a compartment that can satisfy the request \
                AND a bay with no more buffer availability to accept a new mission \
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

                var missions = await warehouse.DispatchRequests();

                #endregion Act

                #region Assert

                Assert.IsFalse(missions.Any());

                #endregion Assert
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN a request for an item on a bay \
                AND two compartments that together can satisfy the request \
               WHEN the request is processed \
               THEN the total dispatched quantity recorded in the request should be equal to the originally requested \
                AND two missions should be generated \
                AND the total quantity of the two missions should be as much as the requested quantity \
                AND the mission associated to the compartment with older items should be the one from which all the stocked quantity is taken")]
        public async Task OneRequestOnTwoCompartments()
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
                ItemId = this.itemFifo.Id,
                AreaId = this.area1.Id,
                BayId = this.bay1.Id,
                IsInstant = true,
                RequestedQuantity = 15,
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

                Assert.AreEqual(2, missions.Count());

                var updatedRequest = context.SchedulerRequests.Single(r => r.Id == request1.Id);
                Assert.AreEqual(updatedRequest.RequestedQuantity, missions.Sum(m => m.Quantity));
                Assert.AreEqual(updatedRequest.RequestedQuantity, updatedRequest.DispatchedQuantity);
                Assert.AreEqual(compartment2.Id, missions.First().CompartmentId);
                Assert.AreEqual(compartment2.Stock, missions.First().Quantity);

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
