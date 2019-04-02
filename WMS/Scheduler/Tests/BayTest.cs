using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Scheduler.Tests
{
    [TestClass]
    public class BayTest : BaseWarehouseTest
    {
        #region Fields

        private Common.DataModels.Aisle aisle1;

        private Common.DataModels.Area area1;

        private Common.DataModels.Bay bay1;

        private Common.DataModels.Cell cell1;

        private Common.DataModels.Item itemFifo;

        private Common.DataModels.LoadingUnit loadingUnit1;

        #endregion

        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestInitialize]
        public void Initialize()
        {
            this.area1 = new Common.DataModels.Area { Id = 1 };
            this.aisle1 = new Common.DataModels.Aisle { Id = 1, AreaId = this.area1.Id };
            this.cell1 = new Common.DataModels.Cell { Id = 1, AisleId = this.aisle1.Id };
            this.loadingUnit1 = new Common.DataModels.LoadingUnit { Id = 1, CellId = this.cell1.Id };
            this.bay1 = new Common.DataModels.Bay { Id = 1, AreaId = this.area1.Id, LoadingUnitsBufferSize = 2 };

            this.itemFifo = new Common.DataModels.Item { Id = 1, ManagementType = Common.DataModels.ItemManagementType.FIFO };

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
        [TestProperty(
            "Description",
            @"GIVEN a request for an item on an area and a bay \
                AND a compartment that can satisfy the request \
                AND a bay that has a mission already assigned, but enough buffer to accept another mission \
               WHEN the request is processed \
               THEN a single mission is successfully created on the bay")]
        public async Task OneAvailableBay()
        {
            #region Arrange

            var missionProvider = this.GetService<IMissionSchedulerProvider>();

            var requestProvider = this.GetService<ISchedulerRequestProvider>();

            var mission1 = new Common.DataModels.Mission
            {
                BayId = this.bay1.Id,
                RequestedQuantity = 1,
                Status = Common.DataModels.MissionStatus.New
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = this.itemFifo.Id,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
                ReservedForPick = mission1.RequestedQuantity
            };

            var request1 = new Common.DataModels.SchedulerRequest
            {
                ItemId = this.itemFifo.Id,
                AreaId = this.area1.Id,
                BayId = this.bay1.Id,
                IsInstant = true,
                RequestedQuantity = 5,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Missions.Add(mission1);
                context.SchedulerRequests.Add(request1);

                context.SaveChanges();
            }

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act

                var requests = await requestProvider.GetRequestsToProcessAsync();
                var missions = await missionProvider.CreateForRequestsAsync(requests);

                #endregion

                #region Assert

                Assert.AreEqual(1, missions.Count());
                Assert.AreEqual(this.bay1.Id, missions.First().BayId);

                #endregion
            }
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a request for an item on a bay \
                AND a compartment that can satisfy the request \
                AND the specified bay has no more buffer availability to accept a new mission \
               WHEN the request is processed \
               THEN no new missions are created")]
        public async Task OneFullBay()
        {
            #region Arrange

            var missionProvider = this.GetService<IMissionSchedulerProvider>();

            var requestProvider = this.GetService<ISchedulerRequestProvider>();

            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = 1,
                ItemId = this.itemFifo.Id,
                AreaId = this.area1.Id,
                BayId = this.bay1.Id,
                IsInstant = true,
                RequestedQuantity = 5,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            var mission1 = new Common.DataModels.Mission
            {
                Id = 1,
                BayId = this.bay1.Id,
                RequestedQuantity = 1,
                Status = Common.DataModels.MissionStatus.New
            };

            var mission2 = new Common.DataModels.Mission
            {
                Id = 2,
                BayId = this.bay1.Id,
                RequestedQuantity = 1,
                Status = Common.DataModels.MissionStatus.New
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.itemFifo.Id,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 100,
                ReservedForPick = mission1.RequestedQuantity + mission2.RequestedQuantity
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Missions.Add(mission1);
                context.Missions.Add(mission2);
                context.SchedulerRequests.Add(request1);

                context.SaveChanges();
            }

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act

                var requests = await requestProvider.GetRequestsToProcessAsync();
                var missions = await missionProvider.CreateForRequestsAsync(requests);

                #endregion

                #region Assert

                Assert.IsFalse(missions.Any());

                #endregion
            }
        }

        #endregion
    }
}
