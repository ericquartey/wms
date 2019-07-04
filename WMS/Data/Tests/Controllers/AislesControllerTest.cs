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
    public class AislesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
            var result = (IEnumerable<Aisle>)((OkObjectResult)actionResult.Result).Value;
            Assert.IsNotNull(result.SingleOrDefault(a => a.Id == this.Aisle1.Id));
            Assert.IsNotNull(result.SingleOrDefault(a => a.Id == this.Aisle2.Id));
            Assert.IsNotNull(result.SingleOrDefault(a => a.Id == this.Aisle3.Id));

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_Found()
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
            Assert.AreEqual(3, result);

            #endregion
        }

        [TestMethod]
        public async Task GetByIdAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(this.Aisle1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
            var result = (Aisle)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(this.Aisle1.Id, result.Id);

            #endregion
        }

        [TestMethod]
        public async Task GetByIdAsync_NotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(99);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

            #endregion
        }

        [TestMethod]
        public async Task GetCellsAsync_Nominal()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult1 = await controller.GetCellsAsync(this.Aisle1.Id);
            var actionResult2 = await controller.GetCellsAsync(this.Aisle2.Id);
            var actionResult3 = await controller.GetCellsAsync(this.Aisle3.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
            var result1 = ((OkObjectResult)actionResult1.Result).Value as IEnumerable<Cell>;
            Assert.IsNotNull(result1.SingleOrDefault(c => c.Id == this.Cell1.Id));
            Assert.IsNotNull(result1.SingleOrDefault(c => c.Id == this.Cell2.Id));

            Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
            var result2 = ((OkObjectResult)actionResult2.Result).Value as IEnumerable<Cell>;
            Assert.IsNotNull(result2.SingleOrDefault(c => c.Id == this.Cell3.Id));
            Assert.IsNotNull(result2.SingleOrDefault(c => c.Id == this.Cell4.Id));
            Assert.IsNotNull(result2.SingleOrDefault(c => c.Id == this.Cell5.Id));
            Assert.IsNotNull(result2.SingleOrDefault(c => c.Id == this.Cell6.Id));

            Assert.IsInstanceOfType(actionResult3.Result, typeof(OkObjectResult));
            var result3 = ((OkObjectResult)actionResult3.Result).Value as IEnumerable<Cell>;
            Assert.AreEqual(0, result3.Count());

            #endregion
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        private AislesController MockController()
        {
            return new AislesController(
                 new Mock<ILogger<AislesController>>().Object,
                 this.ServiceProvider.GetService(typeof(IAisleProvider)) as IAisleProvider,
                 this.ServiceProvider.GetService(typeof(ILoadingUnitProvider)) as ILoadingUnitProvider,
                 this.ServiceProvider.GetService(typeof(ICellProvider)) as ICellProvider);
        }

        #endregion
    }
}
