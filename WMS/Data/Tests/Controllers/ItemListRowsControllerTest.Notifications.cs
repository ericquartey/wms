using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Tests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    public partial class ItemListRowsControllerTest
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
            var itemList1 = new Common.DataModels.ItemList
            {
                Id = 1,
                Code = "Item List #1",
                Priority = 1,
                ItemListType = Common.DataModels.ItemListType.Pick,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemLists.Add(itemList1);
                context.SaveChanges();
            }

            var itemListRow1 = new ItemListRowDetails
            {
                Code = "Item List Row #1",
                Priority = 1,
                ItemListId = itemList1.Id,
                ItemId = item1.Id,
                RequestedQuantity = 1,
            };

            #endregion

            #region Act

            await controller.CreateAsync(itemListRow1);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(ItemListRowDetails)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == itemList1.Id.ToString()
                            && n.ModelType == typeof(ItemList)
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
            var itemList1 = new Common.DataModels.ItemList
            {
                Id = 1,
                Code = "Item List #1",
                Priority = 1,
                ItemListType = Common.DataModels.ItemListType.Pick,
            };
            var itemListRow1 = new Common.DataModels.ItemListRow
            {
                Id = 1,
                Code = "Item List Row #1",
                Priority = 1,
                ItemListId = itemList1.Id,
                ItemId = item1.Id,
                Status = Common.DataModels.ItemListRowStatus.New,
                RequestedQuantity = 1,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemLists.Add(itemList1);
                context.ItemListRows.Add(itemListRow1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.DeleteAsync(itemListRow1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == itemListRow1.Id.ToString()
                            && n.ModelType == typeof(ItemListRowDetails)
                            && n.OperationType == HubEntityOperation.Deleted),
                "A delete notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(ItemList)
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
            var itemList1 = new Common.DataModels.ItemList
            {
                Id = 1,
                Code = "Item List #1",
                Priority = 1,
                ItemListType = Common.DataModels.ItemListType.Pick,
            };
            var itemListRow1 = new Common.DataModels.ItemListRow
            {
                Id = 1,
                Code = "Item List Row #1",
                Priority = 1,
                ItemListId = itemList1.Id,
                ItemId = item1.Id,
                Status = Common.DataModels.ItemListRowStatus.New,
                RequestedQuantity = 1,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
                context.ItemLists.Add(itemList1);
                context.ItemListRows.Add(itemListRow1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.ExecuteAsync(itemListRow1.Id, this.Area1.Id, this.Bay1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == itemListRow1.Id.ToString()
                            && n.ModelType == typeof(ItemListRowOperation)
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
                        n => n.ModelId == itemList1.Id.ToString()
                            && n.ModelType == typeof(ItemListOperation)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(ItemListRowSchedulerRequest)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");

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
            var itemList1 = new Common.DataModels.ItemList
            {
                Id = 1,
                Code = "Item List #1",
                Priority = 1,
                ItemListType = Common.DataModels.ItemListType.Pick,
            };
            var itemListRow1 = new Common.DataModels.ItemListRow
            {
                Id = 1,
                Code = "Item List Row #1",
                Priority = 1,
                ItemListId = itemList1.Id,
                ItemId = item1.Id,
                Status = Common.DataModels.ItemListRowStatus.New,
                RequestedQuantity = 1,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
                context.ItemLists.Add(itemList1);
                context.ItemListRows.Add(itemListRow1);
                context.SaveChanges();
            }

            var itemListRowResult = await controller.GetByIdAsync(itemListRow1.Id);
            var itemListRowToBeUpdated = (ItemListRowDetails)((OkObjectResult)itemListRowResult.Result).Value;
            itemListRowToBeUpdated.Priority = 10;

            #endregion

            #region Act

            await controller.UpdateAsync(itemListRowToBeUpdated, itemListRow1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == itemListRow1.Id.ToString()
                            && n.ModelType == typeof(ItemListRowDetails)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == itemList1.Id.ToString()
                            && n.ModelType == typeof(ItemList)
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
