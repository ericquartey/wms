using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Scheduler.Tests
{
    public partial class MissionOperationCreationProviderTest
    {
        #region Methods

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a request for an item on an area and a bay \
                AND a compartment that can satisfy the request \
                AND a bay that has a mission already assigned, but enough buffer to accept another mission \
               WHEN the request is processed \
               THEN a single mission is successfully created on the bay")]
        public async Task CreateForRequestsAsync_OneAvailableBay()
        {
            #region Arrange

            var operationProvider = this.GetService<IMissionOperationCreationProvider>();
            var missionProvider = this.GetService<IMissionProvider>();

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var mission1ReservedQty = 1;

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
                ReservedForPick = mission1ReservedQty
            };

            var missionOperation1 = new Common.DataModels.MissionOperation
            {
                Id = 1,
                RequestedQuantity = mission1ReservedQty,
                ItemId = this.ItemFifo.Id,
                Priority = 2,
                Status = Common.DataModels.MissionOperationStatus.New,
                CompartmentId = compartment1.Id
            };

            var mission1 = new Common.DataModels.Mission
            {
                Id = 1,
                BayId = this.Bay1Aisle1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Priority = 2,
                Operations = new[] { missionOperation1 },
                Status = Common.DataModels.MissionStatus.New,
            };

            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = 1,
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                IsInstant = true,
                ItemId = this.ItemFifo.Id,
                OperationType = Common.DataModels.OperationType.Withdrawal,
                RequestedQuantity = 5,
                ReservedQuantity = 0,
                Priority = 2,
                Type = Common.DataModels.SchedulerRequestType.Item,
                Status = Common.DataModels.SchedulerRequestStatus.New,
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Missions.Add(mission1);
                context.SchedulerRequests.Add(request1);
                context.SaveChanges();
            }

            var requests = await requestExecutionProvider.GetRequestsToProcessAsync();

            #endregion

            #region Act

            var operations = await operationProvider.CreateForRequestsAsync(requests);

            #endregion

            #region Assert

            Assert.AreEqual(1, requests.Count());
            Assert.AreEqual(1, operations.Count());

            var operation = operations.First();
            var mission = await missionProvider.GetByIdAsync(operation.MissionId);

            Assert.AreEqual(this.Bay1Aisle1.Id, mission.BayId);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a request for an item on a bay \
                AND a compartment that can satisfy the request \
                AND the specified bay has no more buffer availability to accept a new mission \
               WHEN the request is processed \
               THEN no new missions are created")]
        public async Task CreateForRequestsAsync_OneFullBay()
        {
            #region Arrange

            var operationProvider = this.GetService<IMissionOperationCreationProvider>();

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                IsInstant = true,
                RequestedQuantity = 5,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            var missionOperation1 = new Common.DataModels.MissionOperation
            {
                Id = 1,
                RequestedQuantity = 1,
                Status = Common.DataModels.MissionOperationStatus.New,
                Type = Common.DataModels.MissionOperationType.Pick
            };

            var mission1 = new Common.DataModels.Mission
            {
                Id = 1,
                BayId = this.Bay1Aisle1.Id,
                Status = Common.DataModels.MissionStatus.New,
                Operations = new[] { missionOperation1 }
            };

            var missionOperation2 = new Common.DataModels.MissionOperation
            {
                Id = 2,
                RequestedQuantity = 1,
                Status = Common.DataModels.MissionOperationStatus.New,
                Type = Common.DataModels.MissionOperationType.Pick
            };

            var mission2 = new Common.DataModels.Mission
            {
                Id = 2,
                BayId = this.Bay1Aisle1.Id,
                Status = Common.DataModels.MissionStatus.New,
                Operations = new[] { missionOperation2 }
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 100,
                ReservedForPick = missionOperation1.RequestedQuantity + missionOperation2.RequestedQuantity
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Missions.Add(mission1);
                context.Missions.Add(mission2);
                context.SchedulerRequests.Add(request1);
                context.SaveChanges();
            }

            var requests = await requestExecutionProvider.GetRequestsToProcessAsync();
            if (requests == null || !requests.Any())
            {
                Assert.Inconclusive();
            }

            #endregion

            #region Act

            var operations = await operationProvider.CreateForRequestsAsync(requests);

            #endregion

            #region Assert

            Assert.IsFalse(operations.Any());

            #endregion
        }

        #endregion
    }
}
