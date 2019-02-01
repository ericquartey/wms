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
    public class BaysControllerTest : BaseControllerTest
    {
        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestMethod]
        public async Task GetAllBays()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);

                #endregion Arrange

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion Act

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var resultBays = (IEnumerable<Bay>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(2, resultBays.Count());

                #endregion Assert
            }
        }

        [TestMethod]
        public async Task GetBayByIdFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);

                #endregion Arrange

                #region Act

                var actionResult = await controller.GetByIdAsync(1);

                #endregion Act

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var resultBay = (Bay)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(1, resultBay.Id);

                #endregion Assert
            }
        }

        [TestMethod]
        public async Task GetBayByIdNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);

                #endregion Arrange

                #region Act

                var actionResult = await controller.GetByIdAsync(999);

                #endregion Act

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion Assert
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        private static BaysController MockController(DatabaseContext context)
        {
            return new BaysController(
                new Mock<ILogger<BaysController>>().Object,
                new BayProvider(context));
        }

        #endregion Methods
    }
}
