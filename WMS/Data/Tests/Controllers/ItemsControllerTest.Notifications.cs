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
    public partial class ItemsControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task AddCompartmentTypeAssociationAsync_WithNotifications()
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

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.AddCompartmentTypeAssociationAsync(
                item1.Id,
                compartmentType1.Id,
                100);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(ItemCompartmentType)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(Item)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(CompartmentType)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task AddItemCompartmentTypesAsync_WithNotifications()
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
            var item2 = new Common.DataModels.Item
            {
                Id = 2,
                Code = "Item #2",
                ManagementType = Common.DataModels.ItemManagementType.FIFO,
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Depth = 10,
                Width = 10,
            };
            var compartmentType2 = new Common.DataModels.CompartmentType
            {
                Id = 2,
                Depth = 20,
                Width = 20,
            };
            var compartmentType3 = new Common.DataModels.CompartmentType
            {
                Id = 3,
                Depth = 30,
                Width = 30,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.Items.Add(item2);
                context.CompartmentTypes.Add(compartmentType1);
                context.CompartmentTypes.Add(compartmentType2);
                context.CompartmentTypes.Add(compartmentType3);
                context.SaveChanges();
            }

            var itemCompartmentTypesToAdd = new[]
            {
                new ItemCompartmentType { ItemId = item1.Id, CompartmentTypeId = compartmentType1.Id, MaxCapacity = 100 },
                new ItemCompartmentType { ItemId = item1.Id, CompartmentTypeId = compartmentType2.Id, MaxCapacity = 100 },
                new ItemCompartmentType { ItemId = item1.Id, CompartmentTypeId = compartmentType3.Id, MaxCapacity = 100 },
                new ItemCompartmentType { ItemId = item2.Id, CompartmentTypeId = compartmentType1.Id, MaxCapacity = 100 },
                new ItemCompartmentType { ItemId = item2.Id, CompartmentTypeId = compartmentType2.Id, MaxCapacity = 100 },
                new ItemCompartmentType { ItemId = item2.Id, CompartmentTypeId = compartmentType3.Id, MaxCapacity = 100 },
            };

            #endregion

            #region Act

            await controller.AddItemCompartmentTypesAsync(itemCompartmentTypesToAdd);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(ItemCompartmentType)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(Item)
                            && n.ModelId == item1.Id.ToString()
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(Item)
                            && n.ModelId == item2.Id.ToString()
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(CompartmentType)
                            && n.ModelId == compartmentType1.Id.ToString()
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(CompartmentType)
                            && n.ModelId == compartmentType2.Id.ToString()
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(CompartmentType)
                            && n.ModelId == compartmentType3.Id.ToString()
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task CreateAllowedAreaAsync_WithNotifications()
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

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.CreateAllowedAreaAsync(item1.Id, this.Area1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(ItemArea)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == this.Area1.Id.ToString()
                            && n.ModelType == typeof(Area)
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
        public async Task CreateAsync_WithNotifications()
        {
            #region Arrange

            var controller = this.MockController();
            var notificationService =
                this.ServiceProvider.GetService(typeof(INotificationService)) as NotificationServiceMock;

            var item1 = new ItemDetails
            {
                Code = "Item #1",
                ManagementType = ItemManagementType.Volume,
            };

            #endregion

            #region Act

            await controller.CreateAsync(item1);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(ItemDetails)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task DeleteAllowedAreaAsync_WithNotifications()
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
            var itemArea1 = new Common.DataModels.ItemArea
            {
                AreaId = this.Area1.Id,
                ItemId = item1.Id,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsAreas.Add(itemArea1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.DeleteAllowedAreaAsync(item1.Id, this.Area1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(AllowedItemArea)
                            && n.OperationType == HubEntityOperation.Deleted),
                "A delete notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == this.Area1.Id.ToString()
                            && n.ModelType == typeof(Area)
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

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.DeleteAsync(item1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == item1.Id.ToString()
                            && n.ModelType == typeof(ItemDetails)
                            && n.OperationType == HubEntityOperation.Deleted),
                "A delete notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task DeleteCompartmentTypeAssociationAsync_WithNotifications()
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

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.DeleteCompartmentTypeAssociationAsync(
                itemCompartmentType1.ItemId,
                itemCompartmentType1.CompartmentTypeId);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(ItemCompartmentType)
                            && n.OperationType == HubEntityOperation.Deleted),
                "A delete notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(Item)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(CompartmentType)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task PickAsync_WithNotifications()
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
                MaxCapacity = 100,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
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

            var options = new ItemOptions
            {
                AreaId = 1,
                BayId = 1,
                RequestedQuantity = 5,
                RunImmediately = true,
            };

            #endregion

            #region Act

            await controller.PickAsync(item1.Id, options);

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
                        n => n.ModelType == typeof(MissionExecution)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(ItemSchedulerRequest)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task PutAsync_WithNotifications()
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
            var itemArea1 = new Common.DataModels.ItemArea
            {
                AreaId = this.Area1.Id,
                ItemId = item1.Id,
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
                MaxCapacity = 100,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType1.Id,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsAreas.Add(itemArea1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            var options = new ItemOptions
            {
                AreaId = 1,
                BayId = 1,
                RequestedQuantity = 5,
                RunImmediately = true,
            };

            #endregion

            #region Act

            await controller.PutAsync(item1.Id, options);

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
                        n => n.ModelType == typeof(MissionExecution)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(ItemSchedulerRequest)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task UpdateCompartmentTypeAssociationAsync_WithNotifications()
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

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.UpdateCompartmentTypeAssociationAsync(
                itemCompartmentType1.ItemId,
                itemCompartmentType1.CompartmentTypeId,
                150);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(ItemCompartmentType)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(Item)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(CompartmentType)
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

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();
            }

            var itemResult = await controller.GetByIdAsync(item1.Id);
            var itemToBeUpdated = (ItemDetails)((OkObjectResult)itemResult.Result).Value;
            itemToBeUpdated.Code = "Item #1 updated";

            #endregion

            #region Act

            await controller.UpdateAsync(itemToBeUpdated, item1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == item1.Id.ToString()
                            && n.ModelType == typeof(ItemDetails)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        #endregion
    }
}
