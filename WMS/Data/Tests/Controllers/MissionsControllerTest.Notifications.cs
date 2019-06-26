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
    public partial class MissionsControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task CompleteLoadingUnitAsync_WithNotifications()
        {
            #region Arrange

            var controller = this.MockController();
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
                Id = GetNewId(),
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType1.Id,
            };

            var missionOperation1 = new Common.DataModels.MissionOperation
            {
                Id = GetNewId(),
                ItemId = item1.Id,
                Status = Common.DataModels.MissionOperationStatus.Executing,
                Type = Common.DataModels.MissionOperationType.Pick
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

            var actionResult = await controller.CompleteLoadingUnitAsync(mission1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));

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
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        #endregion
    }
}
