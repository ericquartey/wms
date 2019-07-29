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
    public partial class LoadingUnitsControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task CreateAsync_WithNotifications()
        {
            #region Arrange

            var controller = this.MockController();
            var notificationService =
                this.ServiceProvider.GetService(typeof(INotificationService)) as NotificationServiceMock;

            var loadingUnit1 = new LoadingUnitDetails
            {
                Code = "Loading Unit #999",
                CellId = this.Cell1.Id,
                LoadingUnitTypeId = this.LoadingUnitType1.Id,
                Weight = 100,
                AbcClassId = this.AbcClass1.Id,
                Height = 10,
                LoadingUnitStatusId = this.LoadingUnitStatus1.Id,
            };

            #endregion

            #region Act

            await controller.CreateAsync(loadingUnit1);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(LoadingUnitDetails)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == this.LoadingUnit1.Cell.Id.ToString()
                            && n.ModelType == typeof(Cell)
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

            var loadingUnit1 = new Common.DataModels.LoadingUnit
            {
                Id = 999,
                Code = "Loading Unit #999",
                CellId = this.Cell1.Id,
                LoadingUnitTypeId = this.LoadingUnitType1.Id,
                Weight = 100,
                AbcClassId = this.AbcClass1.Id,
                Height = 10,
                LoadingUnitStatusId = this.LoadingUnitStatus1.Id,
            };

            using (var context = this.CreateContext())
            {
                context.LoadingUnits.Add(loadingUnit1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            await controller.DeleteAsync(loadingUnit1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == loadingUnit1.Id.ToString()
                            && n.ModelType == typeof(LoadingUnitDetails)
                            && n.OperationType == HubEntityOperation.Deleted),
                "A delete notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == this.LoadingUnit1.Cell.Id.ToString()
                            && n.ModelType == typeof(Cell)
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

            var loadingUnitResult = await controller.GetByIdAsync(this.LoadingUnit1.Id);
            var loadingUnitToBeUpdated = (LoadingUnitDetails)((OkObjectResult)loadingUnitResult.Result).Value;
            loadingUnitToBeUpdated.Code = "Loading Unit #1 updated";

            #endregion

            #region Act

            await controller.UpdateAsync(loadingUnitToBeUpdated, this.LoadingUnit1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == this.LoadingUnit1.Id.ToString()
                            && n.ModelType == typeof(LoadingUnitDetails)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == this.LoadingUnit1.Cell.Id.ToString()
                            && n.ModelType == typeof(Cell)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        [TestMethod]
        public async Task WithdrawAsync_WithNotifications()
        {
            #region Arrange

            var controller = this.MockController();
            var notificationService =
                this.ServiceProvider.GetService(typeof(INotificationService)) as NotificationServiceMock;

            #endregion

            #region Act

            var actionResult = await controller.WithdrawAsync(this.LoadingUnit1.Id, this.Bay1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult), GetDescription(actionResult.Result));

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
                        n => n.ModelType == typeof(Mission)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");
            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelType == typeof(LoadingUnitSchedulerRequest)
                            && n.OperationType == HubEntityOperation.Created),
                "A create notification should be generated");

            #endregion
        }

        #endregion
    }
}
