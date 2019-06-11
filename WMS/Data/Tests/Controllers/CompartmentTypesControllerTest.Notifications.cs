using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    public partial class CompartmentTypesControllerTest
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

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();
            }

            var compartmentType1 = new CompartmentType
            {
                Height = 10,
                Width = 10,
            };

            #endregion

            #region Act

            await controller.CreateAsync(compartmentType1, item1.Id, 100);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(CompartmentType)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");

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
                Height = 10,
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

            await controller.DeleteAsync(compartmentType1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == compartmentType1.Id.ToString()
                            && n.ModelType == typeof(CompartmentType)
                            && n.OperationType == HubEntityOperation.Deleted),
                "A delete notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task DeleteItemAssociationAsync_WithNotifications()
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

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.DeleteItemAssociationAsync(compartmentType1.Id, item1.Id);

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

        #endregion
    }
}
