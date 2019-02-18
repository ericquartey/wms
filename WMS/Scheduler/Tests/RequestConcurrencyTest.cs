using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Scheduler.Tests
{
    [TestClass]
    public class RequestConcurrencyTest : BaseWarehouseTest
    {
        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a new request for an item on a bay \
                AND another request that was already completed \
               WHEN the new request is processed \
               THEN a new mission is successfully created")]
        public async Task OneCompletedRequest()
        {
            #region Arrange

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                Stock = 10
            };

            var request1 = new Common.DataModels.SchedulerRequest
            {
                ItemId = this.ItemFifo.Id,
                AreaId = this.Area1.Id,
                BayId = this.Bay1.Id,
                IsInstant = true,
                RequestedQuantity = 15,
                DispatchedQuantity = 15,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            var request2 = new Common.DataModels.SchedulerRequest
            {
                ItemId = this.ItemFifo.Id,
                AreaId = this.Area1.Id,
                BayId = this.Bay1.Id,
                IsInstant = true,
                RequestedQuantity = 5,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.SchedulerRequests.Add(request1);
                context.SchedulerRequests.Add(request2);

                context.SaveChanges();
            }

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act

                var missionProvider = this.ServiceProvider.GetService(typeof(IMissionSchedulerProvider)) as IMissionSchedulerProvider;

                var missions = await missionProvider.CreateForPendingRequestsAsync();

                #endregion

                #region Assert

                Assert.AreEqual(1, missions.Count());
                Assert.AreEqual(request2.RequestedQuantity, missions.First().Quantity);

                #endregion
            }
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
        public async Task OneRequestOnTwoCompartments()
        {
            #region Arrange

            var now = System.DateTime.Now;

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                Stock = 10,
                FirstStoreDate = now.AddDays(-1)
            };

            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                Stock = 10,
                FirstStoreDate = now.AddDays(-2)
            };

            var request1 = new Common.DataModels.SchedulerRequest
            {
                ItemId = this.ItemFifo.Id,
                AreaId = this.Area1.Id,
                BayId = this.Bay1.Id,
                IsInstant = true,
                RequestedQuantity = 15,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SchedulerRequests.Add(request1);

                context.SaveChanges();
            }

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act

                var warehouse = new Warehouse(
                    new DataProvider(context),
                    new SchedulerRequestProvider(context),
                    new Mock<ILogger<Warehouse>>().Object);

                var missions = await warehouse.CreateMissionsForPendingRequestsAsync();

                #endregion

                #region Assert

                Assert.AreEqual(2, missions.Count());

                var updatedRequest = context.SchedulerRequests.Single(r => r.Id == request1.Id);
                Assert.AreEqual(updatedRequest.RequestedQuantity, missions.Sum(m => m.Quantity));
                Assert.AreEqual(updatedRequest.RequestedQuantity, updatedRequest.DispatchedQuantity);
                Assert.AreEqual(compartment2.Id, missions.First().CompartmentId);
                Assert.AreEqual(compartment2.Stock, missions.First().Quantity);

                #endregion
            }
        }

        #endregion
    }
}
