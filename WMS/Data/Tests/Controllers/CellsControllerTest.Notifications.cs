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
    public partial class CellsControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task UpdateAsync_WithNotifications()
        {
            #region Arrange

            var controller = this.MockController();
            var notificationService =
                this.ServiceProvider.GetService(typeof(INotificationService)) as NotificationServiceMock;

            var cellResult = await controller.GetByIdAsync(this.Cell1.Id);
            var cellToBeUpdated = (CellDetails)((OkObjectResult)cellResult.Result).Value;
            cellToBeUpdated.Priority = 10;

            #endregion

            #region Act

            await controller.UpdateAsync(cellToBeUpdated, this.Cell1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(
                notificationService
                    .SentNotifications
                    .Any(
                        n => n.ModelId == this.Bay1.Id.ToString()
                            && n.ModelType == typeof(CellDetails)
                            && n.OperationType == HubEntityOperation.Updated),
                "An update notification should be generated");

            #endregion
        }

        #endregion
    }
}
