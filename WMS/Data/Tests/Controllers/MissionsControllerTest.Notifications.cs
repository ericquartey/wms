using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs.Models;
using Ferretto.WMS.Data.WebAPI.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    public partial class MissionsControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task AbortAsync_WithNotifications()
        {
            #region Arrange

            var controller = this.MockController();
            var notificationService =
                this.ServiceProvider.GetService(typeof(INotificationService)) as NotificationServiceMock;

            var item1 = new Common.DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = Common.DataModels.ItemManagementType.Volume,
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = item1.Id,
                CompartmentTypeId = compartmentType1.Id,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType1.Id,
            };

            var mission1 = new Common.DataModels.Mission
            {
                Id = 1,
                CompartmentId = compartment1.Id,
                ItemId = compartment1.ItemId,
                Status = Common.DataModels.MissionStatus.New,
                Type = Common.DataModels.MissionType.Pick,
                RequestedQuantity = 7
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

            await controller.AbortAsync(mission1.Id);

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
                        n => n.ModelId == compartment1.Id.ToString()
                            && n.ModelType == typeof(Compartment)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(MissionExecution)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task CompleteItemAsync_WithNotifications()
        {
            #region Arrange

            var controller = this.MockController();
            var notificationService =
                this.ServiceProvider.GetService(typeof(INotificationService)) as NotificationServiceMock;

            var item1 = new Common.DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = Common.DataModels.ItemManagementType.Volume,
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = item1.Id,
                CompartmentTypeId = compartmentType1.Id,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType1.Id,
            };

            var mission1 = new Common.DataModels.Mission
            {
                Id = 1,
                CompartmentId = compartment1.Id,
                ItemId = compartment1.ItemId,
                Status = Common.DataModels.MissionStatus.Executing,
                Type = Common.DataModels.MissionType.Pick,
                RequestedQuantity = 7
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

            await controller.CompleteItemAsync(mission1.Id, mission1.RequestedQuantity);

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
                        n => n.ModelId == compartment1.Id.ToString()
                            && n.ModelType == typeof(Compartment)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(MissionExecution)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task CompleteLoadingUnitAsync_WithNotifications()
        {
            #region Arrange

            var controller = this.MockController();
            var notificationService =
                this.ServiceProvider.GetService(typeof(INotificationService)) as NotificationServiceMock;

            var item1 = new Common.DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = Common.DataModels.ItemManagementType.Volume,
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = item1.Id,
                CompartmentTypeId = compartmentType1.Id,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType1.Id,
            };

            var mission1 = new Common.DataModels.Mission
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                Status = Common.DataModels.MissionStatus.Executing,
                Type = Common.DataModels.MissionType.Pick
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

            await controller.CompleteLoadingUnitAsync(mission1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == this.LoadingUnit1.Id.ToString()
                            && n.ModelType == typeof(LoadingUnit)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(MissionExecution)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task ExecuteAsync_WithNotifications()
        {
            #region Arrange

            var controller = this.MockController();
            var notificationService =
                this.ServiceProvider.GetService(typeof(INotificationService)) as NotificationServiceMock;

            var item1 = new Common.DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = Common.DataModels.ItemManagementType.Volume,
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = item1.Id,
                CompartmentTypeId = compartmentType1.Id,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType1.Id,
            };

            var mission1 = new Common.DataModels.Mission
            {
                Id = 1,
                CompartmentId = compartment1.Id,
                ItemId = compartment1.ItemId,
                Status = Common.DataModels.MissionStatus.New,
                Type = Common.DataModels.MissionType.Pick,
                RequestedQuantity = 7
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

            await controller.ExecuteAsync(mission1.Id);

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
                        n => n.ModelId == compartment1.Id.ToString()
                            && n.ModelType == typeof(Compartment)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(MissionExecution)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        #endregion
    }
}
