using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Scheduler.Tests
{
    public partial class MissionCreationProviderTest
    {
        #region Methods

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a new request for an item on a bay \
                AND another request that was already completed \
               WHEN the new request is processed \
               THEN a new mission is successfully created")]
        public async Task CreateForRequestsAsync_OneCompletedRequest()
        {
            #region Arrange

            var missionProvider = this.GetService<IMissionCreationProvider>();

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10
            };

            var request1 = new Common.DataModels.SchedulerRequest
            {
                ItemId = this.ItemFifo.Id,
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                IsInstant = true,
                RequestedQuantity = 15,
                ReservedQuantity = 15,
                Status = Common.DataModels.SchedulerRequestStatus.Completed,
                Priority = 1,
                Type = Common.DataModels.SchedulerRequestType.Item,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            var request2 = new Common.DataModels.SchedulerRequest
            {
                ItemId = this.ItemFifo.Id,
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                IsInstant = true,
                RequestedQuantity = 6,
                ReservedQuantity = 0,
                Status = Common.DataModels.SchedulerRequestStatus.New,
                Priority = 1,
                Type = Common.DataModels.SchedulerRequestType.Item,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.SchedulerRequests.Add(request1);
                context.SchedulerRequests.Add(request2);
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
            Assert.AreEqual(request2.RequestedQuantity, missions.First().RequestedQuantity);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a request for an item on a bay \
                AND two compartments that together can satisfy the request \
               WHEN the request is processed \
               THEN the total dispatched quantity recorded in the request should be equal to the originally requested \
                AND two missions should be generated \
                AND the total quantity of the two missions should be as much as the requested quantity \
                AND the mission associated to the compartment with older items should be the one from which all the stocked quantity is taken")]
        public async Task CreateForRequestsAsync_OneRequestOnTwoCompartments()
        {
            #region Arrange

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var missionProvider = this.GetService<IMissionCreationProvider>();

            var now = System.DateTime.Now;

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
                FifoStartDate = now.AddDays(-0.5)
            };

            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
                FifoStartDate = now.AddDays(-2)
            };

            var request1 = new Common.DataModels.SchedulerRequest
            {
                ItemId = this.ItemFifo.Id,
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                IsInstant = true,
                RequestedQuantity = 15,
                ReservedQuantity = 0,
                Priority = 1,
                Status = Common.DataModels.SchedulerRequestStatus.New,
                Type = Common.DataModels.SchedulerRequestType.Item,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SchedulerRequests.Add(request1);
                context.SaveChanges();
            }

            var requests = await requestExecutionProvider.GetRequestsToProcessAsync();

            #endregion

            #region Act

            var missions = await missionProvider.CreateForRequestsAsync(requests);

            #endregion

            #region Assert

            using (var context = this.CreateContext())
            {
                Assert.AreEqual(2, missions.Count());

                var updatedRequest = context.SchedulerRequests.Single(r => r.Id == request1.Id);
                Assert.AreEqual(updatedRequest.RequestedQuantity, missions.Sum(m => m.RequestedQuantity));
                Assert.AreEqual(updatedRequest.RequestedQuantity, updatedRequest.ReservedQuantity);
            }

            Assert.AreEqual(compartment2.Id, missions.First().CompartmentId);
            Assert.AreEqual(compartment2.Stock, missions.First().RequestedQuantity);

            #endregion
        }

        #endregion
    }
}
