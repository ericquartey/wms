using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
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
    public class AreasControllerTest : BaseControllerTest
    {
        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestMethod]
        public async Task GetAllFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);
                var area1 = new Common.DataModels.Area { Id = 1, Name = "Area #1" };
                var area2 = new Common.DataModels.Area { Id = 2, Name = "Area #2" };
                var area3 = new Common.DataModels.Area { Id = 3, Name = "Area #3" };
                context.Areas.Add(area1);
                context.Areas.Add(area2);
                context.Areas.Add(area3);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<Area>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(3, result.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<Area>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(0, result.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task GetBaysFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);
                var area1 = new Common.DataModels.Area { Id = 1, Name = "Area #1" };
                var area2 = new Common.DataModels.Area { Id = 2, Name = "Area #2" };
                var bay1 = new Common.DataModels.Bay { Id = 1, Description = "Bay #1", AreaId = 1 };
                var bay2 = new Common.DataModels.Bay { Id = 2, Description = "Bay #2", AreaId = 2 };
                var bay3 = new Common.DataModels.Bay { Id = 3, Description = "Bay #3", AreaId = 2 };
                context.Areas.Add(area1);
                context.Areas.Add(area2);
                context.Bays.Add(bay1);
                context.Bays.Add(bay2);
                context.Bays.Add(bay3);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync(1);
                var actionResult2 = await controller.GetByIdAsync(2);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (Area)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(1, result1.Id);

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (Area)((OkObjectResult)actionResult2.Result).Value;
                Assert.AreEqual(2, result2.Id);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetBaysNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);
                var area1 = new Common.DataModels.Area { Id = 1, Name = "Area #1" };
                context.Areas.Add(area1);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetBays(1);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion
            }
        }

        [TestMethod]
        public async Task GetByIdFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);
                var area1 = new Common.DataModels.Area { Id = 1, Name = "Area #1" };
                var area2 = new Common.DataModels.Area { Id = 2, Name = "Area #2" };
                var area3 = new Common.DataModels.Area { Id = 3, Name = "Area #3" };
                context.Areas.Add(area1);
                context.Areas.Add(area2);
                context.Areas.Add(area3);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync(1);
                var actionResult2 = await controller.GetByIdAsync(2);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (Area)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(1, result1.Id);

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (Area)((OkObjectResult)actionResult2.Result).Value;
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

                var controller = MockController(context);

                #endregion

                #region Act

                var actionResult = await controller.GetByIdAsync(1);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion
            }
        }

        private static AreasController MockController(DatabaseContext context)
        {
            return new AreasController(
                new Mock<ILogger<AreasController>>().Object,
                new AreaProvider(context),
                new BayProvider(context));
        }

        #endregion
    }
}
