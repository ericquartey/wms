using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Scheduler.Tests
{
    [TestClass]
    public partial class MissionOperationCreationProviderTest : BaseWarehouseTest
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

            var operationProvider = this.GetService<IMissionOperationCreationProvider>();

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = GetNewId(),
                Depth = 10,
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
                Id = GetNewId(),
                CompartmentTypeId = compartmentType1.Id,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 4,
            };
            var compartment2 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                CompartmentTypeId = compartmentType1.Id,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 6,
            };
            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = GetNewId(),
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                IsInstant = true,
                ItemId = this.ItemVolume.Id,
                OperationType = Common.DataModels.OperationType.Put,
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

            var operations = await operationProvider.CreateForRequestsAsync(requests);

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
                operations.Count(),
                "Two operations should be generated");
            Assert.IsTrue(
                operations.Any(o => o.CompartmentId == compartment1.Id),
                "We should generate an operation for each compartment");
            Assert.IsTrue(
                operations.Any(o => o.CompartmentId == compartment2.Id),
                "We should generate an operation for each compartment");
            Assert.AreEqual(
                request1.RequestedQuantity,
                operations.Sum(o => o.RequestedQuantity),
                "The operation should take the full request quantity");
            Assert.IsTrue(
                operations.All(o => o.Type == MissionOperationType.Put),
                "A type of operation should be Put");
            Assert.IsTrue(
                operations.All(o => o.Status == MissionOperationStatus.New),
                "A status of operation should be New");
            Assert.IsTrue(
                operations.All(o => o.ItemId == this.ItemVolume.Id),
                "The operation should be on the right item");

            #endregion
        }

        [TestMethod]
        public async Task CreateForRequestsAsync_SinglePutMissionCompleteRequest()
        {
            #region Arrange

            var operationProvider = this.GetService<IMissionOperationCreationProvider>();

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = GetNewId(),
                Depth = 10,
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
                Id = GetNewId(),
                CompartmentTypeId = compartmentType1.Id,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 0,
            };
            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = GetNewId(),
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                IsInstant = true,
                ItemId = this.ItemVolume.Id,
                OperationType = Common.DataModels.OperationType.Put,
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

            var operations = await operationProvider.CreateForRequestsAsync(requests);

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
                operations.Count(),
                "A mission should be generated");
            Assert.AreEqual(
                MissionOperationType.Put,
                operations.First().Type,
                "A type of mission should be Put");
            Assert.AreEqual(
                MissionOperationStatus.New,
                operations.First().Status,
                "A status of mission should be New");
            Assert.AreEqual(
                this.ItemVolume.Id,
                operations.First().ItemId,
                "The mission should be on the right item");
            Assert.AreEqual(
                compartment1.Id,
                operations.First().CompartmentId,
                "The mission should put on the right compartment");
            Assert.AreEqual(
                request1.RequestedQuantity,
                operations.First().RequestedQuantity,
                "The mission should take the full request quantity");

            #endregion
        }

        [TestMethod]
        public async Task CreateForRequestsAsync_SinglePutMissionIncompleteRequest()
        {
            #region Arrange

            var operationProvider = this.GetService<IMissionOperationCreationProvider>();

            var requestExecutionProvider = this.GetService<ISchedulerRequestExecutionProvider>();

            var compartmentType = new Common.DataModels.CompartmentType
            {
                Id = GetNewId(),
                Depth = 10,
                Width = 10,
            };

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                ItemId = this.ItemVolume.Id,
                CompartmentTypeId = compartmentType.Id,
                MaxCapacity = 10,
            };

            var compartment = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                CompartmentTypeId = compartmentType.Id,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 5,
            };

            var request = new Common.DataModels.SchedulerRequest
            {
                Id = GetNewId(),
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                IsInstant = true,
                ItemId = this.ItemVolume.Id,
                OperationType = Common.DataModels.OperationType.Put,
                RequestedQuantity = 10,
                ReservedQuantity = 0,
                Priority = 2,
                Type = Common.DataModels.SchedulerRequestType.Item,
                Status = Common.DataModels.SchedulerRequestStatus.New,
            };

            using (var context = this.CreateContext())
            {
                context.CompartmentTypes.Add(compartmentType);
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.Compartments.Add(compartment);
                context.SchedulerRequests.Add(request);
                context.SaveChanges();
            }

            var requests = await requestExecutionProvider.GetRequestsToProcessAsync();

            #endregion

            #region Act

            var operations = await operationProvider.CreateForRequestsAsync(requests);

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
                    itemCompartmentType.MaxCapacity - compartment.Stock,
                    request1After.ReservedQuantity,
                    "The reserved quantity should be equal to the available space");
            }

            Assert.AreEqual(
                1,
                operations.Count(),
                "A mission should be generated");
            Assert.AreEqual(
                MissionOperationType.Put,
                operations.First().Type,
                "A type of mission should be Put");
            Assert.AreEqual(
                MissionOperationStatus.New,
                operations.First().Status,
                "A status of mission should be New");
            Assert.AreEqual(
                this.ItemVolume.Id,
                operations.First().ItemId,
                "The mission should be on the right item");
            Assert.AreEqual(
                compartment.Id,
                operations.First().CompartmentId,
                "The mission should put on the right compartment");
            Assert.AreEqual(
                itemCompartmentType.MaxCapacity - compartment.Stock,
                operations.First().RequestedQuantity,
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
