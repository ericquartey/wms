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
    public class CellStatusesControllerTest : BaseControllerTest
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

                var controller = MockController(context);
                var cellStatus1 = new Common.DataModels.CellStatus { Id = 1, Description = "Cell Status #1" };
                var cellStatus2 = new Common.DataModels.CellStatus { Id = 2, Description = "Cell Status #2" };
                var cellStatus3 = new Common.DataModels.CellStatus { Id = 3, Description = "Cell Status #3" };
                var cellStatus4 = new Common.DataModels.CellStatus { Id = 4, Description = "Cell Status #4" };
                context.CellStatuses.Add(cellStatus1);
                context.CellStatuses.Add(cellStatus2);
                context.CellStatuses.Add(cellStatus3);
                context.CellStatuses.Add(cellStatus4);
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

                var controller = MockController(context);

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

                var controller = MockController(context);
                var cellStatus1 = new Common.DataModels.CellStatus { Id = 1, Description = "Cell Status #1" };
                var cellStatus2 = new Common.DataModels.CellStatus { Id = 2, Description = "Cell Status #2" };
                var cellStatus3 = new Common.DataModels.CellStatus { Id = 3, Description = "Cell Status #3" };
                var cellStatus4 = new Common.DataModels.CellStatus { Id = 4, Description = "Cell Status #4" };
                context.CellStatuses.Add(cellStatus1);
                context.CellStatuses.Add(cellStatus2);
                context.CellStatuses.Add(cellStatus3);
                context.CellStatuses.Add(cellStatus4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<CellStatus>)((OkObjectResult)actionResult.Result).Value;
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

                var controller = MockController(context);

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<CellStatus>)((OkObjectResult)actionResult.Result).Value;
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

                var controller = MockController(context);
                var cellStatus1 = new Common.DataModels.CellStatus { Id = 1, Description = "Cell Status #1" };
                var cellStatus2 = new Common.DataModels.CellStatus { Id = 2, Description = "Cell Status #2" };
                var cellStatus3 = new Common.DataModels.CellStatus { Id = 3, Description = "Cell Status #3" };
                var cellStatus4 = new Common.DataModels.CellStatus { Id = 4, Description = "Cell Status #4" };
                context.CellStatuses.Add(cellStatus1);
                context.CellStatuses.Add(cellStatus2);
                context.CellStatuses.Add(cellStatus3);
                context.CellStatuses.Add(cellStatus4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync(1);
                var actionResult2 = await controller.GetByIdAsync(2);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (CellStatus)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(1, result1.Id);

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (CellStatus)((OkObjectResult)actionResult2.Result).Value;
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

        private static CellStatusesController MockController(DatabaseContext context)
        {
            return new CellStatusesController(
                new Mock<ILogger<CellStatusesController>>().Object,
                new CellStatusProvider(context));
        }

        #endregion
    }
}
