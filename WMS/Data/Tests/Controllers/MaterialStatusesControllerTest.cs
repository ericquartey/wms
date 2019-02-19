using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Providers;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class MaterialStatusesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestMethod]
        public async Task GetAllCountFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var materialStatus1 = new Common.DataModels.MaterialStatus { Id = 1, Description = "Material Status #1" };
                var materialStatus2 = new Common.DataModels.MaterialStatus { Id = 2, Description = "Material Status #2" };
                var materialStatus3 = new Common.DataModels.MaterialStatus { Id = 3, Description = "Material Status #3" };
                var materialStatus4 = new Common.DataModels.MaterialStatus { Id = 4, Description = "Material Status #4" };
                context.MaterialStatuses.Add(materialStatus1);
                context.MaterialStatuses.Add(materialStatus2);
                context.MaterialStatuses.Add(materialStatus3);
                context.MaterialStatuses.Add(materialStatus4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllCountAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (int)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(4, result);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllCountNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetAllCountAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (int)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(0, result);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var materialStatus1 = new Common.DataModels.MaterialStatus { Id = 1, Description = "Material Status #1" };
                var materialStatus2 = new Common.DataModels.MaterialStatus { Id = 2, Description = "Material Status #2" };
                var materialStatus3 = new Common.DataModels.MaterialStatus { Id = 3, Description = "Material Status #3" };
                var materialStatus4 = new Common.DataModels.MaterialStatus { Id = 4, Description = "Material Status #4" };
                context.MaterialStatuses.Add(materialStatus1);
                context.MaterialStatuses.Add(materialStatus2);
                context.MaterialStatuses.Add(materialStatus3);
                context.MaterialStatuses.Add(materialStatus4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<MaterialStatus>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(4, result.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<MaterialStatus>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(0, result.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task GetByIdFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var materialStatus1 = new Common.DataModels.MaterialStatus { Id = 1, Description = "Material Status #1" };
                var materialStatus2 = new Common.DataModels.MaterialStatus { Id = 2, Description = "Material Status #2" };
                var materialStatus3 = new Common.DataModels.MaterialStatus { Id = 3, Description = "Material Status #3" };
                var materialStatus4 = new Common.DataModels.MaterialStatus { Id = 4, Description = "Material Status #4" };
                context.MaterialStatuses.Add(materialStatus1);
                context.MaterialStatuses.Add(materialStatus2);
                context.MaterialStatuses.Add(materialStatus3);
                context.MaterialStatuses.Add(materialStatus4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync(1);
                var actionResult2 = await controller.GetByIdAsync(2);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (MaterialStatus)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(1, result1.Id);

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (MaterialStatus)((OkObjectResult)actionResult2.Result).Value;
                Assert.AreEqual(2, result2.Id);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetByIdNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetByIdAsync(1);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion
            }
        }

        private MaterialStatusesController MockController()
        {
            return new MaterialStatusesController(
                new Mock<ILogger<MaterialStatusesController>>().Object,
                this.ServiceProvider.GetService(typeof(IMaterialStatusProvider)) as IMaterialStatusProvider);
        }

        #endregion
    }
}
