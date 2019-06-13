using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public class CellTypesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var cellType1 = new Common.DataModels.CellType { Id = 1, Description = "Cell Type #1" };
            var cellType2 = new Common.DataModels.CellType { Id = 2, Description = "Cell Type #2" };
            var cellType3 = new Common.DataModels.CellType { Id = 3, Description = "Cell Type #3" };
            var cellType4 = new Common.DataModels.CellType { Id = 4, Description = "Cell Type #4" };
            using (var context = this.CreateContext())
            {
                context.CellTypes.Add(cellType1);
                context.CellTypes.Add(cellType2);
                context.CellTypes.Add(cellType3);
                context.CellTypes.Add(cellType4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
            var result = (IEnumerable<CellType>)((OkObjectResult)actionResult.Result).Value;
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

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
            var result = (IEnumerable<CellType>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var cellType1 = new Common.DataModels.CellType { Id = 1, Description = "Cell Type #1" };
            var cellType2 = new Common.DataModels.CellType { Id = 2, Description = "Cell Type #2" };
            var cellType3 = new Common.DataModels.CellType { Id = 3, Description = "Cell Type #3" };
            var cellType4 = new Common.DataModels.CellType { Id = 4, Description = "Cell Type #4" };

            using (var context = this.CreateContext())
            {
                context.CellTypes.Add(cellType1);
                context.CellTypes.Add(cellType2);
                context.CellTypes.Add(cellType3);
                context.CellTypes.Add(cellType4);
                context.SaveChanges();
            }

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

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
            var result = (int)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result);

            #endregion
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public async Task GetByIdAsync_Found(int cellTypeId)
        {
            #region Arrange

            var controller = this.MockController();
            var cellType1 = new Common.DataModels.CellType { Id = 1, Description = "Cell Type #1" };
            var cellType2 = new Common.DataModels.CellType { Id = 2, Description = "Cell Type #2" };
            var cellType3 = new Common.DataModels.CellType { Id = 3, Description = "Cell Type #3" };
            var cellType4 = new Common.DataModels.CellType { Id = 4, Description = "Cell Type #4" };
            using (var context = this.CreateContext())
            {
                context.CellTypes.Add(cellType1);
                context.CellTypes.Add(cellType2);
                context.CellTypes.Add(cellType3);
                context.CellTypes.Add(cellType4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(cellTypeId);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
            var result = (CellType)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(cellTypeId, result.Id);

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

            Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

            #endregion
        }

        private CellTypesController MockController()
        {
            return new CellTypesController(
                new Mock<ILogger<CellTypesController>>().Object,
                this.ServiceProvider.GetService(typeof(ICellTypeProvider)) as ICellTypeProvider);
        }

        #endregion
    }
}
