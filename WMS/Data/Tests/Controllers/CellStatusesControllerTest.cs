using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public class CellStatusesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var cellStatus1 = new Common.DataModels.CellStatus { Id = 1, Description = "Cell Status #1" };
            var cellStatus2 = new Common.DataModels.CellStatus { Id = 2, Description = "Cell Status #2" };
            var cellStatus3 = new Common.DataModels.CellStatus { Id = 3, Description = "Cell Status #3" };
            var cellStatus4 = new Common.DataModels.CellStatus { Id = 4, Description = "Cell Status #4" };
            using (var context = this.CreateContext())
            {
                context.CellStatuses.Add(cellStatus1);
                context.CellStatuses.Add(cellStatus2);
                context.CellStatuses.Add(cellStatus3);
                context.CellStatuses.Add(cellStatus4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<CellStatus>)((OkObjectResult)actionResult.Result).Value;
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
            var result = (IEnumerable<CellStatus>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var cellStatus1 = new Common.DataModels.CellStatus { Id = 1, Description = "Cell Status #1" };
            var cellStatus2 = new Common.DataModels.CellStatus { Id = 2, Description = "Cell Status #2" };
            var cellStatus3 = new Common.DataModels.CellStatus { Id = 3, Description = "Cell Status #3" };
            var cellStatus4 = new Common.DataModels.CellStatus { Id = 4, Description = "Cell Status #4" };

            using (var context = this.CreateContext())
            {
                context.CellStatuses.Add(cellStatus1);
                context.CellStatuses.Add(cellStatus2);
                context.CellStatuses.Add(cellStatus3);
                context.CellStatuses.Add(cellStatus4);
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
        public async Task GetByIdAsync_Found(int cellStatusId)
        {
            #region Arrange

            var controller = this.MockController();
            var cellStatus1 = new Common.DataModels.CellStatus { Id = 1, Description = "Cell Status #1" };
            var cellStatus2 = new Common.DataModels.CellStatus { Id = 2, Description = "Cell Status #2" };
            var cellStatus3 = new Common.DataModels.CellStatus { Id = 3, Description = "Cell Status #3" };
            var cellStatus4 = new Common.DataModels.CellStatus { Id = 4, Description = "Cell Status #4" };

            using (var context = this.CreateContext())
            {
                context.CellStatuses.Add(cellStatus1);
                context.CellStatuses.Add(cellStatus2);
                context.CellStatuses.Add(cellStatus3);
                context.CellStatuses.Add(cellStatus4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(cellStatusId);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));

            var result1 = (CellStatus)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(cellStatusId, result1.Id);

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

        private CellStatusesController MockController()
        {
            return new CellStatusesController(
                new Mock<ILogger<CellStatusesController>>().Object,
                this.ServiceProvider.GetService(typeof(ICellStatusProvider)) as ICellStatusProvider);
        }

        #endregion
    }
}
