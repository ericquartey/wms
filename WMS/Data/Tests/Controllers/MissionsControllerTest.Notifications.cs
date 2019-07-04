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

            var mission = new Common.DataModels.Mission
            {
                Id = GetNewId(),
                LoadingUnitId = this.LoadingUnit1.Id,
                Status = Common.DataModels.MissionStatus.Executing
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

            var actionResult = await controller.CompleteLoadingUnitAsync(mission.Id);

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
