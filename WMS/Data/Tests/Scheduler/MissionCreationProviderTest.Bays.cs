using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests.Scheduler
{
    public partial class MissionCreationProviderTest
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

            var missionProvider = this.GetService<IMissionCreationProvider>();

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var mission1ReservedQty = 1;

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
                ReservedForPick = mission1ReservedQty
            };

            var mission1 = new Common.DataModels.Mission
            {
                BayId = this.Bay1Aisle1.Id,
                RequestedQuantity = mission1ReservedQty,
                Priority = 2,
                Status = Common.DataModels.MissionStatus.New,
                CompartmentId = compartment1.Id
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

            var missions = await missionProvider.CreateForRequestsAsync(requests);

            #endregion

            #region Assert

            Assert.AreEqual(1, requests.Count());
            Assert.AreEqual(1, missions.Count());
            Assert.AreEqual(this.Bay1Aisle1.Id, missions.First().BayId);

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

            var missionProvider = this.GetService<IMissionCreationProvider>();

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

            var mission1 = new Common.DataModels.Mission
            {
                Id = 1,
                BayId = this.Bay1Aisle1.Id,
                RequestedQuantity = 1,
                Status = Common.DataModels.MissionStatus.New
            };

            var mission2 = new Common.DataModels.Mission
            {
                Id = 2,
                BayId = this.Bay1Aisle1.Id,
                RequestedQuantity = 1,
                Status = Common.DataModels.MissionStatus.New
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
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

            var requests = await requestExecutionProvider.GetRequestsToProcessAsync();

            #endregion

            #region Act

            var missions = await missionProvider.CreateForRequestsAsync(requests);

            #endregion

            #region Assert

            Assert.IsFalse(missions.Any());

            #endregion
        }

        #endregion
    }
}
