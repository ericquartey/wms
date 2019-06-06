using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class MaterialStatusesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var materialStatus1 = new Common.DataModels.MaterialStatus { Id = 1, Description = "Material Status #1" };
            var materialStatus2 = new Common.DataModels.MaterialStatus { Id = 2, Description = "Material Status #2" };
            var materialStatus3 = new Common.DataModels.MaterialStatus { Id = 3, Description = "Material Status #3" };
            var materialStatus4 = new Common.DataModels.MaterialStatus { Id = 4, Description = "Material Status #4" };
            using (var context = this.CreateContext())
            {
                context.MaterialStatuses.Add(materialStatus1);
                context.MaterialStatuses.Add(materialStatus2);
                context.MaterialStatuses.Add(materialStatus3);
                context.MaterialStatuses.Add(materialStatus4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<MaterialStatus>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(4, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllAsync_NotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<MaterialStatus>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var materialStatus1 = new Common.DataModels.MaterialStatus { Id = 1, Description = "Material Status #1" };
            var materialStatus2 = new Common.DataModels.MaterialStatus { Id = 2, Description = "Material Status #2" };
            var materialStatus3 = new Common.DataModels.MaterialStatus { Id = 3, Description = "Material Status #3" };
            var materialStatus4 = new Common.DataModels.MaterialStatus { Id = 4, Description = "Material Status #4" };
            using (var context = this.CreateContext())
            {
                context.MaterialStatuses.Add(materialStatus1);
                context.MaterialStatuses.Add(materialStatus2);
                context.MaterialStatuses.Add(materialStatus3);
                context.MaterialStatuses.Add(materialStatus4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllCountAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (int)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(4, result);

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_NotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetAllCountAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (int)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result);

            #endregion
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public async Task GetByIdAsync_Found(int materialStatusId)
        {
            #region Arrange

            var controller = this.MockController();
            var materialStatus1 = new Common.DataModels.MaterialStatus { Id = 1, Description = "Material Status #1" };
            var materialStatus2 = new Common.DataModels.MaterialStatus { Id = 2, Description = "Material Status #2" };
            var materialStatus3 = new Common.DataModels.MaterialStatus { Id = 3, Description = "Material Status #3" };
            var materialStatus4 = new Common.DataModels.MaterialStatus { Id = 4, Description = "Material Status #4" };

            using (var context = this.CreateContext())
            {
                context.MaterialStatuses.Add(materialStatus1);
                context.MaterialStatuses.Add(materialStatus2);
                context.MaterialStatuses.Add(materialStatus3);
                context.MaterialStatuses.Add(materialStatus4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(materialStatusId);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (MaterialStatus)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(materialStatusId, result.Id);

            #endregion
        }

        [TestMethod]
        public async Task GetByIdAsync_NotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(1);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult), GetDescription(actionResult.Result));

            #endregion
        }

        private MaterialStatusesController MockController()
        {
            return new MaterialStatusesController(
                new Mock<ILogger<MaterialStatusesController>>().Object,
                new Mock<IHubContext<DataHub, IDataHub>>().Object,
                this.ServiceProvider.GetService(typeof(IMaterialStatusProvider)) as IMaterialStatusProvider);
        }

        #endregion
    }
}
