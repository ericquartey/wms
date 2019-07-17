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
    public partial class CompartmentsControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task CreateAsync_WithNotifications()
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
                Depth = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = item1.Id,
                CompartmentTypeId = compartmentType1.Id,
            };
            var globalSettings = new Common.DataModels.GlobalSettings
            {
                MinStepCompartment = 5,
            };
            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.GlobalSettings.Add(globalSettings);
                context.SaveChanges();
            }

            var compartment1 = new CompartmentDetails
            {
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType1.Id,
                Depth = 10,
                Width = 10,
                XPosition = 0,
                YPosition = 0,
            };

            #endregion

            #region Act

            await controller.CreateAsync(compartment1);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(CompartmentDetails)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");
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
                        n => n.ModelId == item1.Id.ToString()
                            && n.ModelType == typeof(Item)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task CreateRangeAsync_WithNotifications()
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
                Depth = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = item1.Id,
                CompartmentTypeId = compartmentType1.Id,
            };
            var globalSettings = new Common.DataModels.GlobalSettings
            {
                MinStepCompartment = 5,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.GlobalSettings.Add(globalSettings);
                context.SaveChanges();
            }

            var compartment1 = new CompartmentDetails
            {
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType1.Id,
                Depth = 10,
                Width = 10,
                XPosition = 0,
                YPosition = 0,
            };
            var compartment2 = new CompartmentDetails
            {
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 20,
                CompartmentTypeId = compartmentType1.Id,
                Depth = 10,
                Width = 10,
                XPosition = 10,
                YPosition = 10,
            };

            #endregion

            #region Act

            await controller.CreateRangeAsync(new[] { compartment1, compartment2 });

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Count(
                        n => n.ModelType == typeof(CompartmentDetails)
                            && n.OperationType == HubEntityOperation.Created) == 2,
                "Many create notifications should be generated");
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
                        n => n.ModelId == item1.Id.ToString()
                            && n.ModelType == typeof(Item)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task DeleteAsync_WithNotifications()
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
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 0,
                CompartmentTypeId = compartmentType1.Id,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.DeleteAsync(compartment1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == compartment1.Id.ToString()
                            && n.ModelType == typeof(CompartmentDetails)
                            && n.OperationType == HubEntityOperation.Deleted),
                "A delete notification should be generated");
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
                        n => n.ModelId == item1.Id.ToString()
                            && n.ModelType == typeof(Item)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task UpdateAsync_WithNotifications()
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
                Depth = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = item1.Id,
                CompartmentTypeId = compartmentType1.Id,
                MaxCapacity = 30,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType1.Id,
            };
            var globalSettings = new Common.DataModels.GlobalSettings
            {
                MinStepCompartment = 5,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
                context.GlobalSettings.Add(globalSettings);
                context.SaveChanges();
            }

            var compartmentResult = await controller.GetByIdAsync(compartment1.Id);
            var compartmentToBeUpdated = (CompartmentDetails)((OkObjectResult)compartmentResult.Result).Value;
            compartmentToBeUpdated.Stock = 20;

            #endregion

            #region Act

            var actionResult = await controller.UpdateAsync(compartmentToBeUpdated, compartment1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == compartment1.Id.ToString()
                            && n.ModelType == typeof(CompartmentDetails)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
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
                        n => n.ModelId == item1.Id.ToString()
                            && n.ModelType == typeof(Item)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        #endregion
    }
}
