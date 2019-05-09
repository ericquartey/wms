using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Ferretto.WMS.Data.WebAPI.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class BaysControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllBays()
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
                var resultBays = (IEnumerable<Bay>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(2, resultBays.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllCountFound()
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
                Assert.AreEqual(2, result);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetBayByIdFound()
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

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var resultBay = (Bay)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(1, resultBay.Id);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetBayByIdNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetByIdAsync(999);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion
            }
        }

        [TestMethod]
        public async Task GetMachineByBayId()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetByBayIdAsync(1);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var resultMachine = (Machine)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(1, resultMachine.Id);

                #endregion
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        private BaysController MockController()
        {
            return new BaysController(
                new Mock<ILogger<BaysController>>().Object,
                new Mock<IHubContext<SchedulerHub, ISchedulerHub>>().Object,
                this.ServiceProvider.GetService(typeof(IBayProvider)) as IBayProvider,
                this.ServiceProvider.GetService(typeof(IMachineProvider)) as IMachineProvider);
        }

        #endregion
    }
}
