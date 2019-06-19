using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Scheduler.Tests
{
    [TestClass]
    public partial class MissionCreationProviderTest : BaseWarehouseTest
    {
        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestMethod]
        public async Task CreateForRequestsAsync_MultiplePutMissions()
        {
            #region Arrange

            var missionProvider = this.GetService<IMissionCreationProvider>();

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = this.ItemVolume.Id,
                CompartmentTypeId = compartmentType1.Id,
                MaxCapacity = 10,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                CompartmentTypeId = 1,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 4,
            };
            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                CompartmentTypeId = 1,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 6,
            };
            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = 1,
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                IsInstant = true,
                ItemId = this.ItemVolume.Id,
                OperationType = Common.DataModels.OperationType.Insertion,
                RequestedQuantity = 10,
                ReservedQuantity = 0,
                Priority = 2,
                Type = Common.DataModels.SchedulerRequestType.Item,
                Status = Common.DataModels.SchedulerRequestStatus.New,
            };

            using (var context = this.CreateContext())
            {
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
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
                var request1After = context.SchedulerRequests.First();

                Assert.AreEqual(
                    Common.DataModels.SchedulerRequestStatus.Completed,
                    request1After.Status,
                    "The request should be completed");
            }

            Assert.AreEqual(
                2,
                missions.Count(),
                "Two mission should be generated");
            Assert.IsTrue(
                missions.Any(m => m.CompartmentId == compartment1.Id),
                "We should generate a mission for each compartment");
            Assert.IsTrue(
                missions.Any(m => m.CompartmentId == compartment2.Id),
                "We should generate a mission for each compartment");
            Assert.AreEqual(
                request1.RequestedQuantity,
                missions.Sum(m => m.RequestedQuantity),
                "The mission should take the full request quantity");

            Assert.IsTrue(
                missions.All(m => m.Type == MissionType.Put),
                "A type of mission should be Put");
            Assert.IsTrue(
                missions.All(m => m.Status == MissionStatus.New),
                "A status of mission should be New");
            Assert.IsTrue(
                missions.All(m => m.ItemId == this.ItemVolume.Id),
                "The mission should be on the right item");

            #endregion
        }

        [TestMethod]
        public async Task CreateForRequestsAsync_SinglePutMissionCompleteRequest()
        {
            #region Arrange

            var missionProvider = this.GetService<IMissionCreationProvider>();

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = this.ItemVolume.Id,
                CompartmentTypeId = compartmentType1.Id,
                MaxCapacity = 10,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                CompartmentTypeId = 1,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 0,
            };
            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = 1,
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                IsInstant = true,
                ItemId = this.ItemVolume.Id,
                OperationType = Common.DataModels.OperationType.Insertion,
                RequestedQuantity = 5,
                ReservedQuantity = 0,
                Priority = 2,
                Type = Common.DataModels.SchedulerRequestType.Item,
                Status = Common.DataModels.SchedulerRequestStatus.New,
            };

            using (var context = this.CreateContext())
            {
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
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
                var request1After = context.SchedulerRequests.First();

                Assert.AreEqual(
                    Common.DataModels.SchedulerRequestStatus.Completed,
                    request1After.Status,
                    "The request should be completed");
            }

            Assert.AreEqual(
                1,
                missions.Count(),
                "A mission should be generated");
            Assert.AreEqual(
                MissionType.Put,
                missions.First().Type,
                "A type of mission should be Put");
            Assert.AreEqual(
                MissionStatus.New,
                missions.First().Status,
                "A status of mission should be New");
            Assert.AreEqual(
                this.ItemVolume.Id,
                missions.First().ItemId,
                "The mission should be on the right item");
            Assert.AreEqual(
                compartment1.Id,
                missions.First().CompartmentId,
                "The mission should put on the right compartment");
            Assert.AreEqual(
                request1.RequestedQuantity,
                missions.First().RequestedQuantity,
                "The mission should take the full request quantity");

            #endregion
        }

        [TestMethod]
        public async Task CreateForRequestsAsync_SinglePutMissionIncompleteRequest()
        {
            #region Arrange

            var missionProvider = this.GetService<IMissionCreationProvider>();

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = this.ItemVolume.Id,
                CompartmentTypeId = compartmentType1.Id,
                MaxCapacity = 10,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                CompartmentTypeId = 1,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 5,
            };
            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = 1,
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                IsInstant = true,
                ItemId = this.ItemVolume.Id,
                OperationType = Common.DataModels.OperationType.Insertion,
                RequestedQuantity = 10,
                ReservedQuantity = 0,
                Priority = 2,
                Type = Common.DataModels.SchedulerRequestType.Item,
                Status = Common.DataModels.SchedulerRequestStatus.New,
            };

            using (var context = this.CreateContext())
            {
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
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
                var request1After = context.SchedulerRequests.First();

                Assert.AreEqual(
                    Common.DataModels.SchedulerRequestStatus.New,
                    request1After.Status,
                    "The request should not be completed");
                Assert.AreEqual(
                    itemCompartmentType1.MaxCapacity - compartment1.Stock,
                    request1After.ReservedQuantity,
                    "The reserved quantity should be equal to the available space");
            }

            Assert.AreEqual(
                1,
                missions.Count(),
                "A mission should be generated");
            Assert.AreEqual(
                MissionType.Put,
                missions.First().Type,
                "A type of mission should be Put");
            Assert.AreEqual(
                MissionStatus.New,
                missions.First().Status,
                "A status of mission should be New");
            Assert.AreEqual(
                this.ItemVolume.Id,
                missions.First().ItemId,
                "The mission should be on the right item");
            Assert.AreEqual(
                compartment1.Id,
                missions.First().CompartmentId,
                "The mission should put on the right compartment");
            Assert.AreEqual(
                itemCompartmentType1.MaxCapacity - compartment1.Stock,
                missions.First().RequestedQuantity,
                "The mission should take only a quantity equal to remaining capacity");

            #endregion
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        #endregion
    }
}
