using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs.Models;
using Ferretto.WMS.Data.WebAPI.Tests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    public partial class MissionOperationsControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task AbortAsync_WithNotifications()
        {
            var controller = this.ServiceProvider
                .GetService(typeof(MissionOperationsController)) as MissionOperationsController;

            var notificationService =
                this.ServiceProvider.GetService(typeof(INotificationService)) as NotificationServiceMock;

            var item = new Common.DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = Common.DataModels.ItemManagementType.Volume,
            };

            var compartmentType = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Depth = 10,
                Width = 10,
            };

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                ItemId = item.Id,
                CompartmentTypeId = compartmentType.Id,
            };

            var compartment = new Common.DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType.Id,
            };

            var missionOperation = new Common.DataModels.MissionOperation
            {
                Id = 1,
                Status = Common.DataModels.MissionOperationStatus.New,
                CompartmentId = compartment.Id,
                ItemId = item.Id,
                Type = Common.DataModels.MissionOperationType.Pick,
                RequestedQuantity = 7
            };

            var mission = new Common.DataModels.Mission
            {
                Id = 1,
                Operations = new[]
                {
                    missionOperation
                },
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item);
                context.CompartmentTypes.Add(compartmentType);
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.Compartments.Add(compartment);
                context.Missions.Add(mission);
                context.SaveChanges();
            }

            await controller.AbortAsync(mission.Id);

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == item.Id.ToString()
                            && n.ModelType == typeof(Item)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == compartment.Id.ToString()
                            && n.ModelType == typeof(CandidateCompartment)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(MissionOperation)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
        }

        [TestMethod]
        public async Task CompleteItemAsync_WithNotifications()
        {
            #region Arrange

            var controller = this.ServiceProvider
                .GetService(typeof(MissionOperationsController)) as MissionOperationsController;

            var notificationService =
                this.ServiceProvider.GetService(typeof(INotificationService)) as NotificationServiceMock;

            var item = new Common.DataModels.Item
            {
                Id = GetNewId(),
                Code = "Item #1",
                ManagementType = Common.DataModels.ItemManagementType.Volume,
            };

            var compartmentType = new Common.DataModels.CompartmentType
            {
                Id = GetNewId(),
                Depth = 10,
                Width = 10,
            };

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                ItemId = item.Id,
                CompartmentTypeId = compartmentType.Id,
            };

            var compartment = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType.Id,
            };

            var missionOperation = new Common.DataModels.MissionOperation
            {
                Id = GetNewId(),
                Status = Common.DataModels.MissionOperationStatus.Executing,
                CompartmentId = compartment.Id,
                ItemId = item.Id,
                Type = Common.DataModels.MissionOperationType.Pick,
                RequestedQuantity = 7
            };

            var mission = new Common.DataModels.Mission
            {
                Id = GetNewId(),
                Operations = new[] { missionOperation }
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item);
                context.CompartmentTypes.Add(compartmentType);
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.Compartments.Add(compartment);
                context.Missions.Add(mission);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.CompleteItemAsync(missionOperation.Id, missionOperation.RequestedQuantity);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == item.Id.ToString()
                            && n.ModelType == typeof(Item)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == compartment.Id.ToString()
                            && n.ModelType == typeof(CandidateCompartment)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(MissionOperation)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task ExecuteAsync_WithNotifications()
        {
            #region Arrange

            var controller = this.ServiceProvider
                .GetService(typeof(MissionOperationsController)) as MissionOperationsController;

            var notificationService =
                this.ServiceProvider.GetService(typeof(INotificationService)) as NotificationServiceMock;

            var item1 = new Common.DataModels.Item
            {
                Id = GetNewId(),
                Code = "Item #1",
                ManagementType = Common.DataModels.ItemManagementType.Volume,
            };

            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = GetNewId(),
                Depth = 10,
                Width = 10,
            };

            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = item1.Id,
                CompartmentTypeId = compartmentType1.Id,
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType1.Id,
            };

            var missionOperation1 = new Common.DataModels.MissionOperation
            {
                Id = GetNewId(),
                CompartmentId = compartment1.Id,
                ItemId = item1.Id,
                Status = Common.DataModels.MissionOperationStatus.New,
                Type = Common.DataModels.MissionOperationType.Pick,
                RequestedQuantity = 7
            };

            var mission1 = new Common.DataModels.Mission
            {
                Id = GetNewId(),
                LoadingUnitId = this.LoadingUnit1.Id,
                Operations = new[] { missionOperation1 }
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
                context.Missions.Add(mission1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.ExecuteAsync(missionOperation1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == item1.Id.ToString()
                            && n.ModelType == typeof(Item)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(MissionOperation)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        #endregion
    }
}
