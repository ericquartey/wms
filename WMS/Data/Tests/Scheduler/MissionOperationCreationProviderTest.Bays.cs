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

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var mission1ReservedQty = 1;

            var compartment = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                ReservedForPick = mission1ReservedQty,
                Stock = 10,
            };

            var mission = new Common.DataModels.Mission
            {
                Id = GetNewId(),
                BayId = this.Bay1Aisle1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Priority = 2,
            };

            var existingOperation = new Common.DataModels.MissionOperation
            {
                Id = GetNewId(),
                RequestedQuantity = mission1ReservedQty,
                ItemId = this.ItemFifo.Id,
                Priority = 2,
                Status = Common.DataModels.MissionOperationStatus.New,
                CompartmentId = compartment.Id,
                MissionId = mission.Id,
            };

            var request = new Common.DataModels.SchedulerRequest
            {
                Id = GetNewId(),
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
                context.Compartments.Add(compartment);
                context.MissionOperations.Add(existingOperation);
                context.Missions.Add(mission);
                context.SchedulerRequests.Add(request);
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
            using (var context = this.CreateContext())
            {
                var updatedMission = context.Missions.Single(m => m.Id == operation.MissionId);
                Assert.IsNotNull(updatedMission);

                Assert.AreEqual(this.Bay1Aisle1.Id, updatedMission.BayId);
            }

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

            var bayId = this.Bay1Aisle1.Id;

            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = GetNewId(),
                ItemId = this.ItemFifo.Id,
                AreaId = this.Area1.Id,
                BayId = bayId,
                IsInstant = true,
                RequestedQuantity = 5,
                ReservedQuantity = 0,
                Priority = this.Bay1Aisle1.Priority,
                Type = Common.DataModels.SchedulerRequestType.Item,
                Status = Common.DataModels.SchedulerRequestStatus.New,
                OperationType = Common.DataModels.OperationType.Withdrawal,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
            };

            var missionOperation1 = new Common.DataModels.MissionOperation
            {
                Id = GetNewId(),
                RequestedQuantity = 1,
                Status = Common.DataModels.MissionOperationStatus.New,
                Type = Common.DataModels.MissionOperationType.Pick
            };

            var mission1 = new Common.DataModels.Mission
            {
                Id = GetNewId(),
                BayId = bayId,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Operations = new[] { missionOperation1 }
            };

            var missionOperation2 = new Common.DataModels.MissionOperation
            {
                Id = GetNewId(),
                RequestedQuantity = 1,
                Status = Common.DataModels.MissionOperationStatus.New,
                Type = Common.DataModels.MissionOperationType.Pick
            };

            var mission2 = new Common.DataModels.Mission
            {
                Id = GetNewId(),
                BayId = bayId,
                Operations = new[] { missionOperation2 }
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 100,
                ReservedForPick =
                    missionOperation1.RequestedQuantity +
                    missionOperation2.RequestedQuantity
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
            if (!requests.Any())
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
